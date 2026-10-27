---
name: jhtgj-game-review
description: >-
  Reviews JHTGJ / 遗忘时钟 Unity C# for day-based story progression, PlayerPrefs
  save/load, and Editor content tooling. Use when the user asks for AI review,
  code review, jhtgj-game-review, or checks story/save/editor changes.
last-verified: 2026-10-27 (Unity 2022.3.x, JHTGJ/Assets/Game)
---

# JHTGJ Game Code Review

Review **Forgotten Clock (遗忘时钟)** with focus on three areas only unless the user expands scope:

1. Day / phase story progression (`StoryEventTreeManager` + SO schedules)
2. PlayerPrefs save & session bridge (`SaveManager`, `GameSession`)
3. Editor tools (`*Populator`, `*Setup`, `*Fixer` under `Assets/Game/Scripts/Editor`)

Do not invent systems that are not in the repo. Prefer reading the files below before concluding.
If a file listed in **Primary files** no longer exists, report it under **Checklist gaps** — do not guess a replacement path or behavior.

## Why this exists

Generic reviewers cannot know this project's invariants. Every item below maps to a real symbol in this repo — that is the point of the skill, and the reason a plain "review my code" prompt produces false positives here.

- `IsPlayingStory` aggregates three UI sources (dialogue / choice / ending scroll); checking only the private flag misses endings.
- `phaseChoiceMade` is persisted but set in very few runtime paths — new logic must not depend on it.
- PlayerPrefs keys **are** the save format: renaming a key without migration silently breaks old saves.
- Editor populators rewrite `.asset` content; re-running must not blindly duplicate phases.

## Primary files

| Area | Paths |
|------|--------|
| Story runtime | `Assets/Game/Scripts/Story/StoryEventTreeManager.cs`, `StoryEventDefinition.cs`, `DayStorySchedule.cs`, `StoryCampaign.cs`, `OpeningCgPlayer.cs` |
| Save / session | `Assets/Game/Scripts/Core/SaveManager.cs`, `GameSession.cs` |
| Interaction gate | `Assets/Game/Scripts/Interaction/InteractController.cs` |
| Editor tools | `Assets/Game/Scripts/Editor/*Populator*.cs`, `*Setup*.cs`, `*Fixer*.cs`, `DayStoryScheduleEditor.cs` |
| Data assets | `Assets/Game/Data/*.asset` (campaign / day schedules) |

## Review procedure

1. Establish the change set with a code-only diff: `git status --porcelain -- ':(top)JHTGJ/Assets/'` and `git diff -- ':(top)JHTGJ/Assets/'`. The `:(top)` magic pathspec makes the command work from either the repo root or `JHTGJ/`. Ignore `Library/`, `Temp/`, `Logs/`, `obj/` — this repo tracks generated files (~95% of tracked files are `JHTGJ/Library/`) and an unfiltered diff is unreadable. If no diff is available, review only the files the user names, and say so.
2. Map each change to story / save / editor.
3. Pick the matching checklist — story change → A, save/session → B, editor tooling → C. Run only that section; for the other two write one line: `N/A — no change in <area>`.
4. Every finding needs four fields before it can be reported: `evidence` (file:line) · `trigger` (scene → action → expected vs actual) · `confidence` (high/med/low) · `fixed` (yes/no/pending). A finding without a reproducible trigger may not be labelled **Critical**.
5. Output using **Output format**, then append the run to **Review log**.

---

## Checklist A — Day-based story progression

- [ ] **Schedule source**: Active day comes from `StoryCampaign.GetScheduleForDay(currentDay)` (or fallback `schedule`). Day index clamps / bounds vs campaign length.
- [ ] **Phase advance**: Completing an event calls `AdvanceAfterPhaseChoice` → `currentPhaseIndex++`; past last phase → `CompleteDayLoop` (`currentDay++`, bedroom, phase 0).
- [ ] **WakeUp**: Auto-play only once per day/phase (`autoPlayedWakeUpDay` / `autoPlayedWakeUpPhaseIndex`); player interact blocked during WakeUp.
- [ ] **Night skip**: `ApplyPhaseIndexWithSkips` respects `IncludeNightEvent` when skipping `NightEvent` phases.
- [ ] **Event match**: `FindMatchingEvent` uses `eventId == interactId` first, else `interactKind == kind`. New interact points must match SO `eventId` / kind.
- [ ] **One-event-per-phase**: Finishing any event in a phase advances the whole phase (other events in same phase become unreachable that day). Flag if change assumes multi-event completion.
- [ ] **`phaseChoiceMade`**: Field is saved/loaded but rarely set true in runtime. Flag if new logic depends on it being accurate, or if load with `phaseChoiceMade==1` would block all interacts (`CanInteractWithStory`).
- [ ] **`IsPlayingStory` vs `isPlayingStory`**: Gates (EmergencyStop, InteractController, debug jump) must use **`IsPlayingStory`** (includes Dialogue / Choice / EndingScroll), not only the private flag.
- [ ] **EmergencyStop / endings**: Day 5 requires final phase display name `"最后"`; Day 3–4 → Ending1; Day 5 → Ending2. Flavor uses `hideWifePortrait: true`. Ending path waits a frame before scroll; no Space skip mid-scroll; interactions blocked while ending UI shows.
- [ ] **Post-cooking dining**: Fridge + `CookBreakfast` sets `postCookingDiningActive` + PlayerPrefs; cleared on `CompleteDayLoop`. Background / presence refresh must stay in sync.
- [ ] **Opening CG order**: New game waits for `OpeningCgPlayer.Finished` before `BeginStoryFlow`; continue must not re-require CG.
- [ ] **Hardcoded day limits**: Debug jump / assumptions of days 1–5; adding Day 6 needs campaign + branch review.

