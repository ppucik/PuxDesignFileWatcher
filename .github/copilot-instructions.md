# Copilot Instructions for PuxDesignFileWatcher

## Project Goal

Implement the full assignment "Program na detekciu zmien v adresari" for manual directory analysis across runs.

Core behavior:

- First run: recursively analyze a provided local directory and store state.
- Next runs: print differences since the previous run.
- Detect:
    - New files
    - Changed files (content change only)
    - Deleted files and deleted subdirectories
- Each file has a version starting at `1`, incremented by `1` on each content change.

## Mandatory Tech Stack

- .NET 10, C#
- Solution name: `PuxDesignFileWatcher.slnx`
- Clean Architecture with strict layer separation:
    - Web/API
    - Application
    - Domain
    - Infrastructure
- Web UI: ASP.NET Core MVC
- API: Minimal API with OpenAPI exposed via Scalar UI

## Performance and Detection Rules

- Expected scale:
    - Up to 100 files
    - Individual file size up to 50 MB
- Use hybrid change detection for performance:
    - First compare size + last write time
    - Compute content hash only when needed
- Do NOT implement automatic filesystem watching.
- Analysis is triggered manually from UI button.

## State Storage Rules

- No database.
- Persist snapshots/manifests in local storage only.
- Support formats:
    - JSON
    - MessagePack
- Select storage format via `appsettings.json` key `StorageFormat`.
- Support configurable manifest location via `appsettings.json` keys:
    - `BasePath`
    - `Mode` with values `AppData` or `PerRoot`

## UI Requirements

- Use Bootstrap 5 via CDN only (no npm/webpack).
- UI must include:
    - Textbox for directory path
    - Button to start analysis
- Show results in separate, clear tables with colors:
    - New: green
    - Changed: yellow
    - Deleted: red
- Form must be responsive.

## Reliability and Safety

- Ensure thread safety during analysis of the same directory (for example by keyed `SemaphoreSlim`).
- Prevent concurrent analysis for the same root path.
- Gracefully ignore locked files:
    - Catch `IOException`
    - Log through `ILogger`

## Architecture Guidance

When generating code, keep responsibilities strict:

- Domain:
    - Entities/value objects for snapshot and file state
    - Domain rules for versioning and change typing
- Application:
    - Use-cases (analyze directory, load state, diff state, save state)
    - Interfaces/ports for filesystem, hashing, and persistence
    - CQRS-style command/query separation
- Infrastructure:
    - Filesystem traversal
    - Hashing implementation
    - JSON and MessagePack manifest repositories
    - Logging and path resolution by mode
- Web/API:
    - MVC controller and view for manual trigger and result rendering
    - Minimal API endpoint(s) for analysis
    - OpenAPI + Scalar UI configuration

## Execution Order

Generate and implement iteratively in this exact order:

1. Create solution, project structure, and baseline README.
2. Implement Domain layer.
3. Implement Application layer (use-cases, interfaces, CQRS).
4. Implement Infrastructure (hashing, filesystem access, state storage JSON/MessagePack).
5. Implement Web + API layer (controllers, minimal endpoints, UI).
6. Add unit and integration tests.

## Testing Expectations

- Add unit tests for:
    - Change classification
    - Version increment behavior
    - Hybrid detection flow
    - Manifest format handling
- Add integration tests for:
    - End-to-end analysis across two runs
    - Deleted files/directories detection
    - Concurrency guard for same path

## Coding Standards

- Favor small, testable services and pure domain logic.
- Use async I/O where appropriate.
- Add clear logging for skipped locked files and key analysis milestones.
- Keep public contracts explicit and stable.
- Avoid coupling UI/API to infrastructure implementation details.
