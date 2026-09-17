# TESTING.md scenario 8: hiding removes the column and nothing else.
Feature: hide a column

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    And "Keeper" can do "Cleaning" and "Hauling"
    When I set "Keeper" to priority 2 for "Cleaning"

  Scenario: the column goes, the work stays
    When I hide the work type "Cleaning"
    Then the Work tab has no column for "Cleaning"
    And "Keeper" has priority 2 for "Cleaning"
    And "Keeper" would pick up the task "CleanFilth" while working

  Scenario: showing it again brings the column back with the priority
    When I hide the work type "Cleaning"
    And I show the work type "Cleaning"
    Then the Work tab has a column for "Cleaning"
    And "Keeper" has priority 2 for "Cleaning"
