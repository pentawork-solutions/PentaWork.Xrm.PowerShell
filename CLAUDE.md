# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

This repo has an unusual split: source lives in `src/`, but the test projects referenced by the solution live in a sibling `tests/` directory at the repo root (not under `src/`). Both are wired together by `src/PentaWork.Xrm.PowerShell.sln`.

```
PentaWork.Xrm.PowerShell/       (repo root)
├── src/
│   ├── PentaWork.Xrm.PowerShell.sln
│   ├── PentaWork.Xrm.PowerShell/           # the shipped PowerShell module
│   └── PentaWork.Xrm.PluginGraphAnalyzer/  # plugin call-graph analysis engine (beta), referenced by the module project
├── tests/
│   ├── PentaWork.Xrm.PluginGraphAnalyzer.Tests/   # actual unit tests for the analyzer
│   ├── PentaWork.Xrm.Tests.Plugins/               # sample IPlugin implementations — analysis *fixtures*, not real tests
│   └── PentaWork.Xrm.Tests.PluginsTestSideAssembly/ # supporting types referenced by the fixtures above
├── build.cake                    # Cake build script (Clean → Restore → Build → Dist)
├── ps-build.ps1                  # `dotnet tool install Cake.Tool --global && dotnet cake`
└── dist/PentaWork.Xrm.PowerShell/  # build output: dll, psd1, help xml
```

Both projects target `net462` (Windows-only, .NET Framework), so building requires MSBuild/Visual Studio build tools, not just the `dotnet` SDK CLI.

## Common commands

Build everything (from repo root) — mirrors CI/release packaging:
```
./ps-build.ps1
# equivalent to: dotnet tool install Cake.Tool --global && dotnet cake
# runs Clean -> NuGetRestore -> DotNetBuild(sln) -> copy dll/psd1/help-xml into dist/PentaWork.Xrm.PowerShell
```

Build just the solution (from `src/`):
```
dotnet build PentaWork.Xrm.PowerShell.sln -c Release
```

Run tests (test projects live under `../tests/` relative to `src/`, referenced by the .sln):
```
dotnet test PentaWork.Xrm.PowerShell.sln
```
Only `PentaWork.Xrm.PluginGraphAnalyzer.Tests` contains actual test classes. `PentaWork.Xrm.Tests.Plugins` and `PentaWork.Xrm.Tests.PluginsTestSideAssembly` are sample plugin/support code compiled into fixture DLLs that the analyzer tests load and inspect at runtime — they are not meant to pass/fail themselves.

Load the built module interactively (see `Properties/launchSettings.json`):
```
Import-Module '.\PentaWork.Xrm.PowerShell.dll'
```

T4 templates (`.tt` files under `XrmProxies/Templates/**` and `PluginGraphAnalyzer/Templates/MainTemplate.tt`) are pre-compiled and their generated `.cs` output is committed to git. Editing a `.tt` file requires Visual Studio (or another `TextTemplatingFilePreprocessor`-capable host) to regenerate the corresponding `.cs` — running `dotnet build` alone will not re-run T4 transforms.

## Architecture

### PentaWork.Xrm.PowerShell (the module)

A PowerShell binary module (`PentaWork.Xrm.PowerShell.psd1` lists `CmdletsToExport`) exposing cmdlets for Dynamics 365 / Power Platform automation: solution export/import, entity export/import, sharings, relations, webresource/assembly updates, proxy generation, etc.

- Every cmdlet in `Verbs/*.cs` is a plain `PSCmdlet` subclass (no shared custom base class) implementing `ProcessRecord()`. The universal convention is a mandatory, pipeline-bindable `CrmServiceClient Connection` parameter, meant to be piped in from an external `Get-CrmConnection` call — the module never creates connections itself.
- CRM access goes through extension methods on `CrmServiceClient` in `CrmServiceClientExtensions.cs` (namespace `PentaWork.Xrm.PowerShell.Common`) — e.g. `Query(QueryExpression, getAll)` (handles paging), `QueryEntity`, `TryRetrieve`, `GetMetadata`, `AddToSolution`. Cmdlets call these rather than the raw SDK client directly.
- `SolutionComponentTypes.cs` is an enum mapping Dataverse solution-component type codes (Entity=1, Attribute=2, Form=24, Workflow=29, ...) to names, used by `AddToSolution`.

