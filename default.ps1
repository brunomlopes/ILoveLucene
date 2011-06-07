$framework = "4.0"
properties {
    $version = "1.0.0.0"
}

Task Update-Solution-Assembly-Info -description "Updates the solution wide assembly info" {
    Generate-Version-Info `
        -assembly_file ".\src\Core\Properties\AssemblyInfo.cs" `
        -class_file ".\src\Core\ProgramVersionInformation.cs" `
        -company "" `
        -product "ILoveLucene" `
        -version "$version" `
        -copyright "Copyright Bruno Lopes, 2009-2011"
}

Task Build-Package -depends Update-Solution-Assembly-Info -description "Builds a package on output directory" {
    # Exec { msbuild /version }

    $outputRoot = (Get-Item .).FullName
    $outputDir = "$outputRoot\output"
    if (Test-Path $outputDir){
        Remove-Item $outputDir -recurse -force
    }
    
    Exec { msbuild /t:clean /p:Configuration=Release /p:OutDir=$outputDir\ }
    Exec { msbuild /t:build /p:Configuration=Release /p:OutDir=$outputDir\ }
    $binaries = Get-ChildItem $outputDir -exclude ILoveLucene*,Plugins*,Configuration
    $plugins = Get-ChildItem $outputDir\Plugins*
    mkdir $outputDir\Plugins
    mkdir $outputDir\Bin
    Move-Item $binaries $outputDir\Bin
    Move-Item $plugins $outputDir\Plugins

}

Task Help {
    Write-Documentation
}

Task default -depends Help

function Generate-Version-Info
{
param(
    [string]$clsCompliant = "true",
    [string]$company, 
    [string]$product, 
    [string]$copyright, 
    [string]$version,
    [string]$assembly_file = $(throw "assembly_file is a required parameter."),
    [string]$class_file = $(throw "class_file is a required parameter.")
)
    $commit = Get-Git-Commit
    $timestamp = get-date -UFormat %Y%m%d_%H%M
    $datetime = get-date -UFormat "new DateTime(%Y,%m,%d,%H,%M,%S)"
    $asmInfo = "using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


[assembly: CLSCompliantAttribute(false)]
[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyCompanyAttribute(""$company"")]
[assembly: AssemblyProductAttribute(""$product"")]
[assembly: AssemblyCopyrightAttribute(""$copyright, $commit / $timestamp"")]
[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyInformationalVersionAttribute(""$version"")]
[assembly: AssemblyFileVersionAttribute(""$version"")]
[assembly: AssemblyDelaySignAttribute(false)]
"
    $classInfo = "
namespace Core
{
    using System;
    public static class ProgramVersionInformation
    {
        public static string Version = ""$commit / $timestamp"";
        public static DateTime PackageDate = $datetime;
    }
}
"

    Write-Host "Generating assembly info file: $assembly_file"
    $asmInfo | out-file -encoding utf8 $assembly_file
    
    Write-Host "Generating class info file: $class_file"
    $classInfo | out-file -encoding utf8 $class_file
}

function Get-Git-Commit
{
    $gitPath = ?? { (get-item "env:programfiles(x86)").Value } { $env:programfiles }
    $gitLog = & "$gitPath\Git\bin\git.exe" log --oneline -1
    return $gitLog.Split(' ')[0]
}