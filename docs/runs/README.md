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

## Which evidence to keep when a run is cited

A full Pickle report is up to a gigabyte and the disk is shared, so a run that a document cites is
**minified** as soon as it has been read, and only its useful part stays on disk (never in git).

Keep, per cited run:

- `summary.json`, `junit.xml`, `summary.md`: the verdict, a few KB. The text record goes in the day file here.
- The screenshots of this mod's own `@review` scenarios, that is the labels of the `take a screenshot` steps of the
  features, saved as `manual--<label with every non-alphanumeric turned into a dash>--step0.png`. For a set that
  changes what a capture shows (Enhanced Work Tab, Better Work Tab, RIMMSQOL, Work Type Tag) keep only the captures
  that depend on that mod: the Work tab ones, the shortcut ones, the renamed-job one. The reference languages
  (`sans-facultatifs` in English and in French) keep all of them.
- The screenshots a failure names (`Attachment: screenshot -> ...` in `junit.xml`) until the failure's cause is
  written down here.

Delete: `Player.log` (quote the lines that prove something in the day file instead), `messages.ndjson`,
`report.html`, films, every screenshot of another mod (the shared `pickle-reports` folder keeps the leftovers of
other runs), and every report of a set once a newer run of the same set on the same revision exists. A report that
`STATUS.md` or a day file still points to is repointed first, never just deleted.

Sizes seen on 2026-09-23: a full report 0.9-1.2 GB; kept as above, 8-32 MB.
