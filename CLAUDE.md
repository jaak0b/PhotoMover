# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Hard Rules

1. **TDD mandatory, test-first.** For every behavior change including bug fixes: (a) write the test, (b) run it and quote the failing output, (c) only then touch production code, (d) re-run to green. No red proof = the fix does not start. If you catch yourself having edited production code first, revert and restart from (a).
2. **Verification gate.** A change is not done until verified with real command output quoted — never claim finished or commit on assumed results. Run the test fixtures covering the code you touched (`dotnet test --filter ...`) and quote pass/fail totals. If a pre-existing failure blocks verification, stop and surface it with evidence — do not quietly proceed.
3. **No test touches the developer's filesystem outside temp.** Use `Path.GetTempPath()` temp dirs, cleaned up in teardown/Dispose. Never write to real user folders (`Documents`, `%APPDATA%`) in tests.
4. **No empty catch blocks.** Log the error and, for user-initiated operations, surface it in the UI (e.g. `Status` property). Swallowing exceptions silently hides failures.
5. **NuGet packages: official Microsoft or highly-regarded community only.** No niche/unmaintained single-author packages. Prefer built-in BCL APIs over third-party dependencies.
6. **No positional tuple access.** Never read `.Item1`/`.Item2` or destructure positionally; multi-value returns are named `record`/`record struct` types read by name.
7. **Manual UI verification for UI changes.** Ask the user to run the app with exact repro steps. Tests are in addition to this, never instead.
8. **Test layers per change.** Every feature and bug fix needs unit tests and, where the change crosses service boundaries (pipeline, FTP, repository, filesystem), integration-style tests against real sockets/temp dirs. "It's only a small change" is not an exemption. Untestable-by-design code (pure XAML) is the only exception, and say so explicitly.
9. **No trademarked words in files.**
10. **Self-review only when a feature is fully complete, before merge — multi-file changes only.** Run a medium-effort `/code-review` scoped to the change exactly once: when the feature is finished and ready to merge. Never run it mid-feature or between intermediate steps — it confuses subagents. Every finding gets a logged disposition: Fixed (name the test/commit), False positive (proven with the quoted line that refutes it), or Owner-waived (flagged and the user explicitly chose to skip). "Pre-existing", "out of scope", "cosmetic" are never valid reasons to skip. Single-file changes exempt.
11. **Owner review gates every commit.** Never `git commit` or `git push` until the repository owner has explicitly approved the change in chat — present the diff summary and ask, proceed only after a clear "yes".
12. **Never store or log credentials in plaintext.** Applies to FTP passwords, tokens, anything secret — don't write them to logs, config files, or the UI. If credential storage is ever added, use built-in PBKDF2 (`Rfc2898DeriveBytes`), per-user random salt, nothing reversible.
13. **No AI attribution in anything that touches git or GitHub** — not in commit messages, PR titles or descriptions, comments, tags, or release notes. No `Co-Authored-By` trailer, no "Generated with" line, no AI author/committer identity. Commits carry the human's authorship only. **Commit messages are a short, single sentence** — one line, no body; a change too big for one sentence gets split into smaller commits.

## Definition of Done — check before calling any change "finished"

- [ ] Tests written first (rule #1) — red output quoted before production code existed.
- [ ] Test layers present (rule #8) — or explicit note why a layer doesn't apply.
- [ ] Changed-area fixtures green (rule #2) — totals quoted, not assumed.
- [ ] Manual UI verification requested for UI changes (rule #7) — exact repro steps given.
- [ ] Review run once at feature completion, every finding dispositioned (rule #10) — or single-file exemption stated.

## Testing conventions

- Fixture = `<ClassUnderTest>Tests`, method = `MethodName_State_Expected`. One fixture per production class; never a catch-all fixture.
- Mocks via Moq for Core interfaces; FluentAssertions for asserts.
- Integration tests use real loopback sockets / `Path.GetTempPath()` dirs, cleaned up in Dispose (rule #3).

## Project

PhotoMover: Windows WPF app (.NET 10) that imports photos from SD cards or an embedded FTP server, extracts EXIF metadata, and copies files into destination folders based on user-defined grouping rules with placeholder patterns (e.g. `{CameraModel}/{DateTaken:yyyy}/{DateTaken:MM}`).

## Commands

```
dotnet build                                   # build solution
dotnet test                                    # run all tests
dotnet test --filter "FullyQualifiedName~GroupingRuleEngineTests"   # single test class
dotnet test --filter "FullyQualifiedName~GroupingRuleEngineTests.MethodName"  # single test
dotnet run --project PhotoMover                # run app (Windows only, WPF)
```

Tests: xUnit + FluentAssertions + Moq.

## Architecture

Three-layer solution, dependencies flow inward:

- **PhotoMover.Core** — models and service interfaces only, no implementations. `net10.0`, no Windows dependency. Key models: `GroupingRule` (record with `PathPattern`, `DestinationPath`, `IsActive`, `Priority`), `PhotoMetadata`, `ImportResult`, `ImportSource`.
- **PhotoMover.Infrastructure** — implementations of Core interfaces. Uses `MetadataExtractor` (EXIF) and `FubarDev.FtpServer` (embedded FTP). Notable: `ImportPipeline` (orchestrates extract → resolve path → copy), `GroupingRuleEngine` (expands placeholder patterns), `JsonRuleRepository` (rules persisted as JSON under `%APPDATA%\PhotoMover\Rules`), `EmbeddedFtpServer` + `FileUploadCompletionDetector` (watches FTP temp dir, uses file-lock probing to detect completed uploads before importing).
- **PhotoMover** — WPF UI (`net10.0-windows`), MVVM. `App.xaml.cs` wires everything with `Microsoft.Extensions.DependencyInjection` (all singletons); this is the composition root — register new services/viewmodels there. Single `MainWindow` with tab-per-feature viewmodels: `SdImportViewModel`, `RuleEditorViewModel`, `FtpServerViewModel`, coordinated by `MainWindowViewModel`. Commands use hand-rolled `RelayCommand`/`ViewModelBase` (no MVVM framework).

Key flow: UI viewmodel gathers source files → `IImportPipeline.ProcessPhotosAsync(files, rule, destinationRoot, progress, ct)` → `IMetadataExtractor` reads EXIF → `IGroupingRuleEngine` builds relative path from the rule's `PathPattern` → `IFileSystem` copies. FTP path is the same pipeline, triggered by `FileUploadCompletionDetector` callbacks instead of the UI.

## Conventions

- File-system access goes through `IFileSystem` abstraction (testability) — don't call `System.IO` directly in services.
- Only one grouping rule can be active at a time; rules created before `DestinationPath` existed default to `Documents\PhotoMover`.
- Nullable reference types enabled everywhere; `LangVersion latest`.