**Proxy generation (`Get-XrmProxies`, `Verbs/GetXrmProxies.cs`)** is the most involved cmdlet — a code generator producing C#/TypeScript proxy and FakeXrmEasy fake classes from live CRM metadata:
1. Queries CRM for allowed SDK messages, custom actions, full entity metadata (`RetrieveAllEntitiesRequest`), and system forms.
2. Wraps the raw SDK metadata into a cross-referenced POCO graph under `XrmProxies/Model/` (`EntityInfoList` → `EntityInfo` → `AttributeInfo`/`OptionSetInfo`/`OneToManyRelationInfo`/`ManyToManyRelationInfo`/`FormInfo`/`ActionInfo`).
3. Feeds those model objects into T4 templates under `XrmProxies/Templates/CSharp/*.tt` and `XrmProxies/Templates/Javascript/*.tt` (each compiles to a partial class with settable properties and `TransformText()`), instantiated once per entity (and per many-to-many relation group).
4. Writes the rendered output to `OutputPath/CSharp/{Entities,Relations,Extensions}`, `OutputPath/TS/{Entities,Attributes,FormInfos}`, and `OutputPath/Fake`.

When touching proxy generation, the model classes in `XrmProxies/Model/` and the `.tt` templates need to stay in sync — the templates assume specific shapes/properties on the model objects.

### PentaWork.Xrm.PluginGraphAnalyzer (beta, referenced by the module via `Export-PluginGraphs`)

Analyzes compiled Dataverse plugin assemblies to reconstruct which SDK operations each plugin's `Execute` method performs (Create/Update/Delete/Execute calls, entities/fields touched), cross-referenced with the plugin's registration metadata (message, stage, sync/async, order) pulled from the target environment, and renders the result as Markdown (with Mermaid diagrams) — one file per entity/message.

- `PluginGraphAnalyzer.cs` downloads plugin assemblies/packages via `CrmServiceClient`, loads them with **dnlib**, and locates each plugin type's `Execute` method (including inherited base-class methods).
- `PluginGraphVM.cs` is a custom IL interpreter: it walks a method's instructions on an emulated evaluation stack (per-call `StorageFrame` in `Model/StorageFrame.cs`), following calls into other methods within configured namespaces, with recursion/call-loop detection.
- `Hooks/IHook.cs` defines the interception interface (`HookApplicable`/`ExecuteHook`); implementations are discovered by reflection. `Hooks/Calls/*` intercepts specific known SDK method calls (matched by signature) — e.g. `IOrganizationService.Create`, entity attribute getters — to record a semantically meaningful `XrmApiCall` instead of generically interpreting the IL. `Hooks/Ctors/*` intercepts `newobj` for known SDK types (e.g. `new Entity("account")`) to seed emulated object state.
- `Model/VMObjects/*` are the emulated runtime objects the VM pushes/pops on its stack in place of real SDK instances (`EntityObj`, `ServiceContextObj`, `ExecutionContextObj`, `XrmApiCall`, ...).
- `Model/XrmInfoObjects/*` hold plugin registration metadata read from Dataverse (assemblies, packages, steps, stages).
- `Model/GraphObjects/*` (`EntityGraph`, `EntityGraphList`, `MessageGraph`) are the output-facing graph grouping plugin steps/API calls per entity or message; `EntityGraph.ToMarkdown()` drives `Templates/MainTemplate.tt` to render the final report.
- This project has its own separate copy of `CrmServiceClientExtensions.cs` under `Extensions/` (different namespace, not shared with the module's copy) — a known duplication rather than an intentional split; keep both in sync manually if fixing a paging/query bug in one.

The test fixtures under `tests/PentaWork.Xrm.Tests.Plugins` and `tests/PentaWork.Xrm.Tests.PluginsTestSideAssembly` are real, compiled `IPlugin` implementations used purely as analysis input — `tests/PentaWork.Xrm.PluginGraphAnalyzer.Tests` loads their built DLLs via dnlib and asserts on the `XrmApiCall`s the analyzer extracts from them.
