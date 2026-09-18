# TESTING.md scenario 13, added 2026-09-18: the settings window, which the original twelve scenarios
# never covered - they are all about the editor and its effects on the Work tab and colonists.
#
# The first scenario is a real assertion, and it closes half of a gap STATUS.md carried as unverified
# since the shortcut was written: the hidden WorkStudio_Settings MainButtonDef's worker is built and
# activated the way RimWorld would, and the window it opens is checked. What stays manual is whether
# RIMMSQOL (or another MainButtons customisation mod) lists the def and can reveal the button at all.
#
# The second is a screenshot, so nothing is asserted there. What to look for:
#   - the intro line, "Open the work type editor", "Import / export a setup" and the save note all
#     read as text, with no raw key and nothing clipped;
#   - "Reset the whole setup" is offered only when there is something to reset - the Background
#     leaves a configuration in place, so it should be there;
#   - the window is the same one Mod options -> Work Studio draws: open both and compare, since they
#     share a single WorkStudioSettings instance and must show the same values.
@review
Feature: the settings window through the MainButtons shortcut

  Background:
    Given the save "test-colony" is loaded
    When I create the work type "Pickle herding"
    And I close the work type editor

  Scenario: the hidden shortcut opens the settings window
    When I open the settings window through the MainButtons shortcut
    Then window "Dialog_WorkStudioSettings" is open

  Scenario: what that window looks like
    When I open the settings window through the MainButtons shortcut
    And I take a screenshot "settings window, opened by the MainButtons shortcut"
