param([Parameter(Mandatory=$true)][string]$UnityEditorData)
$ErrorActionPreference='Stop'
$repo=Split-Path $PSScriptRoot -Parent
$output=Join-Path $repo 'Temp\SurvivalChecks'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$compiler=Get-ChildItem "$UnityEditorData\DotNetSdk\sdk" -Filter csc.dll -Recurse | Sort-Object FullName -Descending | Select-Object -First 1
$references=Get-ChildItem "$UnityEditorData\DotNetSdk\packs\Microsoft.NETCore.App.Ref" -Directory | Sort-Object Name -Descending | Select-Object -First 1
$framework=Get-ChildItem (Join-Path $references.FullName 'ref') -Directory | Select-Object -First 1
$runtime=Get-ChildItem "$UnityEditorData\NetCoreRuntime\shared\Microsoft.NETCore.App" -Directory | Sort-Object Name -Descending | Select-Object -First 1
if(-not $compiler -or -not $framework -or -not $runtime){throw 'Unity compiler/runtime was not found.'}
$assembly=Join-Path $output 'SurvivalChecks.dll'
$compilerArgs=@('-nologo','-target:exe','-nostdlib+',"-out:$assembly",(Join-Path $PSScriptRoot 'SurvivalChecks.cs'))
$compilerArgs+=@(Get-ChildItem $framework.FullName -Filter '*.dll' | ForEach-Object {'-r:'+$_.FullName})
$compilerArgs+=@(Get-ChildItem (Join-Path $repo 'Assets\Scripts') -Filter '*.cs' | ForEach-Object FullName)
$dotnet=Join-Path $UnityEditorData 'NetCoreRuntime\dotnet.exe'
& $dotnet $compiler.FullName @compilerArgs
if($LASTEXITCODE -ne 0){throw 'Test compilation failed.'}
@{runtimeOptions=@{tfm=$framework.Name;framework=@{name='Microsoft.NETCore.App';version=$runtime.Name}}} |
    ConvertTo-Json -Depth 4 | Set-Content (Join-Path $output 'SurvivalChecks.runtimeconfig.json')
& $dotnet $assembly
if($LASTEXITCODE -ne 0){throw 'Survival checks failed.'}
