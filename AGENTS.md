## Project Overview


## Speed Is Not the Goal

Quality and completeness beat finishing fast. Finish the whole task - edge cases,
error paths, no stubs or `TODO`s. If it is bigger than it looked, complete it and
say what it cost rather than quietly narrowing scope.

## Architecture and Design

- Implement requirements radically simply: the least code that satisfies the
  acceptance criteria, and nothing more. Simple in design, never reduced in scope.
- Apply SOLID principles throughout the codebase.
- Organize features and behaviors into vertical slices.
- One file per type. Every class, interface, record, and enum gets its own file,
  named for the type it holds.
- Folders and namespaces agree.

## Incremental Implementation and ATDD - mandatory

Every production feature implementation MUST follow
[Addy Osmani's incremental approach](https://addyosmani.com/blog/ai-coding-workflow/)
combined with acceptance test-driven development (ATDD). No exceptions. Plan small,
reviewable slices, then complete one slice at a time: write Given-When-Then
acceptance criteria, write the acceptance test, and run it to prove it fails for
the expected reason BEFORE writing production code. Implement only what satisfies
that slice, refactor with tests green, and run the relevant regression checks.
Do not move to the next slice until those checks pass. No bulk implementation,
no tests added afterward, and no weakening tests to manufacture a pass. Keep
criteria, tests, and implementation aligned until the entire feature is complete.

Back end: integration tests against the API. Front end: Playwright, using the
Page Object Model - one page object per screen, owning the selectors and the
interactions. Tests state intent; page objects know the DOM. Never put a
selector in a test.

Run frontend tests in Chromium only. Do not configure or run Firefox, WebKit,
or any other browser for frontend testing.

### Never write architecture tests

Never add a test that asserts the shape of the codebase rather than its behavior:
no structure, layout, or naming tests; no banned-API scans; no traceability tests
that parse the specifications. Those constraints belong to the compiler, the
formatter, and review. A test suite exists to prove behavior.
