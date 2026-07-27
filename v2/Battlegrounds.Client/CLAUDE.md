# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Battlegrounds Desktop Launcher V2 — a WPF (.NET 10, `net10.0-windows7.0`) companion launcher for the Company of Heroes: Battlegrounds mod. It manages persistent player companies, hosts/joins multiplayer lobbies, launches matches in CoH2/CoH3, and processes replays after a match to apply company changes. This directory (`v2/Battlegrounds.Client`) is a complete rewrite of the original launcher.

## Commands

All commands run from `v2/Battlegrounds.Client` (the directory containing `Battlegrounds.Client.sln`).

```bash
dotnet restore                                      # restore packages
dotnet build                                        # build (Debug)
dotnet build -c Release                             # build (Release)
dotnet run --project Battlegrounds/Battlegrounds.csproj             # run the app
dotnet run --project Battlegrounds/Battlegrounds.csproj -- --noplay # run without actually launching CoH (dev/testing)
dotnet run --project Battlegrounds.Cli/Battlegrounds.Cli.csproj     # run bgc-edit, the CLI company-file editor
```

### Tests

- **Unit/component tests** live in `Battlegrounds.Test` (NUnit 4 + NSubstitute). Most run with no external dependencies.
- **Integration tests** live in `Battlegrounds.IntegrationTest` and are marked `[Category("Integration")]`. They spin up a containerised backend via Testcontainers and require `docker login ghcr.io/battlegroundscoh` (GitHub username + PAT with `read:packages`). A few `[Category("Integration")]`-tagged tests also exist inside `Battlegrounds.Test` (e.g. `ServerIntegrationTests.cs`).

```bash
dotnet test --filter TestCategory!=Integration      # all non-integration tests (what CI runs)
dotnet test Battlegrounds.Test                      # just the unit test project
dotnet test                                          # everything, including integration (needs Docker + ghcr login)
dotnet test --filter "FullyQualifiedName~MatchOverViewModel"   # run a single class/test by name
```

## Prerequisites

- **.NET SDK 10** — build/run.
- **Git submodule** `coh-battlegrounds-proto` (at repo root, referenced two levels up as `..\..\..\coh-battlegrounds-proto`). Clone with `--recurse-submodules` or run `git submodule update --init --recursive`, otherwise the build fails on the missing `lobby.proto`.
- **`protoc`** — only if regenerating gRPC stubs.
- **Docker** — only for integration tests. **Velopack CLI (`vpk`)** — only for packaging releases (see README for `vpk pack`/release-tag flow).

## Architecture

MVVM over `Microsoft.Extensions.DependencyInjection`. The composition root is **`Battlegrounds/BattlegroundsApp.cs`** — a singleton (`BattlegroundsApp.Instance`) that owns configuration, file-storage bootstrap, DI registration, and async startup. **Read this file first** when tracing how anything is wired: `ConfigureServices` is the single place every service, view, and view-model is registered.

Key structural conventions:

- **Views ↔ ViewModels.** Views (`Views/`) are registered `Transient`; most page-level ViewModels (`ViewModels/`) are `Singleton`, while modal ViewModels (`ViewModels/Modals/`) are `Transient` (a fresh instance per open). Navigation is driven by `MainWindowViewModel`.
- **Services** are interface-first (`IXxxService` → `XxxService`), all registered as singletons, split by concern:
  - `Services/Data/` — loading/persisting domain data: companies, blueprints, doctrines, maps, locales, statistics.
  - `Services/Infrastructure/` — cross-cutting: `UserService` (auth), `UpdateService` (Velopack), `DialogService`, `BrowserService`, `GameService`, `CoH3ArchiverService`.
  - `Services/Playing/` — lobby + match lifecycle: `LobbyService`, `PlayService`, `ReplayService`. `AbstractPlayService` (in `Playing/Common/`) is the shared base for `PlayService` and `SimulatedPlayService`.
- **`--noplay` mode** swaps `IPlayService` from `PlayService` to `SimulatedPlayService` (plus registers `SimulationParameters`) so the app runs the full flow without launching a real game. Preserve this branch when touching play/launch code.
- **Facades (`Facades/API/`)** wrap outbound HTTP: `IBattlegroundsServerAPI` (game server) and `IBattlegroundsWebAPI` (web backend), plus `IAsyncHttpClient` with upload/download progress streaming.
- **gRPC** is used only for lobbies. `lobby.proto` (from the submodule) is compiled to client stubs in the `Battlegrounds.Proto.Lobbies` namespace via `Grpc.Tools`. `GrpcServerClientFactory` builds a `LobbyService.LobbyServiceClient` from `Configuration` host/port; `Services/Playing/LobbyService` consumes it. Note the name collision: `Battlegrounds.Services.Playing.LobbyService` (our service) vs `Battlegrounds.Proto.Lobbies.LobbyService` (generated) — proto types are usually aliased at the top of consuming files.
- **Parsers/Serializers.** `Parsers/` handles CoH data formats — replays (`CoH3ReplayParser`), blueprints, doctrines, scenarios, locales (UCS). Companies persist via a binary format (`ICompanySerializer`/`ICompanyDeserializer` → `BinaryCompany*`). Factories in `Factories/` assemble match data, Lua gamemode sources, and lobby setup.
- **Config & storage.** Runtime state lives outside the repo: `%AppData%/CoHBattlegrounds` and `Documents/my games/CoHBattlegrounds` (config.json, logs, companies). `Configuration` (`Models/Configuration.cs`) is loaded/saved as JSON and registered as a singleton. Logging is Serilog (console + rolling file, 7 files retained), configured in `ConfigureServices`.

