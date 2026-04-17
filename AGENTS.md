# AGENTS.md
Guidance for agentic coding assistants working in this Unity repository.
## Project snapshot
- Project: Unity game (`Flat Cafe.sln`)
- Unity version: `6000.0.62f1` (`ProjectSettings/ProjectVersion.txt`)
- Main gameplay code: `Assets/Scripts/`
- Major third-party code: `Assets/Plugins/`, `Assets/Demigiant/`, `Assets/Samples/`, `Packages/com.arongranberg.astar/`
- Generated C# language version: C# 9 (`Assembly-CSharp.csproj`)
## Cursor/Copilot rule files
Checked locations:
- `.cursorrules`: not found
- `.cursor/rules/`: not found
- `.github/copilot-instructions.md`: not found
If any of these files are added later, treat them as higher-priority instructions and update this document.
## Default agent behavior
- Prefer small, focused changes over broad refactors.
- Read nearby code first and follow local conventions before introducing new patterns.
- Do the work without asking clarifying questions unless blocked by a real ambiguity, missing secret, or risky/destructive action.
- When a reasonable default exists, choose it and state the assumption in the final response.
- Avoid touching unrelated files, systems, or assets.
- Validate changes with the narrowest relevant check first, then run broader verification only when justified.
- Report back concisely: what changed, what was verified, and any remaining limitations.
## Safe edit boundaries
- Prefer edits in `Assets/Scripts/` unless task explicitly targets package/plugin code.
- Avoid editing generated/build outputs:
  - `*.csproj`, `*.sln`, `*.slnx`
  - `Library/`, `Temp/`, `Logs/`, `obj/`
- Avoid modifying vendored/sample content unless explicitly asked.
- Preserve Unity metadata consistency for moved/created assets (`.meta` pairing).
## Do not over-explore
- For a localized task, inspect only the files and systems directly related to the request.
- Do not scan the whole project unless the task explicitly requires broad analysis or the affected area is unclear.
- Prefer targeted file search and nearby-code inspection over repository-wide exploration.
- Stop exploring once there is enough context to make a safe, minimal change.
- If multiple implementations are possible, prefer the one that matches the nearest existing pattern.
## Environment setup
- Open root project folder in Unity Hub.
- Prefer Unity Editor `6000.0.62f1`.
- Let Unity finish import/domain reload before compile/test/build runs.
## Unity MCP preference
- When Unity MCP tools are available, prefer them for Unity-specific operations such as scene inspection, GameObject changes, component edits, asset management, prefab work, console reads, and Unity test execution.
- Prefer filesystem edits for normal C# source changes unless the task specifically benefits from Unity-side operations.
- Prefer Unity MCP over manual asset/file manipulation when `.meta` consistency or serialized Unity data may be affected.
- Use shell-based Unity batch commands as a fallback when Unity MCP cannot perform the required verification or operation.
- Do not modify scenes, prefabs, or assets through Unity MCP unless the task requires it.
## Build and compile commands
Run commands from repo root: `E:\Unity Projects\Flat Cafe`
### 1) Unity batch compile (authoritative)
```powershell
$UNITY_EXE = "D:\Unity Editors\6000.0.62f1\Editor\Unity.exe"
& $UNITY_EXE -batchmode -nographics -quit -projectPath "E:\Unity Projects\Flat Cafe" -logFile "Logs\unity-compile.log"
```
- Validates script compile + asset import pipeline.
- Check `Logs/unity-compile.log` for errors.
### 2) .NET build (secondary sanity check)
```powershell
dotnet build "Flat Cafe.sln" -c Debug
```
- Useful for quick C# feedback.
- Unity batch compile remains source of truth.
### 3) Player build
- No dedicated CI build entry method was found in repo scripts.
- Current reliable path: Unity Editor Build Settings.
- For automation, create a static Editor build method and run it via `-executeMethod`.
```powershell
& $UNITY_EXE -batchmode -nographics -quit -projectPath "E:\Unity Projects\Flat Cafe" -executeMethod BuildScript.BuildWindows -logFile "Logs\unity-build.log"
```
## Lint/format commands
No explicit lint config (`.editorconfig`, StyleCop, Roslyn ruleset) was found.
Recommended verification:
```powershell
dotnet format "Flat Cafe.sln" --verify-no-changes
```
Optional auto-fix:
```powershell
dotnet format "Flat Cafe.sln"
```
Notes:
- Unity analyzers are referenced in generated project files.
- Treat runtime-risk warnings (nullability/API misuse) as actionable.
## Test commands (Unity Test Framework)
`com.unity.test-framework` is present in `Packages/manifest.json`.
### Run all EditMode tests
```powershell
& $UNITY_EXE -batchmode -nographics -projectPath "E:\Unity Projects\Flat Cafe" -runTests -testPlatform EditMode -testResults "Logs\editmode-results.xml" -logFile "Logs\editmode.log" -quit
```
### Run all PlayMode tests
```powershell
& $UNITY_EXE -batchmode -nographics -projectPath "E:\Unity Projects\Flat Cafe" -runTests -testPlatform PlayMode -testResults "Logs\playmode-results.xml" -logFile "Logs\playmode.log" -quit
```
### Run a single test (important)
Use `-testFilter` with a fully-qualified test name.
```powershell
# Single method
& $UNITY_EXE -batchmode -nographics -projectPath "E:\Unity Projects\Flat Cafe" -runTests -testPlatform EditMode -testFilter "Namespace.ClassName.TestMethod" -testResults "Logs\single-test.xml" -logFile "Logs\single-test.log" -quit

# Entire class
& $UNITY_EXE -batchmode -nographics -projectPath "E:\Unity Projects\Flat Cafe" -runTests -testPlatform EditMode -testFilter "Namespace.ClassName" -testResults "Logs\class-tests.xml" -logFile "Logs\class-tests.log" -quit
```
Current repo state:
- No first-party tests were detected under `Assets/Scripts/` during scan.
- If adding tests, use:
  - `Assets/Tests/EditMode/`
  - `Assets/Tests/PlayMode/`
