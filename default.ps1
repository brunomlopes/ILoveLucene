$framework = "4.0"
properties {
    $version = "1.0.1.0"
    $dropbox_base_url = "http://dl.dropbox.com/u/118385/ilovelucene/"
    $appcast_path = "appcast.xml"
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
    $gitHash = Get-Git-Commit
    $outputRoot = (Get-Item .).FullName
    $outputDir = "$outputRoot\output"
    if (Test-Path $outputDir){
        Remove-Item $outputDir -recurse -force
    }
    $packageDir = "$outputRoot\package"
    if (-not (Test-Path $packageDir)){
        mkdir $packageDir
    }
    
    Exec { msbuild /t:clean /p:Configuration=Release /p:OutDir=$outputDir\ }
    Exec { msbuild /t:build /p:Configuration=Release /p:OutDir=$outputDir\ }
    $binaries = Get-ChildItem $outputDir -exclude ILoveLucene*,Plugins*,Configuration
    $plugins = Get-ChildItem $outputDir\Plugins*
    mkdir $outputDir\Plugins | Out-Null
    mkdir $outputDir\Bin | Out-Null
    Move-Item $binaries $outputDir\Bin
    Move-Item $plugins $outputDir\Plugins
    
    Remove-Item $outputDir\Bin\*.pdb -exclude Core.pdb,ElevationHelper.Services.pdb
    
    $package_name = "ILoveLucene-$version-$gitHash.zip"
    $package_path = "$packageDir\$package_name"
    
    Push-Location $outputDir
    $zip = Write-Zip .\* $package_path -level 9
    Pop-Location
    
    Write-Host "File is up at $zip"
    
    Generate-Appcast-Item `
        -version $version `
        -package_url $dropbox_base_url `
        -package_filename $package_name `
        -package_size (get-item $package_path).Length `
        -appcast_path $appcast_path
}

Task Help {
    Write-Documentation
}

Task default -depends Help

function Generate-Appcast-Item
{
param(
    [string]$version,
    [string]$package_url,
    [string]$package_filename,
    [string]$package_size,
    [string]$appcast_path
)
    $pub_date = Get-Date -format r
    $full = [xml]"<rss version=""2.0"" xmlns:appcast=""http://www.adobe.com/xml-namespaces/appcast/1.0"">
  <channel>
    <item>
      <title>ILoveLucene</title>
      <link>http://github.com/brunomlopes/ILoveLucene/downloads</link>
      <description></description>
      <pubDate>$pub_date</pubDate>
      <appcast:version>$version</appcast:version>
      <enclosure url=""$package_url/$package_filename"" length=""$package_size"" type=""application/octet-stream"" />
    </item>
  </channel>
</rss>
"
    $item = $full.rss.SelectSingleNode("channel/item")

    $appcast = [xml](Get-Content $appcast_path)
    $imported_item = $appcast.ImportNode($item, $true)
    $result = $appcast.rss.channel.AppendChild($imported_item)
    $appcast | Format-Xml | Out-File -encoding utf8 $appcast_path
}

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
        public static string Version = ""$version"";
        public static string GitVersion = ""$commit / $timestamp"";
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
    $programFiles = ""
    if (Test-Path "env:programfiles(x86)"){
      $programFiles = (Get-Item "env:programfiles(x86)").Value
    }else{
      $programFiles = (Get-Item "env:programfiles").Value
    }
    $gitLog = & "$programFiles\Git\bin\git.exe" log --oneline -1
    return $gitLog.Split(' ')[0]
}