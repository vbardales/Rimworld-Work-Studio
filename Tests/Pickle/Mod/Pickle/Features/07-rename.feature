# TESTING.md scenario 7. The header draws labelShort, and the column worker measures it once and
# keeps the width, so a rename must reach both. Checked straight after the rename, before any draw
# would build a new worker.
Feature: rename a work type

  Scenario: the header and its width follow the new name
    When I rename the work type "Hauling" to "Portage"
    Then the Work tab column of "Hauling" is headed "Portage" and measured afresh

  Scenario: an empty name goes back to the original
    When I rename the work type "Hauling" to "Portage"
    And I rename the work type "Hauling" to ""
    Then nothing is left to reset
