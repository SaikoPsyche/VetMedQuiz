# Changelog

## v2 — 2026-09-16

Everything below is unreleased and lives on the `v2` branch.

### Question banks

Both banks grew from 10 questions to 42. The original 10 in each file were
left exactly as they were, apart from the spelling corrections noted further
down.

- **Student** (`Assets/Resources/VetMedQuestions.json`, difficulty 0) — 32 new
  entry-level questions: clinical abbreviations, normal vitals, directional
  anatomy, zoonoses, parasite transmission, restraint, radiation safety,
  sharps handling and clinic protocol.
- **Vet Tech** (`Assets/Resources/VetMedQuestions_VetTech.json`, difficulty 1)
  — 32 new credentialed-level questions: dosage and fluid-rate calculations,
  anaesthesia monitoring and reversal agents, breathing circuits and oxygen
  supply, haematology and urinalysis interpretation, dentistry, radiographic
  exposure factors, CPR per RECOVER, toxicology and transfusion medicine.

Correct answers in the new questions are spread evenly across all four
positions. In the original 20, 18 of the answers sat on A or B, which is
guessable.

### Randomised quizzes

A quiz is still 10 questions, but they are now drawn at random from the full
bank of 42, so retaking a quiz gives a different set. Selection uses a partial
Fisher–Yates shuffle over a copy of the bank: a uniform sample, no repeats, and
the loaded asset is never reordered. Restarting reloads the scene and deals a
fresh set.

The quiz length is a serialized field (`questionsPerQuiz`, default 10) rather
than a magic number, and it clamps to the bank size.

`ShowQuestions` was re-parsing the entire JSON asset once per question. Beyond
the waste, that would have re-rolled the random draw between questions. Parsing
and selection now happen once, in `Awake`.

### Timer driven by the questions drawn

Every question carries a `secondsToAnswer` field, estimated from a reading plus
processing model rather than chosen by hand:

    seconds = round_to_5( words x 0.4 + processing )   clamped to 15-90

`words` counts the stem and all four options, at an effective 150 wpm — slower
than casual prose because the text is clinical and because options get re-read.
`processing` is thinking time by what the question asks for: 10s to retrieve a
fact, 20s to interpret a finding or answer a NOT/EXCEPT, 45s to work a
calculation.

That gives a 21.8s mean for the student bank (range 15–45s) and 32.0s for vet
tech (range 15–60s), comfortably inside the ~63s per question the VTNE allows.

`QuizManager` sums these across the ten questions actually drawn, adds a 15
second cushion, then rounds to the nearest 30s so the player starts on a round
number. The clock can span 3m00s to 6m00s on the student bank and 3m30s to 8m30s
on vet tech; over 200k random draws it clustered on 4m00s (52% of draws) and on
5m00s-6m00s (83%) respectively. Rounding can only give back the cushion, never
cut into the questions' own estimates.

The cushion and the rounding step are serialized fields.

### Fixes

- **The save chain never worked.** It was broken in four places at once:
  - `StartScreen.unity` wires both difficulty buttons to `SetDifficulty(int)`,
    which was defined nowhere in the project, and then to `SaveData` with no
    argument while the only `SaveData` took an `int`. Neither call resolved, so
    Unity dropped both and `EventManager.AddTester` was never reached.
    `StartScreenManager` now provides the signatures the scene already asks for,
    so the scene itself did not need changing.
  - `TesterDataManager.testTakers` was declared but never assigned, so the first
    `List.Add` would have thrown.
  - The write side used `BinaryFormatter` on a `TestTaker` that was not marked
    `[Serializable]`, while the read side parsed the same file as JSON.
  - `QuizManager.SaveFinalScore` set `scoreText` only in the branch that runs
    when no save file exists, so the score display was inverted and the score was
    never written back.

  Persistence now goes through a single owner, `TesterDataStore`, which makes the
  format mismatch impossible to reintroduce. `BinaryFormatter` is gone — it is
  obsolete in current .NET and unsafe against a user-editable file.

- **No questions loaded without a save file.** `LoadQuizDifficulty` left
  `_vetMedText` null whenever the file was missing, so a clean install got an
  empty quiz. It now falls back to the student bank, and opening `Quiz.unity`
  directly gives a playable quiz.

- **The end of the quiz was not latched.** `QuestionTimer` runs from `Update`, so
  once the clock reached zero it called `SaveFinalScore` on every frame,
  rewriting the save file continuously. It now ends the quiz once and clamps the
  displayed clock at zero.

