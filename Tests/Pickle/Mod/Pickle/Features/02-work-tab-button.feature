# TESTING.md scenario 2, the one that failed twice. Clicks are real OS input: the pointer moves on
# screen while this runs.
Feature: the button in the Work tab

  Background:
    Given the save "test-colony" is loaded

  Scenario: Work types… opens the editor
    When I open the "Work" tab
    And I click button "Work types…"
    Then window "Dialog_WorkTypes" is open

  Scenario: the Animals tab does not get the button
    When I open the "Animals" tab
    Then the Work Studio button "Work types…" is not drawn

  Scenario: the Assign tab does not get the button
    When I open the "Assign" tab
    Then the Work Studio button "Work types…" is not drawn
