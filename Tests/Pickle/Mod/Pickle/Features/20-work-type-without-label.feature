# BACKLOG.md, "Category UI misaligned when it holds a custom work type": the creator of a competing work type mod fixed a
# misalignment on 2026-10-01 that came from other mods writing a work type with no label. Work Studio's own reads of `label`
# all guard the empty case (read in Source/ the same day); this plays it: a type with no label, added the way a mod loaded
# before Work Studio would have left it, then the editor and the Work tab drawn, with the game log watched for errors.
# Not run when written. No other mod needed: the pass is the default one (no -DepMap).
Feature: a work type written without a label does not break the screens

  Background:
    Given the save "test-colony" is loaded
    And I start watching the game log for errors

  Scenario: the editor and the Work tab draw with a type that has no label
    Given another mod added the work type "PickleLabelless" with no label
    When I open the work type editor
    And I let the Work Studio interface draw
    Then the work type editor lists the work type "PickleLabelless"
    When I close the work type editor
    And I open the "Work" tab
    And I let the Work Studio interface draw
    Then the game log held no error since I started watching
    # Out again, so the scenarios played after this one do not meet it in the database.
    When I close all windows but the main tabs, for Work Studio
    And the other mod's work type "PickleLabelless" is taken out again

  # Found on the 2026-10-06 gallery image: the editor listed three vanilla types by their raw defName, because they carry a
  # labelShort and no label. The name now falls back to the short label, as the Work tab columns do. The check does not spell the names:
  # in French the game translates the label as well (BasicWorker is "Manutention", run fd10 of 2026-10-08, English expectation red).
  Scenario: vanilla types that have no label are named by their short label
    Then Work Studio names the work type "BasicWorker" with a text of the game, not its defName
    And Work Studio names the work type "PatientBedRest" with a text of the game, not its defName
    And Work Studio names the work type "PlantCutting" with a text of the game, not its defName
