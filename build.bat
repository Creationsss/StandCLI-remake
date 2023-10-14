@echo off
setlocal enabledelayedexpansion

set "current_folder=%cd%"
set "dotnet_publish_command=dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true"
set "publish_folder=%current_folder%\bin\Release\net6.0\win-x64\publish"

call %dotnet_publish_command%
if %errorlevel% equ 0 (
    echo dotnet publish completed successfully.

    if exist "%publish_folder%" (
        echo Output location: %publish_folder%

        for /f %%f in ('dir /s /b "%publish_folder%\*.exe"') do set "executable_file=%%f"

        if not "!executable_file!"=="" (
            echo Executable file: !executable_file!
            
            if "%~1"=="-c" (
                echo Copying the executable file next to the script...
                copy "!executable_file!" "%current_folder%"
                echo Executable file copied successfully.
            ) else if "%~1"=="-r" (
                "!executable_file!"
            ) else (
                echo Invalid option: %~1
                exit /b 1
            )
        ) else (
            echo Executable file not found in the publish directory.
        )
    ) else (
        echo Output folder does not exist.
    )
) else (
    echo dotnet publish failed.
)
endlocal
