# Runs

One text file per day of testing, written from the run reports. It is the only record of a run that
lives in git, and the same convention as the other mods of this workspace (`ContentedLivestock`,
`SkillIcons`, ...).

The evidence itself - Pickle reports, `Player.log`, screenshots, films - stays **on disk**, under
`Tests/Pickle/evidence/`, and is ignored by git: a full pass is up to a gigabyte, mostly PNGs that never
diff. The shared `pickle-reports` folder holds one report for the whole machine and rotates, so a report
is read (set name, suite and scenario names, the launcher's totals) and summarised here before it is
cited, and its archive folder gets a `keep.txt` if captures are still to be reviewed.

What a summary here carries, since the media are not beside it:

- the set (`setName`), the language, the revision tested, `exitReason`, and discovered / passed / failed / skipped
- for a failure, the cause as read from the report, or "not established"
- what a person actually opened and saw in the captures, and what they did not show
- where the media are on disk

A summary without those is a claim, not a record. The evidence directory is not backed up: if the
machine is lost, these files and the history are what remains.

An earlier attempt committed the verdict files (`summary.json`, `junit.xml`) and a generated file per
run; it was dropped for this hand-written daily record.

| Day | Contents |
| --- | --- |
| [2026-09-22](2026-09-22.md) | Five full passes by another session; the Enhanced Work Tab regression |
| [2026-09-23](2026-09-23.md) | Scenario 5 confirmed, scenario 2 under Work Tab failed, three full passes |
