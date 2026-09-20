# TESTING.md scenario 5, the one the mod exists for.
#
# Built so that it fails without the mod's protection. A created type is appended at the end of the
# def database, so creating one shifts nothing; deleting one that sits before another does. Two
# types, a save, the first deleted, the save loaded: positional loading would hand the second
# type the first one's value.
Feature: priorities survive a reconfiguration across a save

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    And "Keeper" is given backstories that disable no work type
    And "Keeper" can do the work types "Handling" and "Cleaning"
    When I create the work type "Pickle first"
    And I move the task "Milk" into "Pickle first"
    And I create the work type "Pickle second"
    And I move the task "CleanFilth" into "Pickle second"
    And I set "Keeper" to priority 1 for "Doctor"
    And I set "Keeper" to priority 2 for "Pickle first"
    And I set "Keeper" to priority 3 for "Pickle second"
    And I set "Keeper" to priority 4 for "Hauling"

  Scenario: deleting a type in a running game keeps every other priority in place
    When I delete the work type "Pickle first"
    Then "Keeper" has priority 3 for "Pickle second"
    And "Keeper" has priority 1 for "Doctor"
    And "Keeper" has priority 4 for "Hauling"

  Scenario: a save written with two types, loaded with one
    When I save the Work Studio test game as "two-types"
    And I delete the work type "Pickle first"
    And I load the Work Studio test game "two-types"
    Then "Keeper" has priority 3 for "Pickle second"
    And "Keeper" has priority 1 for "Doctor"
    And "Keeper" has priority 4 for "Hauling"

  Scenario: a save written with one type, loaded with two
    # The reverse: the second type exists again when the save made without it is read.
    When I delete the work type "Pickle first"
    And I save the Work Studio test game as "one-type"
    And I create the work type "Pickle first"
    And I load the Work Studio test game "one-type"
    Then "Keeper" has priority 3 for "Pickle second"
    And "Keeper" has priority 1 for "Doctor"
    And "Keeper" has priority 4 for "Hauling"
    And "Keeper" has priority 0 for "Pickle first"
