# ManagementApp

Cross platform Avalonia app for tracking daily and study tasks with an offline SQLite database

## Features
- Daily tasks and study tasks separated into tabs.
- Priority, due dates, and subject tracking.
- Start/stop timer with break reminders.
- Archive tasks instead of deleting.
- Report page with PDF export (QuestPDF).

## Development in IntelliJ
This repository is intended to be developed with IntelliJ. Open the `DidiApp.sln` file in IntelliJ IDEA (or JetBrains Rider if you have the .NET tooling plugin installed) and use the built-in run configuration for `DidiApp`. The project targets .NET 8, so ensure the .NET 8 SDK is installed locally.

## Quick start
1. Install .NET 8 SDK.
2. Restore and run:

```bash
dotnet restore

dotnet run --project src/DidiApp/DidiApp.csproj
```

The app stores data locally in a SQLite file under your OS user profile (LocalApplicationData) in a `DidiApp` folder so the database persists across runs without requiring write access to the install directory.

## Troubleshooting debug profile issues
If IntelliJ or Visual Studio says the debug executable is missing, it usually means the project did not build successfully. Ensure you run a clean build:

```bash
dotnet clean src/DidiApp/DidiApp.csproj
dotnet build src/DidiApp/DidiApp.csproj
```

For IntelliJ users, a `.run/DidiApp.run.xml` configuration is included so the run profile is present immediately after cloning.
-
Please contact me for any other bugs , my email is `elhaddouryyounes@gmail.com´.
-



