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
    When I remember Work Studio's whole setup
    And I export the Work Studio setup as "round-trip"
    And I reset Work Studio's whole setup
    Then nothing is left to reset in Work Studio
    When I import the Work Studio setup "round-trip"
    Then Work Studio's whole setup is as remembered

  Scenario: the export carries no snapshot of this mod list
    When I export the Work Studio setup as "portable"
    Then the exported Work Studio file "portable" does not contain "knownWorkTypes"

  @allow-errors
  Scenario: a broken file leaves the current setup whole
    When I export the Work Studio setup as "whole"
    And I remember Work Studio's whole setup
    And I import a truncated copy of the Work Studio setup "whole"
    Then the Work Studio import is refused
    And Work Studio's whole setup is as remembered
