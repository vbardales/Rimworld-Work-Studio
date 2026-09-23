# 2026-09-22-avec-enhanced-work-tab

- Set: `avec-enhanced-work-tab`, language: English
- Result: **55 scenarios, 48 passed, 1 failed, 6 skipped**, exitReason `failed`
- Full report (screenshots, log, junit): `Tests/Pickle/evidence/2026-09-22-avec-enhanced-work-tab` on disk, ignored by git
- Suites: Work Studio loads and its patches apply (3); the button in the Work tab (3); the three columns say what they are (3); create a work type, and fill it (5); priorities survive a reconfiguration across a save (3); reordering is visible in the editor (2); reordering types and tasks (7); a renamed work type in the Work tab (1); rename a work type (2); hide a column (2); export, reset, import (3); the drift warning's wording (1); the mod list drift warning (2); what removing the mod would leave behind, read from the raw save (1); what the settings window looks like (2); the settings window, both doors (6); images for the Workshop page (3); the declared incompatibility with Fluffy's Work Tab (1); RIMMSQOL reveals and hides the Work Studio shortcut (4); Work Type Tag follows a renamed work type without a restart (1)

## Failed

- **Work types… opens the editor**

```
the click on 'Work types…' (key 'WorkStudio.OpenEditorShort') reached no window named Dialog_WorkTypes in 60 frames. No window on the stack absorbs input, so nothing sits above the button; this message does not say which cause it was: compare where the button was drawn with where the pointer was, both printed below.
Pointer before the click (1389.00, 707.00), after it (1389.00, 707.00).
Where the button was drawn, as Pickle records it (the click goes to the centre):
  Widgets.ButtonText 'Work types…' drawn at (x:1312.00, y:692.00, width:155.00, height:28.00), centre (1389.50, 706.00)
Buttons on the pointer, in draw order (Pickle does not tag image buttons):
  Widgets.ButtonText 'Done editing' at (x:1362.00, y:697.00, width:98.00, height:24.00), drawn #64 this frame
  Widgets.ButtonText 'Work types…' at (x:1312.00, y:692.00, width:155.00, height:28.00), drawn #103 this frame
Window stack, top first:
  #3 top  ImmediateWindow [Assembly-CSharp]  layer=Super  rect=(x:1405.00, y:721.00, width:268.00, height:46.00)  absorbsInput=False  getsInput=True  holdsPointer=False  drawn by Verse.ActiveTip+<>c__DisplayClass12_0.<DrawTooltip>b__0 [Assembly-CSharp]
  #2  ImmediateWindow [Assembly-CSharp]  layer=Super  rect=(x:1712.00, y:8.00, width:200.00, height:130.00)  absorbsInput=False  getsInput=True  holdsPointer=False  drawn by RimWorld.LearningReadout.WindowOnGUI [Assembly-CSharp]
  #1  MainTabWindow_Work [Assembly-CSharp]  layer=GameUI  rect=(x:0.00, y:683.00, width:1478.00, height:362.00)  absorbsInput=False  getsInput=True  holdsPointer=True
  #0  ImmediateWindow [Assembly-CSharp]  layer=GameUI  rect=(x:862.00, y:3.00, width:196.00, height:25.00)  absorbsInput=False  getsInput=True  holdsPointer=False  drawn by Verse.DebugWindowsOpener.DrawButtons [Assembly-CSharp]
```

## Skipped

- Work Tab's startup column snapshot cannot contain a type created at runtime
- RIMMSQOL lists the shortcut hidden while the main bar omits it
- the revealed shortcut is persisted, drawn and opens Work Studio settings
- hiding and forgetting the choice removes every RIMMSQOL trace
- the revealed shortcut remains usable at 150 percent interface scale
- the current job label changes immediately
