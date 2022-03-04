![Warrior's Snuggery](https://i.imgur.com/Is8gUOz.png)
![Continuous Integration](https://github.com/abc013/WarriorsSnuggery/actions/workflows/build.yml/badge.svg)
# Please visit the [Warrior's Snuggery Wiki](https://github.com/abc013/WarriorsSnuggery/wiki)!

## Downloading
For playing the precompiled versions, please head over to [the releases](https://github.com/abc013/WarriorsSnuggery/releases).

You can download the source code either as zip and extract it or clone it via `git` using 
```git
git clone https://github.com/abc013/WarriorsSnuggery.git
```
## Compiling
For building Warrior's Snuggery, it is required to have [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0) installed (see dependencies).
Start the command line and head over to the main directory of Warrior's Snuggery. Then, execute
```
dotnet build
```
For more information about the `dotnet build` command, visit [this documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build).

## Publishing
For publishing Warrior's Snuggery, `publish.ps1` is used. This is a powershell file, which means it can only be run on Windows for now. Execute it with PowerShell and select the desired runtime. The project is then published to a corresponding folder.

## Recommended IDEs
If you want to edit and compile the code in an IDE, Visual Studio 2019 or Visual Studio Code is highly recommended (the solution is prepared for the use of those).

## Issues and Errors
for any issues or build failures, feel free to [open an issue](https://github.com/abc013/WarriorsSnuggery/issues/new)!

## Dependencies
### Framework: [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Roslyn Compiler](https://github.com/dotnet/roslyn)
- [OpenTK](https://github.com/opentk/opentk)
- [OpenAL-Soft](https://openal-soft.org/)
- [ImageSharp](https://sixlabors.com/products/imagesharp/)
- System.Runtime.Loader

The dependencies are available on NuGet.
