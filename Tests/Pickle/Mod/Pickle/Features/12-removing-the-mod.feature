# TESTING.md scenario 12, adapted rather than skipped. This suite cannot actually restart RimWorld
# without Work Studio: its own steps run inside a companion mod bound to Work Studio's assembly, so
# testing "the mod is gone" from within it is a contradiction (see the README's "What stays
# manual" table). What it CAN do is read the raw save file and prove the one fact TESTING.md's
# claim rests on: custom types are always appended at the END of DefDatabase<WorkTypeDef> (see the
# README's "Why scenario 05 is built the way it is"), so a mod-less DefDatabase is simply the same
# list with the tail cut off, and vanilla's own positional DefMap loading reads the same leading
# values into the same leading types regardless. Only the trailing, custom-type values would have
# nowhere to go - which is exactly the "only loss" TESTING.md documents.
Feature: what removing the mod would leave behind, read from the raw save

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    And "Keeper" can do "Handling" and "Cleaning"
    When I create the work type "Pickle removed"
    And I move the task "Milk" into "Pickle removed"
    And I set "Keeper" to priority 1 for "Doctor"
    And I set "Keeper" to priority 2 for "Pickle removed"
    And I set "Keeper" to priority 3 for "Hauling"
    And I save the game as "before-removal"

  Scenario: the custom type's priority sits where a mod-less load would simply stop reading
    Then in the raw save "before-removal", the custom types sit at the end of "Keeper"'s vanilla priority list, and nothing else would shift if the mod were gone
