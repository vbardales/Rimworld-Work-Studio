# TESTING.md scenario 1. No save needed: everything here is settled at the main menu.
Feature: Work Studio loads and its patches apply

  Scenario: the mod loads after Harmony
    Then mod "nelim.workstudio" is loaded
    And mod "nelim.workstudio" loads after "brrainz.harmony"

  Scenario: the three patches are in place
    Then Work Studio patched "RimWorld.Pawn_WorkSettings::ExposeData"
    And Work Studio patched "Verse.Pawn::GetDisabledWorkTypes"
    And Work Studio patched the draw method of the window the Work tab really uses

  Scenario: nothing from Work Studio in the startup log
    # "Could not find the Work tab window", "Could not add the button", an exception naming a
    # WorkTypeRuntime step: each announces itself here and nowhere else.
    Then the game log holds nothing from Work Studio since startup
