# TESTING.md scenario 6, both levels. Drops are replayed through the callback each column
# registered on its last repaint, so the insertion convention under test is the mod's own. Rows are
# named: a mod list that adds work types or tasks does not move the targets.
#
# With Fluffy's Work Tab or Better Work Tab, the type order may not reach execution once a column
# was dragged there (scenario 11); what is checked below is the order the game and the Work tab
# columns hold.
Feature: reordering types and tasks

  Scenario: a work type dragged downwards lands where it was dropped
    When I drag the work type "Cooking" below "Mining"
    Then the work type "Cooking" comes right after "Mining"

  Scenario: a work type dragged upwards lands where it was dropped
    When I drag the work type "Research" above "Doctor"
    Then the work type "Research" comes right before "Doctor"

  Scenario: the arrows move a type one place
    When I press the down arrow on the work type "Cooking"
    Then the work type "Cooking" moved 1 place down

  Scenario: the up arrow on the first type does nothing
    When I remember Work Studio's whole setup
    And I press the up arrow on the first work type
    Then Work Studio's whole setup is as remembered

  Scenario: a task dragged downwards lands where it was dropped
    Given the tasks of "Handling" include "Slaughter, Milk, Shear, Tame, Train" in that order
    When I drag the task "Slaughter" of "Handling" below "Tame"
    Then the task "Slaughter" of "Handling" comes right after "Tame"

  Scenario: a task dragged upwards lands where it was dropped
    Given the tasks of "Handling" include "Slaughter, Milk, Shear, Tame, Train" in that order
    When I drag the task "Train" of "Handling" above "Milk"
    Then the task "Train" of "Handling" comes right before "Milk"

  Scenario: the down arrow on the last task does nothing
    When I remember Work Studio's whole setup
    And I press the down arrow on the last task of "Handling"
    Then Work Studio's whole setup is as remembered
