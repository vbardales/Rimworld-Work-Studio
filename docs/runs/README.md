# Pickle runs

One text summary per run that a document cites. The full reports (screenshots, `Player.log`,
`junit.xml`, up to a gigabyte each) stay on disk in `Tests/Pickle/evidence/<name>`, ignored by git;
the shared `pickle-reports` folder holds one report for the whole machine and rotates, so a report
has to be read (set name, suite and scenario names) and summarised here before it is cited.

| Run | Set | Language | Passed / failed / skipped | exitReason |
| --- | --- | --- | --- | --- |
| [2026-09-22-avec-better-work-tab](2026-09-22-avec-better-work-tab.md) | `avec-better-work-tab` | English | 49 / 0 / 6 | passed |
| [2026-09-22-avec-enhanced-work-tab](2026-09-22-avec-enhanced-work-tab.md) | `avec-enhanced-work-tab` | English | 48 / 1 / 6 | failed |
| [2026-09-22-avec-work-type-tag](2026-09-22-avec-work-type-tag.md) | `avec-work-type-tag` | English | 50 / 0 / 5 | passed |
| [2026-09-22-minimal-english](2026-09-22-minimal-english.md) | `sans-facultatifs` | English | 49 / 0 / 6 | passed |
| [2026-09-22-minimal-french](2026-09-22-minimal-french.md) | `sans-facultatifs` | French | 49 / 0 / 6 | passed |
| [2026-09-23-ewt-full](2026-09-23-ewt-full.md) | `avec-enhanced-work-tab` | English | 49 / 0 / 6 | passed |
| [2026-09-23-ewt-scenario5](2026-09-23-ewt-scenario5.md) | `avec-enhanced-work-tab` | English | 3 / 0 / 0 | passed |
| [2026-09-23-fluffy-scenario2](2026-09-23-fluffy-scenario2.md) | `incompat-fluffy-worktab` | English | 2 / 1 / 0 | failed |
| [2026-09-23-minimal-english](2026-09-23-minimal-english.md) | `sans-facultatifs` | English | 49 / 0 / 6 | passed |
| [2026-09-23-minimal-french](2026-09-23-minimal-french.md) | `sans-facultatifs` | French | 49 / 0 / 6 | passed |
