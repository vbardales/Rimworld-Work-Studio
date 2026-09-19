# TESTING.md scenario 10. A disabled mod is stood in for by a type the last startup recorded and
# no mod provides now; the check itself is the one StartupInit runs. Needs a save for the dialog
# to have a window stack to open on.
Feature: the mod list drift warning

  Background:
    Given the save "test-colony" is loaded
    When I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"

  Scenario: a vanished type raises the warning once, and not again
    Given the last startup saw a work type "PickleVanishedWork" that is gone now
    When Work Studio runs its startup check
    Then Work Studio opens 1 warning dialog
    When I close all dialogs
    And Work Studio runs its startup check
    Then Work Studio opens 0 warning dialogs

  Scenario: no configuration, no warning
    Given the last startup saw a work type "PickleVanishedWork" that is gone now
    When I reset Work Studio's whole setup
    And Work Studio runs its startup check
    Then Work Studio opens 0 warning dialogs
