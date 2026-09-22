# TESTING.md scenario 7, the visual half. 07 already asserts that the column header carries the new
# label and that the column worker was thrown away; what no assertion covers is whether the column
# remeasures after the rename. The renamed header state is asserted before the screenshot; the
# pixels show whether a long label safely folds to its icon instead of covering other columns.
#
# What to look for:
#   - the Work tab column that was "Hauling" is still identifiable by its icon, with no long label
#     spilling into its neighbours; the full new name remains in the type and its tooltip;
#   - with [baku] Work Type Tag installed, the name in front of a colonist's current job follows too,
#     without a restart (Tests/OffGame proves the cache is cleared; this is seeing it).
# The new name is deliberately long, and deliberately accented: a width bug and an encoding bug both
# show here or nowhere.
@review
Feature: a renamed work type in the Work tab

  Background:
    Given the save "test-colony" is loaded

  Scenario: the column header and width after a rename
    When I rename the work type "Hauling" to "Portage des marchandises"
    Then the Work tab column of "Hauling" is headed "Portage des marchandises" and measured afresh
    And I close the work type editor
    And I open the "Work" tab
    And I let the Work Studio interface draw
    And I take a screenshot "Work tab, Hauling renamed"
    # Leaving the Work tab open would spoil the next scenario in the run: 07 asserts that a rename
    # threw the column's worker away, and a table left on screen rebuilds it on the very next
    # repaint. Found the hard way on 2026-09-18, when adding this file turned 07 red.
    And I close all windows but the main tabs, for Work Studio
