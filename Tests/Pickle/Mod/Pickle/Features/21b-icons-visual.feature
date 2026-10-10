# TESTING.md, "The icons and their three settings", the visual half of 21-icons.feature. 21 asserts what was drawn and what the header
# answered to the game; what no assertion reaches is whether the icons look right: a skill icon beside its
# name and bar, an icon at the foot of a header that is not clipped or on top of its label, the icon-only
# header still identifiable, the settings window saying why the header choice is unavailable.
#
# What to look for:
#   - Character tab: an icon before each of the twelve skills, the name and the bar not shifted out of the row;
#   - Work tab, icon and label: the icon at the foot of each column under the label, the label legible;
#   - Work tab, icon only: the label gone, every column still readable from its icon, nothing on a neighbour;
#   - Work tab, label only: the game's own header, no icon anywhere;
#   - settings, column icons off: the three header choices greyed out with the explanation line.
# Written 2026-10-10, not run.
@review
Feature: what the icons look like

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists

  Scenario: the skill list with its icons
    When I turn the Work Studio skill icons on
    And I open the Work Studio Character tab of "Keeper"
    And I let the Work Studio icons draw for a moment
    Then I take a screenshot "Character tab, skill icons on"
    And I close the Work Studio Character tab

  Scenario: the Work tab headers, icon and label
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "icon and label"
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then I take a screenshot "Work tab, icon and label"
    And I close all windows but the main tabs, for Work Studio

  Scenario: the Work tab headers, icon only
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "icon only"
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then I take a screenshot "Work tab, icon only"
    And I close all windows but the main tabs, for Work Studio

  Scenario: the Work tab headers, label only
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "label only"
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then I take a screenshot "Work tab, label only"
    And I close all windows but the main tabs, for Work Studio

  Scenario: the settings window with the column icons off
    When I turn the Work Studio column icons off
    And I open Work Studio's settings through the MainButtons shortcut
    And I let the Work Studio icons draw for a moment
    Then I take a screenshot "Settings, column icons off"
    And I close Work Studio's settings window
