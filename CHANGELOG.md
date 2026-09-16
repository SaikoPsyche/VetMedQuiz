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

### Known issues, not addressed here

- `GameManager.Settings()` is wired to the settings button on the start screen
  but does not exist, so that button does nothing.
- Three `PlayClickAudio()` button events have no target object assigned.
- True/False questions have only two options, so the C and D buttons render blank
  but stay clickable, and clicking one counts as a wrong answer.
- `questionTime` reads as a per-question timer but has always been a whole-quiz
  timer. The field name is kept because renaming it would break the scene's
  serialized reference.
- The repository has no README, and `VetMedQuiz_BurstDebugInformation_DoNotShip/`
  is committed with no matching `.gitignore` entry.
