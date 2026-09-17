# TESTING.md scenario 2, the one that failed twice. Clicks are real OS input: the pointer moves on
# screen while this runs. "I click the Work types button" and "the Work Studio button is not drawn"
# are this mod's own steps (ModSteps.cs), not Pickle's generic "I click button" - Pickle tags a
# button by its drawn text, so a hardcoded English literal would silently miss it on a French
# client, exactly the bug found in Architect Studio's suite the same night.
Feature: the button in the Work tab

  Background:
    Given the save "test-colony" is loaded

  Scenario: Work types… opens the editor
    When I open the "Work" tab
    And I click the Work types button
    Then window "Dialog_WorkTypes" is open

  Scenario: the Animals tab does not get the button
    When I open the "Animals" tab
    Then the Work Studio button is not drawn

  Scenario: the Assign tab does not get the button
    When I open the "Assign" tab
    Then the Work Studio button is not drawn
