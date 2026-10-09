# BACKLOG.md history (moved 2026-10-09)

Section done and removed from BACKLOG.md. Full former file: `git show 139fcf1:BACKLOG.md`.

## Pickle: a click that lands on a control while the layout is still moving

Raised 2026-09-21 as "say why a click was lost to a control that is not a text button", and
validated by Virginie as a real need on the strength of an explanation that turned out to be wrong
three times over. Rewritten the same evening once a trace settled it; the history is kept below
because the reasoning is the useful part. Not Work Studio code: any work is upstream, in
RimWorks/Rimworld-Pickle, and would go through Virginie first. To be taken up on Wednesday
2026-09-23. Move it if a Pickle backlog ever exists.

**What actually happened, measured.** "Work types..." with Fluffy's Work Tab loaded: Pickle clicked
and `Dialog_WorkTypes` never opened. A Pickle build that traces its own `TagStore.Record`, run beside
this suite's own probe in the same game, gave identical numbers on both sides for every frame: raw
rectangle, converted rectangle, identity GUI matrix, origin (6, 756). Pickle's conversion is correct.
What moved is the button. For frames 216-225 it was drawn at raw x 975 (centre 1058.5), and from frame
226 at raw x 1748 (centre 1831.5): with Work Tab the tab window is drawn narrow for about ten frames
after it opens and then widens to the full screen, and the button is anchored to its right edge.
Pickle resolved the tag at 975, moved the pointer there (its log: `pointer at (1058.50, 773.00)`,
OS and game agreeing) and pressed; the window widened before the release; IMGUI counts a click only
when press and release land on the same control. Nothing is wrong with the button for a person, who
does not click within a fifth of a second of opening a tab, and nothing is wrong with Pickle's
rectangle at any instant.

**What survives as a possible upstream idea, and how weak it is.** Pickle resolves a tag once, moves
the pointer, and clicks the rectangle it resolved. A control that is still moving loses the click
silently, and the report says only that the window did not open. A "wait until this tag has stood
still for N frames" step, or a guard that re-reads the rectangle at the click, would turn that into a
clear message. That is a convenience for suites, not a defect; this suite now does it itself
(the step `the button keyed {string} has stood still` of `PickleTools/ClickDiagnostics`, moved there from `ModSteps.cs` on 2026-09-21), which is the cheaper answer. Virginie then asked for a pull request to Pickle for it and for the lost-click report; both are in `PickleTools/Upstream/PENDING.md` and a chip session prepares them.

**What is still open from the other diagnostics.** With Enhanced Work Tab the same step aimed at the
exact centre of the drawn button, and a TEXT button drawn earlier (98x24, drawn #64 against #103)
also contained that point. Pickle tags text buttons, so its own store could have listed the overlap;
that is a real, measured case for "list the tags whose rectangle contains the click", and the only
one. The window-stack dump (per window: layer, rect, `absorbInputAroundWindow`, `WindowStack.GetsInput`,
owner of an `ImmediateWindow`) answers "a window swallowed it", which has never been seen. Recording
unnamed controls, `Widgets.ButtonImage` and the rest, would need new hooks and is a design choice
for upstream, so an issue first and not a PR. None of it has a case that needs it yet.

**The three explanations that were wrong, in order, kept on purpose.**

1. *One of Work Tab's three 30x30 image toggles took the click.* Computed from the layout, never
   measured. The pointer was not on the button, and no image button contained it.
2. *Something moved the pointer after the click.* Pickle's own log showed it aimed at (1058.5, 773)
   and the OS and game agreed it arrived and stayed.
3. *Pickle stored a rectangle 773 px to the left of the button, and 773 also equals the centre's
   y coordinate, which may be a transform.* The trace showed Pickle's rectangle was right at every
   frame and the 773 was a coincidence: it is simply the distance between the two window widths.

Each was stated with more confidence than its evidence carried, and each was withdrawn the moment a
measurement disagreed. The lesson for next time is the order: measure at the moment of the click,
frame by frame, before forming a theory about where it landed.

