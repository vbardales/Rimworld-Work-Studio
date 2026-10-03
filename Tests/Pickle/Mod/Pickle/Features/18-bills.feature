# Work Studio#2: a player reports that with the mod on, bills at a campfire and other furniture are
# never picked up and the right-click "prioritize" option is missing. Not reproduced when written.
Feature: bills at a workbench still reach the colonists

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    And "Keeper" is given backstories that disable no work type
    And "Keeper" can do the work types "Cooking" and "Cleaning"

  Scenario: every bill task is still reachable with the default configuration
    When I set "Keeper" to priority 1 for "Cooking"
    Then every bill task is listed under its own work type and no other
    And "Keeper" goes through every bill task of the types they work

  Scenario: a campfire bill is offered for prioritizing
    When I set "Keeper" to priority 1 for "Cooking"
    And a fuelled campfire with the bill "CookMealSimple" stands next to "Keeper"
    Then right-clicking the campfire offers "Keeper" an option about the bill "CookMealSimple"

  Scenario: bill tasks stay reachable after a task is moved into a created type
    When I create the work type "Pickle kitchen"
    And I move the task "DoBillsCook" into "Pickle kitchen"
    And I set "Keeper" to priority 1 for "Pickle kitchen"
    Then every bill task is listed under its own work type and no other
    And "Keeper" goes through every bill task of the types they work
