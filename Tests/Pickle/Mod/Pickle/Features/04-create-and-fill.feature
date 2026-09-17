# TESTING.md scenario 4: def creation, task reassignment and the column rebuild, all at runtime.
Feature: create a work type, and fill it

  Scenario: a new type gets a column at once
    When I create the work type "Pickle herding"
    Then a work type labelled "Pickle herding" exists
    And the Work tab has a column for "Pickle herding"

  Scenario: a task moves into it and leaves its old type
    When I remember how many tasks each work type holds
    And I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"
    Then the task "Milk" belongs to "Pickle herding"
    And "Handling" no longer lists the task "Milk"
    And "Handling" holds 1 task fewer than before

  Scenario: the task goes home when the type is deleted
    When I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"
    And I delete the work type "Pickle herding"
    Then no work type labelled "Pickle herding" exists
    And the task "Milk" belongs to "Handling"

  Scenario: a colonist's cell for the new type takes a priority
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    When I create the work type "Pickle cleaning"
    And I move the task "CleanFilth" into "Pickle cleaning"
    And I set "Keeper" to priority 2 for "Pickle cleaning"
    Then "Keeper" has priority 2 for "Pickle cleaning"
    And "Keeper" would pick up the task "CleanFilth" while working

  @slow
  Scenario: a colonist assigned only to the new type goes and does its task
    Given the save "test-colony" is loaded
    And a colonist "Porter" exists
    When I create the work type "Pickle hauling"
    And I move the task "HaulGeneral" into "Pickle hauling"
    And "Porter" does nothing but "Pickle hauling"
    And I spawn a "Steel" at (152, 155)
    And game speed is ultrafast
    Then I wait for "Porter" to have job "HaulToCell"
