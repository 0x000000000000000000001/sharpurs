const fs = require('fs');
let code = fs.readFileSync('src/Main.purs', 'utf8');

let newProjHeader = `projHeader = """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>7.0</LangVersion>
    <WarningsAsErrors>false</WarningsAsErrors>
    <NoWarn>40,25,46,66,67,3370</NoWarn>
  </PropertyGroup>
  <ItemGroup>
"""`;

// replace projHeader entirely
let start = code.indexOf('projHeader = """<Project');
let end = code.indexOf('</ItemGroup>', start);
if (start !== -1 && end !== -1) {
    code = code.substring(0, start) + newProjHeader + code.substring(end + 13);
    fs.writeFileSync('src/Main.purs', code);
    console.log("Patched Main.purs!");
} else {
    console.log("Could not find projHeader in Main.purs");
}