### Theming (`Themes/`, `Controls/`)

The client shares its design system with the website (`coh-battlegrounds-website-v2`):
warm near-black surfaces, gold accent, red primary CTA, square corners, Oswald headings over
JetBrains Mono body. `src/styles/global.css` in that repo is the upstream source of truth for
the values.

**`App.xaml` merges exactly one dictionary, `Themes/Theme.xaml`**, which layers the rest.
Each layer may reference only the ones above it:

| Layer | File | Holds |
|---|---|---|
| Palette | `Themes/Palette.xaml` | raw `<Color>` values — the only place a literal colour belongs |
| Brushes | `Themes/Brushes.xaml` | semantic roles over the palette (`Brush.Surface.Card`, `Brush.Text.Dim`) |
| Metrics | `Themes/Metrics.xaml` | corners, spacing, strokes, control sizing |
| Typography | `Themes/Typography.xaml` | fonts, type styles, tracking tokens |
| Controls | `Themes/Controls/*.xaml` | control styles and templates |

Rules that are load-bearing, not stylistic:

- **Every dictionary merges what it references.** A `{StaticResource}` resolves against the
  dictionary itself and its ancestors, *never* against a sibling merged alongside it. Two
  dictionaries merged side by side in `Theme.xaml` cannot see each other's keys.
- **`{DynamicResource}` inside `Themes/`, `{StaticResource}` inside `Views/`.**
  `Themes/Generic.xaml` is resolved independently of `Application.Resources`, so a static
  reference to a token from a control template there would not find it.
- **Name tokens for their role, never their hue.** The pre-redesign palette named its keys
  `AccentBlue` and `BackgroundDeepBlue`; recolouring the app meant every view was asking for
  "blue" and getting gold. Add a brush when a new *role* appears, not when a view wants a
  slightly different shade.
- **Corners are 0** everywhere except the scrollbar thumb and genuinely circular badges.
- **Gold fills take dark text** (`Brush.Text.OnAccent`). White on `#e0a53b` fails contrast.
- Red is the primary CTA *and* the destructive colour — `Button.Cta` and `Button.Danger`
  differ by size and tracking, not hue. That is faithful to the design.

**Reusable controls** live in `Controls/` (lookless, C#) with their default styles in
`Themes/Controls/Surfaces.xaml`, reached through `Themes/Generic.xaml`: `Card`, `PageHeader`,
`Eyebrow`, `StatTile`, `StatusBadge`. Use these instead of hand-assembling a `Border` — that
habit is what left `HomeView` carrying its own private card styles.

`TrackedTextBlock` renders letter-spacing, which WPF has no property for. It draws a
`GlyphRun` with tracking added to each advance, so it does **not** wrap, trim, or support
selection — use it for short static display strings (headings, eyebrows, labels, button
captions) and a plain `TextBlock` for body copy and user data. Button styles set
`TrackedTextBlock.Tracking` on the button; it is an inheriting attached property, so the
caption the template generates picks it up.

**Text crispness has three separate requirements, and missing any one makes the whole app
look soft:**

- Window roots set `TextOptions.TextFormattingMode="Display"` and `UseLayoutRounding="True"`.
  WPF's default `Ideal` mode is built for print-accurate metrics, not for the 10–14px this
  design system runs at. Any new `Window` needs both.
- `TrackedTextBlock` snaps its own glyph origins, baseline and measured size to whole device
  pixels, and sets `UseLayoutRounding` on itself. It has to: the public `GlyphRun`
  constructor always renders in `Ideal` mode, so unlike a `TextBlock` it cannot be put into
  Display mode from a theme, and a baseline landing on a half-pixel has no ClearType to hide
  it. Positions are rounded, not advances — rounding advances individually accumulates and
  quantises small tracking values away to nothing.
- `app.manifest` declares `PerMonitorV2`. Without it the process is only system-DPI-aware and
  Windows bitmap-stretches the window on any monitor at a different scale.

`ControlAssist` supplies per-variant hover/pressed brushes and input placeholders so a single
`ControlTemplate` serves every button variant. It exists because `Setter.Value` cannot be a
binding in WPF, which is what normally forces a copy of the template per variant.

**Reviewing the system:** `dotnet run --project Battlegrounds/Battlegrounds.csproj -- --gallery`
opens a window rendering every token, type style, control and state on one page. It
short-circuits before DI and needs no backend or sign-in. Add to it when you add to the
system — anything missing there is a component nobody can review.

**Checking resource keys:** `python scripts/check-xaml-resources.py` reports every
`{StaticResource}`/`{DynamicResource}` that resolves to no `x:Key`. Worth running after any
theme change: `dotnet build` does *not* catch a missing key — a static reference throws when
the view is first opened and a dynamic one silently resolves to nothing, so a typo can only
surface by navigating to the affected screen.

### Domain models (`Models/`)

Companies (`Company`, `Squad`), Blueprints, Doctrines, Gamemodes, Lobbies, Matches, Replays, Statistics, and Playing (`Game`, `CoH3`, `Scenario`, app-instance abstractions). `LocaleString` carries localised text (English/German/French/Polish, per `Consts.SupportedLanguages`).

## CI

`.github/workflows/dotnet-build-test.yml` runs on push/PR to `master` (windows-latest): restore, build, non-integration tests — PRs with failing tests are rejected. `dotnet-build-release.yml` runs on `v*` tags: publishes a self-contained `win-x64` build and packs/uploads via Velopack.
