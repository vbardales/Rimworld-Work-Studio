# TESTING.md scenario 13, RIMMSQOL's side of the hidden shortcut contract. These scenarios use the
# shared PickleTools companion to drive RIMMSQOL's real settings, persisted choice and main bar.
# The screenshots leave only layout/readability to a person; every functional state is asserted.
@review @rimmsqol @requires:MalteSchulze.RIMMSqol @requires:nelim.pickletools.rimmsqol @requires:nelim.pickletools.interfacescale
Feature: RIMMSQOL reveals and hides the Work Studio shortcut

  Background:
    Given the save "test-colony" is loaded
    And I close all dialogs
    Then mod "MalteSchulze.RIMMSqol" is loaded
    And RIMMSQOL is ready to be driven

  Scenario: RIMMSQOL lists the shortcut hidden while the main bar omits it
    Then RIMMSQOL's own list of main buttons offers "WorkStudio_Settings"
    And RIMMSQOL shows the main button "WorkStudio_Settings" as hidden
    And RIMMSQOL holds no choice for the main button "WorkStudio_Settings"
    And the main bar does not draw the button "WorkStudio_Settings"
    When RIMMSQOL's own window is opened on its list of main buttons
    Then RIMMSQOL's own window is open
    When I take a screenshot "rimmsqol, list containing the Work Studio shortcut"
    And I close all dialogs

  Scenario: the revealed shortcut is persisted, drawn and opens Work Studio settings
    When RIMMSQOL reveals the main button "WorkStudio_Settings"
    Then RIMMSQOL shows the main button "WorkStudio_Settings" as visible
    And RIMMSQOL's settings file records the main button "WorkStudio_Settings" as visible
    And the main bar draws the button "WorkStudio_Settings"
    When RIMMSQOL's own window is opened on the main button "WorkStudio_Settings"
    Then RIMMSQOL's own window is open
    When I take a screenshot "rimmsqol, Work Studio shortcut revealed"
    And I close all dialogs
    And the main bar's button "WorkStudio_Settings" is activated
    Then window "Dialog_WorkStudioSettings" is open
    When I take a screenshot "Work Studio settings opened by the shortcut RIMMSQOL revealed"
    And I close all dialogs

  Scenario: hiding and forgetting the choice removes every RIMMSQOL trace
    Given RIMMSQOL reveals the main button "WorkStudio_Settings"
    And the main bar draws the button "WorkStudio_Settings"
    When RIMMSQOL hides the main button "WorkStudio_Settings"
    Then RIMMSQOL shows the main button "WorkStudio_Settings" as hidden
    And the main bar does not draw the button "WorkStudio_Settings"
    And RIMMSQOL's settings file records the main button "WorkStudio_Settings" as hidden
    When RIMMSQOL forgets its choice for the main button "WorkStudio_Settings"
    Then RIMMSQOL holds no choice for the main button "WorkStudio_Settings"
    And RIMMSQOL's settings file records no choice for the main button "WorkStudio_Settings"

  Scenario: the revealed shortcut remains usable at 150 percent interface scale
    Given Nelim's Pickle Tools: the interface scale is 150 percent
    When RIMMSQOL reveals the main button "WorkStudio_Settings"
    Then the main bar draws the button "WorkStudio_Settings"
    When the main bar's button "WorkStudio_Settings" is activated
    Then window "Dialog_WorkStudioSettings" is open
    When I take a screenshot "Work Studio settings opened from RIMMSQOL at 150 percent scale"
    And I close all dialogs
