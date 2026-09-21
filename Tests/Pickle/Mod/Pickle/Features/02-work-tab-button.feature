# TESTING.md scenario 2, the one that failed twice. Clicks are real OS input: the pointer moves on
# screen while this runs. The button is named by the translation key its label comes from, not by
# the text: Pickle tags a button by its drawn text, so a hardcoded English literal would silently
# miss it on a French client, exactly the bug found in Architect Studio's suite the same night.
#
# The three "Nelim's Pickle Tools" steps come from PickleTools/ClickDiagnostics, staged by every pass
# map of this suite. They exist because of this scenario's history: under Work Tab the button MOVES
# for about ten frames after the tab opens, and a click aimed at where it was matched nothing. So the
# scenario waits until the button stands still, checks that nothing covers it, and clicks; a click
# that opens nothing prints the pointer, the buttons under it and the window stack.
Feature: the button in the Work tab

  Background:
    Given the save "test-colony" is loaded

  Scenario: Work types… opens the editor
    When I open the "Work" tab
    And Nelim's Pickle Tools: the button keyed "WorkStudio.OpenEditorShort" has stood still
    And Nelim's Pickle Tools: the button keyed "WorkStudio.OpenEditorShort" is reachable in "MainTabWindow"
    When Nelim's Pickle Tools: I click the button keyed "WorkStudio.OpenEditorShort" and the window "Dialog_WorkTypes" opens

  Scenario: the Animals tab does not get the button
    When I open the "Animals" tab
    Then the Work Studio button is not drawn

  Scenario: the Assign tab does not get the button
    When I open the "Assign" tab
    Then the Work Studio button is not drawn
