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

You can then start the game either by starting the application (`WarriorsSnuggery.exe` on Windows, `WarriorsSnuggery` on MacOS and Linux) or by running
```
dotnet ./WarriorsSnuggery.dll
```

## Publishing
For publishing Warrior's Snuggery, the dotnet-integrated publish process is used with the profiles `linux_x64`, `windows_x64` and `osx_x64`.
```
dotnet publish /p:PublishProfile=<PROFILE>
```

## Recommended IDEs
If you want to edit and compile the code using an IDE, Visual Studio 2022 or Visual Studio Code is highly recommended (the solution is prepared for the use of those).

Otherwise, apart from `dotnet` and a text editor, no other programs are required.

## Issues and Errors
for any issues, bugs and build errors, feel free to [open an issue](https://github.com/abc013/WarriorsSnuggery/issues/new)!

## Dependencies
### Framework: [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Roslyn Compiler](https://github.com/dotnet/roslyn)
- [OpenTK](https://github.com/opentk/opentk)
- [OpenAL-Soft](https://openal-soft.org/)
- [ImageSharp](https://sixlabors.com/products/imagesharp/)

The dependencies are available on NuGet.
