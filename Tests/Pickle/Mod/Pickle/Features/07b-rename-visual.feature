# TESTING.md scenario 7, the visual half. 07 already asserts that the column header carries the new
# label and that the column worker was thrown away; what no assertion covers is whether the column
# ends up as WIDE as the new name needs. Nothing here is asserted - the screenshot is the evidence.
#
# What to look for:
#   - the Work tab column that was "Hauling" is headed by the new name, not the old one;
#   - the column is wide enough for it: a stale width means the worker was reused, and shows up as a
#     name cut off or spilling into its neighbour;
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
    And I close the work type editor
    And I open the "Work" tab
    And I let the interface draw
    And I take a screenshot "Work tab, Hauling renamed"
