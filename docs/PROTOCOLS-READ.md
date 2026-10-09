# Protocols read

Which rule documents this mod's session read, at which version, so that after a context compaction only what moved is re-read
(`Rimworld-Ticket-Dispatcher/docs/WELCOME.md`, point 5). The version of a file is the last commit that touched it. To see what
moved since: `git log --oneline <hash>..HEAD -- <file>` (protocol files: `git --git-dir=../rimworld-protocols.git --work-tree=. log ...`).
The 2026-09-25 reading is superseded; see `docs/runs/protocols-read-2026-09-25.md`.

**Read 2026-10-09.** All files clean (no `M`) when read.

| File | Version read | Read |
| --- | --- | --- |
| `AGENTS.md` | `5f4e2dd`, 2026-10-09 | full: mod workflow, test evidence, publishing by CI, closing pass |
| `AUDIT.md` | `5f4e2dd`, 2026-10-09 | lines 121-183 and 279-281: code review, gallery, cleanup at published, stage codes, session title |
| `PUBLISHING.md` | `8a4f67d`, 2026-10-09 | "Images" and the sections from "Au moment d'envoyer" to "À chaque mise à jour" |
| `Rimworld-Ticket-Dispatcher/docs/WELCOME.md` | `53c0ad2`, 2026-10-09 | from "Bienvenue sur la file Pickle" to the end (headings and the points that apply) |
| `Rimworld-Ticket-Dispatcher/docs/SUBMIT.md` | `a3c1527`, 2026-10-09 | full |
| `Rimworld-Ticket-Dispatcher/README.md` | current checkout, 2026-10-09 | full |

Not read this time: `TRANSLATIONS.md`, `MOD_SETTINGS.md`, `STYLE_RIMWORLD.md`, `OPERATIONS.md` (nothing in this mod's text, settings or workflow changed; read `OPERATIONS.md` before any workflow, tag, release or Steam secret).
