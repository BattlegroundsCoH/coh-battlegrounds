# Battlegrounds Desktop Launcher V2

The **Battlegrounds Desktop Launcher V2** is a Windows desktop application that serves as the companion launcher for the Company of Heroes: Battlegrounds mod. It allows players to manage their persistent companies, set up multiplayer lobbies, and launch matches in Company of Heroes 2 and Company of Heroes 3.

> **Note:** This launcher is a complete rewrite of the original Battlegrounds launcher, built on a modern tech stack with a focus on maintainability, testability, and a polished user experience.

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Building](#building)
- [Testing](#testing)
- [Distribution Pipeline](#distribution-pipeline)
- [CI/CD Workflows](#cicd-workflows)

---

## Features

- **Company Management** – Browse, create, and edit persistent player companies and their squads.
- **Multiplayer Lobbies** – Host or join lobbies, configure match settings, and invite other players.
- **Match Orchestration** – Automatically generates and deploys a unique win-condition for each match.
- **Post-Match Analysis** – Parses replay files to verify match events and apply company changes.
- **Automatic Updates** – Uses [Velopack](https://velopack.io/) to deliver seamless installer-based updates.
- **Authentication** – Supports Discord and Steam identity providers.
- **Localisation** – Supports English, German, French, and Polish game locales.

---

## Tech Stack

| Component | Technology |
|---|---|
| UI Framework | WPF (.NET 10, `net10.0-windows7.0`) |
| MVVM Toolkit | [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) |
| Dependency Injection | `Microsoft.Extensions.DependencyInjection` |
| Logging | [Serilog](https://serilog.net/) (console + file sinks) |
| Networking | gRPC ([Grpc.Net.ClientFactory](https://grpc.io/)) + [Google.Protobuf](https://protobuf.dev/) |
| Configuration | YAML via [YamlDotNet](https://github.com/aaubry/YamlDotNet) |
| Installer / Updates | [Velopack](https://velopack.io/) |
| Unit Testing | [NUnit 4](https://nunit.org/), [NSubstitute](https://nsubstitute.github.io/) |
| Integration Testing | [Testcontainers for .NET](https://dotnet.testcontainers.org/) |

---

## Project Structure

```
v2/
└── Battlegrounds.Client/
    ├── Battlegrounds.Client.sln       # Solution file
    ├── Battlegrounds/                 # Main WPF application
    │   ├── Assets/                    # Images and icons
    │   ├── Models/                    # Domain models (companies, squads, lobbies, etc.)
    │   ├── ViewModels/                # MVVM view-models
    │   ├── Views/                     # XAML views and code-behind
    │   ├── Services/                  # Business logic and infrastructure services
    │   ├── Facades/                   # API client facades (gRPC)
    │   ├── Parsers/                   # Replay and blueprint parsers
    │   ├── Serializers/               # Data serialisers
    │   ├── Converters/                # XAML value converters
    │   ├── Resources/                 # Shared XAML resource dictionaries
    │   ├── Logging/                   # Logging configuration helpers
    │   ├── App.xaml / App.xaml.cs     # Application entry-point
    │   └── BattlegroundsApp.cs        # Application host and DI composition root
    └── Battlegrounds.Test/            # NUnit test project
        ├── Facades/                   # Tests for API facades
        ├── Models/                    # Tests for domain models
        ├── Parsers/                   # Tests for parsers
        ├── Serializers/               # Tests for serialisers
        ├── Services/                  # Tests for services
        ├── ViewModels/                # Tests for view-models
        └── TestData/                  # Fixture files (blueprints, replays, locales)
```

---

## Prerequisites

The following tools must be installed before you can build or run the application:

| Tool | Notes |
|---|---|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | Required to build and run the application |
| [Protocol Buffer Compiler (`protoc`)](https://github.com/protocolbuffers/protobuf/releases) | Required to generate gRPC stubs from `.proto` files |
| [Velopack CLI (`vpk`)](https://velopack.io/) | Required only when creating release packages |
| [Docker](https://www.docker.com/) | Required only when running integration tests locally |

---

## Getting Started

1. **Clone the repository with submodules:**
   ```bash
   git clone --recurse-submodules https://github.com/BattlegroundsCoH/coh-battlegrounds.git
   cd coh-battlegrounds
   ```

   If you already cloned without submodules, initialise them now:
   ```bash
   git submodule update --init --recursive
   ```

2. **Restore NuGet packages:**
   ```bash
   cd v2/Battlegrounds.Client
   dotnet restore
   ```

3. **Build the solution:**
   ```bash
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project Battlegrounds/Battlegrounds.csproj
   ```

---

## Building

To produce a Release build:

```bash
cd v2/Battlegrounds.Client
dotnet build -c Release
```

To publish a self-contained executable for Windows x64:

```bash
dotnet publish Battlegrounds/Battlegrounds.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o publish/
```

---

## Testing

The solution includes two categories of automated tests.

### Unit & Component Tests

These tests run without any external dependencies and are safe to execute locally at any time:

```bash
cd v2/Battlegrounds.Client
dotnet test --filter TestCategory!=Integration
```

### Integration Tests

Integration tests spin up a containerised Battlegrounds backend server using [Testcontainers](https://dotnet.testcontainers.org/). Before running them you must authenticate against the GitHub Container Registry:

```bash
docker login ghcr.io/battlegroundscoh
```

Use your GitHub username and a [personal access token](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens) with `read:packages` scope.

Then run all tests (including integration):

```bash
cd v2/Battlegrounds.Client
dotnet test
```

---

## Distribution Pipeline

The distribution pipeline uses [Velopack](https://velopack.io/) to build a Windows installer and to publish delta-update packages to GitHub Releases.

### Install the Velopack CLI

```bash
dotnet tool install -g vpk
```

### Pack a Release Locally

```bash
# Build and publish first (see Building section above)
vpk pack \
  --packId Battlegrounds \
  --packVersion <version> \
  --packDir publish/ \
  --mainExe Battlegrounds.exe \
  --packTitle "Company of Heroes: Battlegrounds" \
  --icon v2/Battlegrounds.Client/Battlegrounds/Assets/app.ico \
  --outputDir releases/
```

### Triggering a Release

Create a new GitHub Release with a tag in the format `v<major>.<minor>.<patch>` (e.g. `v2.1.0`). The [Build & Release](#cicd-workflows) workflow will automatically build, pack, and upload the Velopack artefacts to that release.

---

## CI/CD Workflows

Two GitHub Actions workflows are defined in `.github/workflows/`:

### `dotnet-build-test.yml` – Build & Test

| Trigger | `push` or `pull_request` targeting `master` |
|---|---|
| Runner | `windows-latest` |

Runs on every push to `master` and on all pull requests. Steps:
1. Check out the repository (including submodules).
2. Set up .NET 10 SDK.
3. Restore dependencies.
4. Build and run all non-integration tests.

Pull requests with failing tests will be rejected.

### `dotnet-build-release.yml` – Build & Release

| Trigger | `push` of a tag matching `v*` |
|---|---|
| Runner | `windows-latest` |

Runs automatically when a version tag is pushed. Steps:
1. Check out the repository.
2. Set up .NET 10 SDK.
3. Install the Velopack CLI.
4. Extract the version number from the tag.
5. Restore dependencies.
6. Publish a self-contained `win-x64` executable.
7. Pack with Velopack (`vpk pack`).
8. Upload the packaged release to GitHub Releases (`vpk upload github`).

