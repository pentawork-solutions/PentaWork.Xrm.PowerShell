#tool nuget:?package=NuGet.CommandLine&version=5.9.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Dist");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// VARIABLES
//////////////////////////////////////////////////////////////////////

var distDir = $"./dist/PentaWork.Xrm.PowerShell";
var solution = $"./src/PentaWork.Xrm.PowerShell.sln";
var buildDir = $"./src/PentaWork.Xrm.PowerShell/bin/{configuration}";

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() => {
        CleanDirectory(distDir);
        CleanDirectory(buildDir);
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => { 
        NuGetRestore(solution);
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => {
        DotNetBuild(solution, new DotNetBuildSettings
        {
            Configuration = configuration,
        });
    });

Task("Dist")
    .IsDependentOn("Build")
    .Does(() => {
        CreateDirectory(distDir);
        CopyFiles(buildDir + "/net462/*.dll", distDir);
        CopyFiles(buildDir + "/net462/*.psd1", distDir);
        CopyFiles(buildDir + "/net462/*-Help.xml", distDir);        
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);