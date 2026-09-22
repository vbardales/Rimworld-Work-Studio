# TESTING.md scenario 6, the visual half. Feature 06 drives both reorderable callbacks and asserts
# the resulting order. This feature leaves a named downward move on screen and captures it, so the
# only human action is judging the image: row highlight, insertion result and arrow state.
@review
Feature: reordering is visible in the editor

  Background:
    Given the save "test-colony" is loaded

  Scenario: a downward work type drag leaves the row where it was dropped
    When I open the work type editor
    And I drag the work type "Cooking" below "Mining"
    Then the work type "Cooking" comes right after "Mining"
    And I take a screenshot "editor, Cooking dropped below Mining"

  Scenario: a downward task drag leaves the row where it was dropped
    Given the tasks of "Handling" include "Slaughter, Milk, Shear, Tame, Train" in that order
    When I open the work type editor
    And I select the work type "Handling"
    And I drag the task "Slaughter" of "Handling" below "Tame"
    Then the task "Slaughter" of "Handling" comes right after "Tame"
    And I take a screenshot "editor, Slaughter dropped below Tame"
