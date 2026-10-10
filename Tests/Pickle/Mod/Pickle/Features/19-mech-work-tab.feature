# BACKLOG.md, "Mech Work Tab: untested, undeclared" and its follow-up of 2026-10-06: a player asked the creator of a
# competing work type mod whether it, like Personal Work Categories, empties the mechanoid work priorities set in Mech Work
# Tab after a save and a load. Work Studio walks every pawn, mechanoids included, and rewrites priorities by type name after
# loading; what Mech Work Tab stores itself and when is not read. Not run when written.
#
# Pass: wsl-deps.avec-mechworktab.map, in English. Every other pass skips this feature.
# Mech Work Tab is staged from the owner's Windows Workshop subscription (2026-10-06, after the first ticket found no copy of it in
# the staging). Its Workshop item is under a Steam Workshop protection, not removed.
@requires:spacemoth.mechtab
Feature: mechanoid priorities survive a save and a load, with Mech Work Tab

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists
    And the Work Studio test colony owns a mechanoid of kind "Mech_Lifter" named "Hoist"
    And I set the mechanoid "Hoist" to priority 2 for "Hauling"

  Scenario: a mechanoid's priority is the same after a save and a load
    When I save the Work Studio test game as "mech-plain"
    And I load the Work Studio test game "mech-plain"
    Then the mechanoid "Hoist" has priority 2 for "Hauling"

  Scenario: and the same when a type was deleted between the save and the load
    When I create the work type "Pickle first"
    And I save the Work Studio test game as "mech-shifted"
    And I delete the work type "Pickle first"
    And I load the Work Studio test game "mech-shifted"
    Then the mechanoid "Hoist" has priority 2 for "Hauling"
