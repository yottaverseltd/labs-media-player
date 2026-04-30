# AGENTS

Rules for humans and agents touching this repo.

## Prose

- No em-dashes. Use period, comma, colon, parens, or a line break.
- No AI-smell phrases: "Let's", "We will", "Here we", "Simply", "Essentially", "Imagine", "In essence", "At its core", "This function", "Now we".
- No tutorial tone. Senior voice. Short paragraphs.
- No emojis.

## Code

- Nullable enabled. Warnings-as-errors. Analyzer level latest.
- Comments explain why, not what. Only add a `// why` note for non-obvious invariants, constraints, or trade-offs.
- Colors resolve through tokens in `LabsMediaPlayer/Styles/Tokens.xaml`. Page XAML avoids raw hex brushes except merged dictionaries.
- Motion durations resolve through resources in `LabsMediaPlayer/Styles/Motion.xaml`.
- File-scoped namespaces. `sealed` by default for leaf classes.

## Commits

- Imperative, lowercase subject.
- No trailing period on the subject.
- Body optional, wrap at 72.

## Allowed here

- HTTP GET for RSS via configured Worker proxy URL or direct desktop fetch.
- No telemetry, analytics, third-party trackers beyond GitHub Actions Pages deploy.

## Forbidden

- Provider API keys or secrets in source or client binaries.
- Brush color literals in page markup where a named token exists.