## Code style guidelines
Apply these conventions in new/edited files.
### File/type organization
- One primary public type per file; filename matches type name.
- Keep gameplay code in domain folders under `Assets/Scripts/`.
- Keep editor-only code in an `Editor/` folder.
- Avoid broad refactors done only for style normalization.
### Namespaces
- Existing gameplay scripts are namespace-less.
- In touched files, stay namespace-less unless task requires migration.
- For brand new isolated modules, namespaces are fine if applied consistently.
### Imports (`using`)
- Remove unused imports.
- Preferred order:
  1. `System*`
  2. Third-party namespaces (e.g., `DG.Tweening`, `Pathfinding`)
  3. Unity namespaces (`UnityEngine*`, `UnityEditor*`)
- Avoid `using static` unless it clearly improves readability.
### Formatting
- 4 spaces, no tabs.
- Prefer Allman braces (`{` on next line), which is dominant in this repo.
- Keep methods focused and small.
- Use blank lines between logical blocks.
- Remove trailing whitespace.
### Access and serialized fields
- Prefer `[SerializeField] private` for Inspector references.
- Prefer properties for public read access, with private setters where needed.
- Use explicit access modifiers (avoid implicit private members).
- Backing fields commonly use `_camelCase`; match local file style.
### Serialized field style
- Prefer `[SerializeField] private` for Inspector-assigned references and tunable values.
- Keep serialized fields private unless there is a clear need for broader access.
- Prefer public read access through properties or methods rather than public fields.
- Match the local naming style of the file; default to `_camelCase` for private serialized fields when no stronger convention exists.
- Group serialized fields near the top of the class.
- Use `[Header]`, `[Tooltip]`, and `[Range]` only when they improve Inspector clarity.
- Example:
  [SerializeField] private Transform _target;
  [SerializeField] private float _moveSpeed = 5f;

  public Transform Target => _target;
