# TESTING.md scenario 3, which is visual through and through: it is about what a person can read,
# so the layout itself is reviewed from a screenshot. Each scenario asserts the data state first;
# the report then carries the pixels a person judges.
# carries the screenshots and a person decides. Run the suite once per language and these double as
# the FR/EN display check TRANSLATIONS.md asks for.
#
# What to look for, from TESTING.md scenario 3:
#   - left column: every work type, in the real order, each with the number of tasks it holds;
#   - middle column: titled Tasks of "Handling", holding that type's own tasks;
#   - right column: titled Tasks from other types, each row showing its current type in grey on the
#     right, and a help line underneath naming where a click would send it;
#   - in either language: no raw key (a literal WorkStudio.something), nothing clipped, no overlap.
# If the right column still reads "Add a task" and shows no grey type, the running assembly predates
# the 1.0.1 fix.
@review
Feature: the three columns say what they are

  Background:
    Given the save "test-colony" is loaded

  Scenario: the editor with a work type selected
    When I open the work type editor
    And I select the work type "Handling"
    Then the task "Tame" belongs to "Handling"
    And I take a screenshot "editor, Handling selected"

  Scenario: the right column, searched
    When I open the work type editor
    And I select the work type "Handling"
    And I search the other tasks for "clean"
    Then the task "CleanFilth" belongs to "Cleaning"
    And I take a screenshot "editor, other tasks searched for clean"

  # The help line under the search box carries the selected type's own label, and a player can
  # rename a type to anything. Until 2026-09-19 it was drawn in a fixed 20 px box, so a long name
  # wrapped and clipped. A long name is used here on purpose: this screenshot is where that would
  # show again.
  Scenario: the help line under a long type name
    When I rename the work type "Handling" to "Soins et dressage des animaux de la colonie"
    And I open the work type editor
    And I select the work type "Handling"
    Then a work type labelled "Soins et dressage des animaux de la colonie" exists
    And I take a screenshot "editor, help line under a long type name"
