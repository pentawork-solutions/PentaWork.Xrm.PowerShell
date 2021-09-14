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
        MSBuild(solution, settings => settings.SetConfiguration(configuration));
    });

Task("Dist")
    .IsDependentOn("Build")
    .Does(() => {
        CreateDirectory(distDir);
        CopyFiles(buildDir + "/*.dll", distDir);
        CopyFiles(buildDir + "/*.psd1", distDir);
        CopyFiles(buildDir + "/*-Help.xml", distDir);        
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);