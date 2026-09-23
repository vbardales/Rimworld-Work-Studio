# 2026-09-23-fluffy-scenario2

- Set: `incompat-fluffy-worktab`, language: English
- Result: **3 scenarios, 2 passed, 1 failed, 0 skipped**, exitReason `failed`
- Full report (screenshots, log, junit): `Tests/Pickle/evidence/2026-09-23-fluffy-scenario2` on disk, ignored by git
- Suites: the button in the Work tab (3)

## Failed

- **Work types… opens the editor**

```
the click on 'Work types…' (key 'WorkStudio.OpenEditorShort') reached no window named Dialog_WorkTypes in 60 frames. No window on the stack absorbs input, so nothing sits above the button; this message does not say which cause it was: compare where the button was drawn with where the pointer was, both printed below.
Pointer before the click (1058.00, 774.00), after it (1058.00, 774.00).
Where the button was drawn, as Pickle records it (the click goes to the centre):
  Widgets.ButtonText 'Work types…' drawn at (x:1754.00, y:759.00, width:155.00, height:28.00), centre (1831.50, 773.00)
Buttons on the pointer, in draw order (Pickle does not tag image buttons):
  no drawn button contains the pointer
Window stack, top first:
  #2 top  ImmediateWindow [Assembly-CSharp]  layer=Super  rect=(x:1712.00, y:8.00, width:200.00, height:130.00)  absorbsInput=False  getsInput=True  holdsPointer=False  drawn by RimWorld.LearningReadout.WindowOnGUI [Assembly-CSharp]
  #1  MainTabWindow_WorkTab [WorkTab]  layer=GameUI  rect=(x:0.00, y:750.00, width:1920.00, height:295.00)  absorbsInput=False  getsInput=True  holdsPointer=True
  #0  ImmediateWindow [Assembly-CSharp]  layer=GameUI  rect=(x:862.00, y:3.00, width:196.00, height:25.00)  absorbsInput=False  getsInput=True  holdsPointer=False  drawn by Verse.DebugWindowsOpener.DrawButtons [Assembly-CSharp]
```