- **The settings button on the start screen did nothing.**
  `StartScreen.unity` wires it to `GameManager.Settings()`, which existed nowhere
  in the project, so Unity dropped the call. The method now raises an event that
  the `Settings` component listens for. It does not load the Settings scene:
  `GameManager.Start` already adds that scene additively, so the panel is present
  and only needed toggling, and the panel lives in a different scene so it cannot
  be reached by inspector reference from the start screen.

- **The score was hard coded to a 10 question quiz** (`score * 10`). It is now
  derived from the number of questions actually asked.

- **The clock only ever displayed seconds within the current minute**, so a quiz
  timed at 3m52s showed `00:52secs`. It now shows minutes too.

- **Misspellings in the original questions.** Two were pathogen names in the 4DX
  question: `Ehrlicia` to `Ehrlichia` and `Leptosporosis` to `Leptospirosis`,
  each twice. Also `dermataphyte` to `dermatophyte`, `floation` to `flotation`,
  `scrapinhg` to `scraping`, `WHat` to `What`, three `thats` to `that's`, and
  `Cushings Disease` to `Cushing's Disease`. Display text only — no answer keys,
  option ordering or timings changed.

### UI Toolkit port

The three Canvas scenes were replaced by five UIDocuments inside `Quiz.unity`, so
the C# that drove the old uGUI hierarchy was rewritten against it.

`UIManager` is new and owns the views: it finds the five documents by their visual
tree asset name (no inspector wiring), caches the elements, registers the button
callbacks, and switches screens. Screens are hidden with `style.display` rather
than by deactivating the GameObject, because a UIDocument rebuilds its visual tree
whenever it is re-enabled, which would invalidate every cached element and drop
every registered callback.

`QuizManager` keeps the quiz logic unchanged - the draw, the timer, the scoring -
and lost its `Button[]`, `TextMeshProUGUI` and `SetActive` fields.

Fixes that the merge into one scene made necessary:

- **The quiz used to start before the player chose anything.** `Awake` loaded a
  bank, drew the questions and started the clock. That was safe while the start
  screen was a separate scene loaded earlier; in one scene it ran immediately, so
  the bank came from whatever was last written to disk and the countdown ran
  during the welcome screen. `BeginQuiz(difficulty)` is now called when a level is
  chosen, and the clock only ticks while a quiz is running.
- **`GameManager.Start` loaded a scene that no longer exists.** It additively
  loaded Settings at build index 2 on every launch. Removed; the settings panel is
  a document in this scene, shown via `EventManager.ToggleSettings`.
- **Build settings listed two deleted scenes.** `StartScreen.unity` and
  `Settings.unity` were still entries, which also broke the `buildIndex + 1` and
  `LoadScene(0)` arithmetic in `GameManager`. Only `Quiz.unity` remains.
- **True/False questions** now hide the two unused answer buttons via
  `DisplayStyle.None` instead of showing them blank but still clickable.
- **Button sounds** go through new `EventManager` events, since a `VisualElement`
  cannot carry the UnityEvent wiring the uGUI buttons used.
- **`PlayerData.UpdateEndDisplayText`** takes the real question count. It hard
  coded 10, but `questionsPerQuiz` is settable and clamps to the bank size, so a
  quiz of a different length would have reported "12/10".

Data split, as agreed: `PlayerData` carries the run in progress and drives the
bound labels; `TesterDataStore` writes the finished result to disk, because a
ScriptableObject does not persist its runtime values in a build.

### Known issues, not addressed here

- `LevelScreen.uxml` binds `PlayerName` to the name field's `placeholderText`,
  which is display only, so the binding never captures what the player types.
  `UIManager` reads `TextField.value` directly instead. Binding `value` with
  `binding-mode="TwoWay"` would let the binding do it.
- `Settings.cs` and `StartScreenManager.cs` are superseded by `UIManager` and are
  in no scene. `Settings.cs` still subscribes to `OnToggleSettings`, so it would
  double-handle the toggle if it were ever added back to a scene.
- `GameManager.HomeScreen` and `GameManager.StartGame` load scene indices that no
  longer exist. Nothing calls them; they are marked but not deleted.
- The settings document sits below the others in the panel sort order, so
  `UIManager` raises it while open and lowers it on close.
- True/False questions have only two options, so the C and D buttons render blank
  but stay clickable, and clicking one counts as a wrong answer.
- `questionTime` reads as a per-question timer but has always been a whole-quiz
  timer. The field name is kept because renaming it would break the scene's
  serialized reference.
- The repository has no README, and `VetMedQuiz_BurstDebugInformation_DoNotShip/`
  is committed with no matching `.gitignore` entry.
