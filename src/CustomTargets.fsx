open Fake

let testDirectory = getBuildParamOrDefault "buildMode" "Debug"
let nUnitRunner = "nunit3-console.exe"
let mutable nUnitToolPath = @"tools\NUnit.ConsoleRunner\"
let acceptanceTestPlayList = getBuildParamOrDefault "playList" ""
let nunitTestFormat = getBuildParamOrDefault "nunitTestFormat" "nunit2"
let rootPublishDirectory = getBuildParamOrDefault "publishDirectory"  @"C:\CompiledSource"
let mutable projectName = ""
let mutable publishDirectory = rootPublishDirectory @@ projectName

Target "Build And Zip Webjob Host Projects" ( fun _ ->
    let buildMode = getBuildParamOrDefault "buildMode" "Debug"

    if buildMode.ToLower().Equals("release") then
        let directoryinfo = FileSystemHelper.directoryInfo(@".\" @@ publishDirectory @@ "\..\Host")
        let directory = directoryinfo.FullName
        traceImportant directory
        let properties =
                        [
                            ("DeployOnBuild", "True");
                            ("WebPublishMethod", "Package");
                            ("PackageAsSingleFile", "True");
                            ("SkipInvalidConfigurations", "true");
                            ("PackageLocation", directory);
                            ("ToolsVersion","14");
                        ]

        !! (@".\**\*.Host.csproj")
            |> MSBuildReleaseExt null properties "Build"
            |> Log "Build-Output: "

        let portalProject = "./SFA.DAS.EAS.Portal.Worker/SFA.DAS.EAS.Portal.Worker.csproj"
        DotNetCli.Publish(fun p ->
            { p with
                Project = portalProject
                Output = directory})
)

Target "Build And Zip Web App Projects" ( fun _ ->
    let buildMode = getBuildParamOrDefault "buildMode" "Debug"

    if buildMode.ToLower().Equals("release") then
        let directoryinfo = FileSystemHelper.directoryInfo(@".\" @@ publishDirectory @@ "\..\WebApps")
        let directory = directoryinfo.FullName
        traceImportant directory
        let properties =
                        [
                            ("DeployOnBuild", "True");
                            ("WebPublishMethod", "Package");
                            ("PackageAsSingleFile", "True");
                            ("SkipInvalidConfigurations", "true");
                            ("PackageLocation", directory);
                            ("ToolsVersion","14");
                        ]

        !! (@".\**\SFA.DAS.EmployerFinance.Web.csproj")
            |> MSBuildReleaseExt null properties "Build"
            |> Log "Build-Output: "

        !! (@".\**\SFA.DAS.EmployerAccounts.Web.csproj")
            |> MSBuildReleaseExt null properties "Build"
            |> Log "Build-Output: "

        !! (@".\**\SFA.DAS.EmployerAccounts.Api.csproj")
            |> MSBuildReleaseExt null properties "Build"
            |> Log "Build-Output: "

        !! (@".\**\SFA.DAS.EmployerFinance.Api.csproj")
            |> MSBuildReleaseExt null properties "Build"
            |> Log "Build-Output: "

        let portalProject = "./SFA.DAS.EAS.Portal/SFA.DAS.EAS.Portal.csproj"
        DotNetCli.Publish(fun p ->
            { p with
                Project = portalProject
                Output = directory})
)

Target "Restore Solution Packages" (fun _ ->
    let solutionNames = [| "./SFA.DAS.EAS.sln" |]

    for solutionName in solutionNames do
        solutionName
        |> RestoreMSSolutionPackages (fun p ->
            { p with
                OutputPath = ".\\packages"
                Retries = 4 })

    let coreSolutionNames =  [| "./SFA.DAS.EAS.Portal.sln" |]
    for solutionName in coreSolutionNames do
        DotNetCli.Restore(fun p ->
            { p with
                Project = solutionName })
 )

