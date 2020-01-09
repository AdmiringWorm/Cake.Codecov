#load "nuget:https://ci.appveyor.com/nuget/cake-recipe-pylg5x5ru9c2?package=Cake.Recipe&prerelease&version=0.3.0-alpha0500"
#load "./.build/*.cake"
#tool "nuget:?package=Codecov&version=1.7.1"
#addin "nuget:?package=Cake.Coverlet&version=2.3.4"

Environment.SetVariableNames();

BuildParameters.SetParameters(
                            context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./Source",
                            title: "Cake.Codecov",
                            repositoryOwner: "cake-contrib",
                            repositoryName: "Cake.Codecov",
                            appVeyorAccountName: "cakecontrib",
                            shouldRunDotNetCorePack: true,
                            shouldBuildNugetSourcePackage: false,
                            shouldExecuteGitLink: false,
                            shouldGenerateDocumentation: false,
                            shouldRunCodecov: true,
                            shouldRunGitVersion: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(
                            context: Context,
                            dupFinderExcludePattern: new string[] {
                                BuildParameters.RootDirectoryPath + "/Source/Cake.Codecov.Tests/*.cs"
                            },
                            dupFinderExcludeFilesByStartingCommentSubstring: new string[] {
                                "<auto-generated>"
                            },
                            testCoverageFilter: "+[Cake.Codecov]*",
                            frameworkPathApiVersion: "4.5");

// Tasks we want to override
((CakeTask)BuildParameters.Tasks.UploadCodecovReportTask.Task).Actions.Clear();
((CakeTask)BuildParameters.Tasks.UploadCodecovReportTask.Task).Criterias.Clear();
BuildParameters.Tasks.UploadCodecovReportTask
    .IsDependentOn("DotNetCore-Pack")
    /*.WithCriteria(() => FileExists(BuildParameters.Paths.Files.TestCoverageOutputFilePath))
    .WithCriteria(() => BuildParameters.IsMainRepository)*/
    .Does(() => {
        var nugetPkg = $"nuget:file://{MakeAbsolute(BuildParameters.Paths.Directories.NuGetPackages)}?package=Cake.Codecov&version={BuildParameters.Version.SemVersion}&prepelease";
        Information("PATH: " + nugetPkg);

        var coverageFilter = BuildParameters.Paths.Files.TestCoverageOutputFilePath.ToString().Replace(".xml", "*.xml");
        Information($"Passing coverage filter to codecov: {coverageFilter}");

        var script = string.Format(@"#addin ""{0}""
Codecov(new CodecovSettings {{
    Files = new[] {{ ""{1}"" }},
    Root = ""{2}"",
    Required = true,
    EnvironmentVariables = new Dictionary<string,string> {{ {{ ""APPVEYOR_BUILD_VERSION"", EnvironmentVariable(""TEMP_BUILD_VERSION"") }} }}
}});",
            nugetPkg, coverageFilter, BuildParameters.RootDirectoryPath);
        RequireAddin(script, new Dictionary<string,string> {
            { "TEMP_BUILD_VERSION", BuildParameters.Version.FullSemVersion + ".build." + BuildSystem.AppVeyor.Environment.Build.Number }
            });

});

// Enable drafting a release when running on the master branch
if (BuildParameters.IsRunningOnAppVeyor &&
    BuildParameters.IsMainRepository && BuildParameters.IsMasterBranch && !BuildParameters.IsTagged)
{
    BuildParameters.Tasks.AppVeyorTask.IsDependentOn("Create-Release-Notes");
}

Task("Unix")
    .IsDependentOn("Package")
    .IsDependentOn("Upload-Coverage-Report");

Task("Appveyor-Unix")
    .IsDependentOn("Unix")
    .IsDependentOn("Upload-AppVeyor-Artifacts");


Build.RunDotNetCore();
