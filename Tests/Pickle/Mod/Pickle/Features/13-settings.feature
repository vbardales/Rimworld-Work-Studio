# TESTING.md scenario 13: the settings window, both doors. These assert; the screenshot half lives
# in 13b. Written 2026-09-18 to close three of the four things STATUS.md names as holding
# settings_audit at partial - the Mod options door, a configuration surviving a reload, and the
# reset confirmation. Feature 16 drives RIMMSQOL's side through the shared PickleTools companion.
Feature: the settings window, both doors

  Background:
    Given the save "test-colony" is loaded

  Scenario: the hidden shortcut opens the settings window
    When I open Work Studio's settings through the MainButtons shortcut
    Then window "Dialog_WorkStudioSettings" is open

  Scenario: Mod options opens the same settings
    When I open Work Studio's settings through Mod options
    Then window "Dialog_ModSettings" is open
    And the Mod options window is drawing Work Studio's own settings

  # The nearest thing to a restart one process can do: leaving the Mod options window is what makes
  # vanilla write the file (its PreClose calls WriteSettings), then the settings held in memory are
  # thrown away and the file read again. A configuration that survives that survives a restart,
  # short of the def database being rebuilt - which is scenario 12's territory, not this one.
  Scenario: a configuration survives being re-read from disk
    When I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"
    And I hide the work type "Cleaning"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    And I close Work Studio's settings window
    And Work Studio's settings are re-read from disk, as a restart would
    Then a work type labelled "Pickle herding" exists
    And the task "Milk" belongs to "Pickle herding"
    And the Work tab has no column for "Cleaning"

  # The whole point of a confirmation is that the first click destroys nothing. The button only
  # draws when there is something to reset, which is why the scenario creates a type first.
  Scenario: the reset button asks before it destroys anything
    When I create the work type "Pickle herding"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    And I click the Work Studio button keyed "WorkStudio.Settings.ResetAll"
    Then Work Studio asks to confirm first
    And a work type labelled "Pickle herding" exists

  Scenario: confirming the reset clears the configuration
    When I create the work type "Pickle herding"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    And I click the Work Studio button keyed "WorkStudio.Settings.ResetAll"
    And I wait for Work Studio's confirmation to become clickable
    And I click the Work Studio button keyed "Confirm"
    Then nothing is left to reset in Work Studio
    And no work type labelled "Pickle herding" exists

  Scenario: going back from the confirmation leaves the setup whole
    When I create the work type "Pickle herding"
    And I close the work type editor
    And I remember Work Studio's whole setup
    And I open Work Studio's settings through Mod options
    And I click the Work Studio button keyed "WorkStudio.Settings.ResetAll"
    And I click the Work Studio button keyed "GoBack"
    Then Work Studio's whole setup is as remembered
