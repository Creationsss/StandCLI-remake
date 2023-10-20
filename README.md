# StandCLI
![GitHub last commit](https://img.shields.io/github/last-commit/Creationsss/StandCLI-remake)
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Creationsss/StandCLI-remake)
![GitHub issues](https://img.shields.io/github/issues/Creationsss/StandCLI-remake)
![GitHub](https://img.shields.io/github/license/Creationsss/StandCLI-remake)

Inject Stand's DLL into GTA V using this command-line interface. This tool allows you to manage the injection process and interact with the game. Keep in mind that this tool is intended for educational and research purposes and should be used responsibly.

> [*Stand*](https://stand.gg/)

## Features

- Inject the "Stand" DLL into the GTA V game process.
- Choose what version of supported stand version you want to use
- Create a GTA V launcher. (Automatically launches StandCLI on startup of game)
- Monitor logs and error messages.

## Prerequisites

- Windows operating system. (or a translation layer for use in Unix based systems.)
- .NET Framework.
- Administrative privileges for specific operations.

## Installation

1. Clone this repository and build it from source:

```bash
git clone https://github.com/Creationsss/StandCLI-remake.git
```

1. Compile the source code using your preferred C# compiler (e.g., Visual Studio). For Linux, you can use mcs:
```bash
mcs -unsafe Program.cs
```

> Alternatively, you can use the included script for Linux to build and run it, you need dotnet installed. To run it with Wine, use the -w option:

> You can also easily build it on windows by just running the windows-build.bat, -c and -r are the options

```bash
./linux-build.sh -w
```
## Usage

- Use the arrow keys to navigate the menu, in some options left and right arrow keys can be used to modify values


## Contributing

Contributions to this project are highly encouraged. You are welcome to open issues or submit pull requests to enhance the tool or introduce new features. For significant changes, please initiate a discussion by opening an issue first.
License

Disclaimer: This tool is exclusively for educational and research purposes. Its usage may be subject to the game's terms of service and the laws of your jurisdiction. Please use it responsibly and at your own risk. The creator of this tool and its contributors are not accountable for any misuse or consequences arising from its usage.
