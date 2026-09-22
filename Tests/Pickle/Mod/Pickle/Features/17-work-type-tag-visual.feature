# TESTING.md scenario 7, the optional [baku] Work Type Tag integration. The scenario gives the
# selected colonist a real current Job carrying HaulGeneral as its WorkGiverDef, renames Hauling,
# asserts the patched job report contains the new label, and captures the selected pawn UI.
@review @requires:baku.worktypetag
Feature: Work Type Tag follows a renamed work type without a restart

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists

  Scenario: the current job label changes immediately
    When I rename the work type "Hauling" to "Portage des marchandises"
    And "Keeper" starts a visible job for the task "HaulGeneral"
    Then "Keeper"'s current job report contains the work type label "Portage des marchandises"
    When I close the work type editor
    And I take a screenshot "Work Type Tag, renamed Hauling on Keeper's current job"
