# TESTING.md scenario 9. The files go to SaveData/WorkStudio/ under a pickle-workstudio- prefix
# and are deleted after each scenario.
Feature: export, reset, import

  Background:
    When I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"
    And I drag the work type "Cooking" below "Mining"
    And I rename the work type "Hauling" to "Portage"
    And I hide the work type "Cleaning"

  Scenario: everything returns after a reset
    When I remember the whole setup
    And I export the setup as "round-trip"
    And I reset the whole setup
    Then nothing is left to reset
    When I import the setup "round-trip"
    Then the whole setup is as remembered

  Scenario: the export carries no snapshot of this mod list
    When I export the setup as "portable"
    Then the exported file "portable" does not contain "knownWorkTypes"

  @allow-errors
  Scenario: a broken file leaves the current setup whole
    When I export the setup as "whole"
    And I remember the whole setup
    And I import a truncated copy of the setup "whole"
    Then the import is refused
    And the whole setup is as remembered