## Checklist B — PlayerPrefs save & GameSession

Keys (must stay consistent):

| Key | Meaning |
|-----|---------|
| `JHTGJ_SaveExists` | Has save |
| `JHTGJ_Day` | Day |
| `JHTGJ_Room` | `RoomType` int |
| `JHTGJ_PhaseIndex` | Phase index |
| `JHTGJ_PhaseChoiceMade` | Phase choice flag |
| `JHTGJ_StoryEnded` | Story finished |
| `JHTGJ_PostCookingDining` | Post-breakfast dining state |
| `JHTGJ_PendingStartMode` / `PendingRoom` / `PendingOpeningCg` | Session bridge (not long-term save) |

Checks:

- [ ] **`HasContinuableSave`**: True only if save exists **and** `!storyEnded`. Main menu continue / load button must use this, not bare `HasSave`.
- [ ] **`LoadProgressFromSave`**: Skips applying progress when `ended`; clamps day/phase; restores `postCookingDining` via `IsPostCookingDiningActive`.
- [ ] **`RequestNewGame`**: Deletes save, sets NewGame + Bedroom + PendingOpeningCg.
- [ ] **`RequestContinue`**: Requires continuable save; clears PendingOpeningCg; sets pending room from save.
- [ ] **Autosave timing**: Phase advance / debug jump / return-to-menu should call `SaveProgress` / `TryAutoSaveProgress` when story not ended.
- [ ] **Ending**: After ending scroll → `DeleteSave` + main menu. Do not leave `storyEnded` save continuable.
- [ ] **`NormalizeRoom`**: Invalid room ints fall back safely.
- [ ] **Session vs save**: Pending keys are one-shot across scene load; do not confuse with durable save fields.
- [ ] **No silent key rename**: Renaming PlayerPrefs keys without migration breaks old saves — flag as breaking change.

## Checklist C — Editor tools

- [ ] **Populator vs runtime**: `Day*StoryContentPopulator`, library populators write/edit `.asset` only; must not be required at runtime.
- [ ] **Re-run safety**: Running Populate twice should update SO fields intentionally, not duplicate phases/events blindly (flag destructive Clear without confirmation if present).
- [ ] **Campaign wiring**: After populate, `DefaultStoryCampaign` (or equivalent) still references Day1–5 schedules.
- [ ] **Setup / Fixer menus**: `StorySystemSetup`, `RoomSelectorSetup`, `*UIFixer` should target `SampleScene` / `MainMenu` correctly; warn if they mutate Play Mode state unsafely.
- [ ] **SerializedObject**: Editor scripts apply properties and mark scene/asset dirty (`EditorSceneManager.MarkSceneDirty` / `EditorUtility.SetDirty` / `AssetDatabase.SaveAssets` as appropriate).
- [ ] **Paths**: Hardcoded `Assets/...` paths still exist; missing sprites leave null references — flag silent null assigns.
- [ ] **Debug defaults**: Editor setup must not force `enableDebugShortcuts = true` into Release player builds without `DebugShortcutsUtility` / `#if` protection.
- [ ] **Content typos / IDs**: Populator dialogue or `eventId` strings must match scene `InteractPoint.interactId` (e.g. `Interact_Fridge`, EmergencyStop ids).

## Output format

```markdown
## JHTGJ Review Summary
- Scope: story / save / editor (list what was reviewed)
- Verdict: Pass | Pass with notes | Needs fixes

## Findings
### Critical
- **[area]** `file → symbol`: issue.
  - evidence: `path:line`
  - trigger: 打开 X 场景 → 做某操作 → 期望 vs 实际
  - confidence: high · fixed: no

### Moderate
- ... (same four fields)

### Nit / Tech debt
- ...

## Checklist gaps
- Items that could not be verified (missing files, no diff, etc.)

## Interview-ready note (optional)
- Only if a Critical/Moderate finding above was confirmed **and** fixed. State symptom + fix, no counts that cannot be shown in `git log`. If nothing material: write "no material finding this run".
- Never describe the workflow as "used AI to review code"; describe what the checklist constrains (invariants, evidence, gaps).
```

## Review log (required)

Append every run to `docs/review-log.md` **at the repo root** (`<repo>/docs/review-log.md`; the git root is `Forgetful Clock`, one level above `JHTGJ/` — do not create `JHTGJ/docs/`):

| date | scope | critical | moderate | confirmed | fixed | commit |
|------|-------|----------|----------|-----------|-------|--------|

Never delete rows. `confirmed` means a human reproduced it; `fixed` carries the commit. This file is the only evidence that the review caught something.

## Severity

- **Critical**: Wrong day/phase advance, corrupt/unreadable continue, ending leaves bad save, editor wipe of content.
- **Moderate**: Input/story desync, mismatched interact ids, debug on in release, incomplete dirty/save after editor write.
- **Nit**: Naming, unused `phaseChoiceMade`, comments, non-breaking hardcodes.

## Out of scope (unless asked)

Combat, shaders, full UI visual polish, TMP vs Legacy font debates, unrelated packages.