### Naming conventions
- Types, methods, properties, events: `PascalCase`.
- Locals/parameters: `camelCase`.
- Constants: `PascalCase` unless file has a stronger local convention.
- Events should describe occurrence (`OnFoodPicked`, `OnInteractionEnded`).
- Avoid ambiguous abbreviations.
### Types and null safety
- Use explicit types when it helps clarity; use `var` for obvious RHS.
- Guard serialized references and lookup results (`GetComponent`, finds).
- Prefer early returns to reduce nesting.
- In invalid states, fail fast with concise actionable logs.
### Error handling and logging
- Use `Debug.LogError` for blocking misconfiguration/invalid state.
- Use `Debug.LogWarning` for recoverable issues.
- Keep logs concise and include useful context.
- Avoid per-frame log spam in `Update()`.
### Unity-specific guidance
- Cache repeated component lookups.
- Keep `Update()` lightweight; move heavier logic to events/coroutines/state transitions.
- Use Inspector attributes (`[Header]`, `[Tooltip]`, `[Range]`) when they clarify intent.
- Do not rely on editor-only APIs in runtime paths.
### UI implementation guidance
- Current project UI stack is `uGUI` + `TextMeshPro`; prefer continuing this stack unless the task explicitly requires another approach.
- Preserve the existing visual language unless the user explicitly asks for a redesign.
- Prefer reusable UI prefabs/components over one-off scene-only setups.
- Prefer a prefab-first workflow for UI. Avoid generative UI for static structure; do not build full screen, panel, popup, or window hierarchy in code if it can be authored as a prefab.
- Generative UI is acceptable only for repeated or data-driven content such as list items, catalog cards, inventory entries, tabs, and similar content elements.
- Runtime-generated content elements should be authored as reusable prefabs under `Assets/Prefabs/UI/` and instantiated from there instead of being built from raw GameObjects in code.
- For new UI, attach it to the appropriate existing UI root/canvas/document if one already exists; do not create an additional root unless the current architecture requires a separate one.
- Prefer the existing main screen-space `Canvas` for regular UI. Use a separate `World Canvas` only for world-space indicators or diegetic UI.
- New screens, popups, and side panels should follow a consistent `Root + Panel` structure:
  - `Root`: full-screen stretched container that owns visibility, input blocking, and optional dim background
  - `Panel`: the visible window/container anchored inside `Root`
  - Optional children: `Header`, `Content`, `Footer`, `CloseButton`
- Keep new screens/panels under a clear root container so they are easy to enable, disable, and reference.
- Keep hierarchy clean and readable; use clear object names and avoid unnecessary nesting.
- Standardize UI object naming with `PascalCase` for roots and major nodes, for example `BuildShopRoot`, `BuildShopPanel`, `Header`, `Content`, `Footer`, `CloseButton`.
- Expose designer-tunable values through `[SerializeField] private` fields instead of hardcoding them.
- Separate presentation from gameplay logic where practical; keep view scripts focused on display, input wiring, animation triggering, and state reflection.
- For interactive UI, wire references safely and guard against missing bindings with concise `Debug.LogError` messages.
- Do not wire UI by searching for GameObjects or Buttons by name at runtime. Prefer `[SerializeField] private` references, typed bindings, or explicit setup methods.
- For dynamic collections and resizable content, prefer `ScrollRect` plus layout components such as `VerticalLayoutGroup`, `HorizontalLayoutGroup`, `GridLayoutGroup`, `ContentSizeFitter`, and `LayoutElement` over manual positioning.
- Avoid hardcoded pixel offsets and sizes when anchors, padding, spacing, and layout components can express the intended behavior more safely.
- Structure content so UI elements do not overlap each other. If content can grow, place it in a scrollable container instead of allowing layout overflow or panel overlap.
- Keep action buttons in a dedicated `Footer` area when appropriate instead of mixing them into free-form content positioning.
- Modal UI should typically use `CanvasGroup` on `Root`; animate the `Panel` rather than the full canvas root when practical.
- When animation improves clarity or feel, prefer `DOTween` for UI transitions, panel movement, fades, and popup state changes to keep behavior consistent with the rest of the project.
- Ensure the result works on common target aspect ratios/resolutions relevant to the project.
- If creating new UI assets outside Unity would risk broken serialization or `.meta` issues, prefer Unity MCP or Unity Editor workflows.
- Place reusable UI prefabs under `Assets/Prefabs/UI/` and related scripts under `Assets/Scripts/UI/`.
- For UI tasks, do not over-design or restyle unrelated screens.
### Testing expectations for new changes
- Add EditMode tests for isolated gameplay logic.
- Add PlayMode tests for integration behavior.
- Run narrowest relevant test first (single test), then broader scope when practical.
## Agent workflow checklist
1. Read nearby code and mirror local conventions.
2. Make minimal focused changes.
3. Run compile check (Unity batch compile or `dotnet build`).
4. Run relevant tests (single-test first when available).
5. Report exactly what was run and any limitations.
## Maintenance note
If tooling/process changes (new CI, tests, lint rules, Cursor/Copilot files), update this file immediately.
