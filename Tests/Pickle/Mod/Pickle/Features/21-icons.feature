# TESTING.md, "The icons and their three settings". The description promises an icon for each skill in the character tab and
# one at the foot of each Work tab column, and three settings that choose what shows (skill icons, column
# icons, and what a column header shows: icon and label, icon only, label only). Until 2026-10-10 nothing
# played them: the textures were checked off-game, the choices were never changed on screen.
#
# A drawing is not asserted from pixels. A probe (IconSteps.cs) counts what Work Studio hands to
# GUI.DrawTexture and what its header prefix answers to the game, while the real tab is open. It counts
# "some" or "none", never how many: a draw passes through several overloads. The pixels are 21b's.
#
# The header answer is the game's: "left to the game" means Work Studio drew its icon and let the vanilla
# label draw too; "took over" means icon only, the vanilla label is not drawn (the tooltip carries the name).
#
# The off-game half (defaults, persistence, a mode out of range held to the nearest, a file without the
# keys, an export that leaves the choices out) is Tests/OffGame, TheIconSettings. Written 2026-10-10 from the
# code, not run: every step of the probe is new.
Feature: the icons, and the three settings that choose them

  Background:
    Given the save "test-colony" is loaded
    And a colonist "Keeper" exists

  Scenario: first use shows every icon
    Then a new Work Studio settings object starts with the skill icons on, the column icons on and the header showing icon and label

  Scenario: the skill list carries an icon for each skill
    When I turn the Work Studio skill icons on
    And I start counting the icons Work Studio draws
    And I open the Work Studio Character tab of "Keeper"
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew a skill icon
    And I close the Work Studio Character tab

  Scenario: with the skill icons off the skill list draws none
    When I turn the Work Studio skill icons off
    And I start counting the icons Work Studio draws
    And I open the Work Studio Character tab of "Keeper"
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew no skill icon
    And I close the Work Studio Character tab

  Scenario: the skill icons stay on when only the column icons are turned off
    When I turn the Work Studio skill icons on
    And I turn the Work Studio column icons off
    And I start counting the icons Work Studio draws
    And I open the Work Studio Character tab of "Keeper"
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew a skill icon
    And I close the Work Studio Character tab

  Scenario: a column header carries an icon and keeps its label
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "icon and label"
    And I start counting the icons Work Studio draws
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew a work type icon
    And Work Studio left the Work tab header of "Cooking" to the game
    And I close all windows but the main tabs, for Work Studio

  Scenario: with the column icons off no header carries an icon
    When I turn the Work Studio column icons off
    And I start counting the icons Work Studio draws
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew no work type icon
    And Work Studio left the Work tab header of "Cooking" to the game
    And I close all windows but the main tabs, for Work Studio

  Scenario: the column icons stay on when only the skill icons are turned off
    When I turn the Work Studio skill icons off
    And I turn the Work Studio column icons on
    And I set the Work Studio column header to "icon and label"
    And I start counting the icons Work Studio draws
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew a work type icon
    And I close all windows but the main tabs, for Work Studio

  Scenario: icon only takes the header over
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "icon only"
    And I start counting the icons Work Studio draws
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew a work type icon
    And Work Studio took over the Work tab header of "Cooking"
    And I close all windows but the main tabs, for Work Studio

  Scenario: label only draws the game's own header and no icon
    When I turn the Work Studio column icons on
    And I set the Work Studio column header to "label only"
    And I start counting the icons Work Studio draws
    And I open the "Work" tab
    And I let the Work Studio icons draw for a moment
    Then Work Studio drew no work type icon
    And Work Studio left the Work tab header of "Cooking" to the game
    And I close all windows but the main tabs, for Work Studio

  # The nearest thing to a restart one process can do: the setters write the file the way leaving the Mod
  # options window does, then the settings held in memory are thrown away and the file is read again.
  Scenario: the three choices survive being re-read from disk
    When I turn the Work Studio skill icons off
    And I turn the Work Studio column icons off
    And I set the Work Studio column header to "label only"
    And Work Studio's settings are re-read from disk, as a restart would
    Then the Work Studio skill icons are off, the column icons are off and the header shows "label only"

  # Display preferences stay out of an export: importing someone else's setup must not change the importer's screen.
  Scenario: an export leaves the icon choices out
    When I turn the Work Studio skill icons off
    And I set the Work Studio column header to "icon only"
    And I export the Work Studio setup as "icons"
    Then the exported Work Studio file "icons" does not contain "showSkillIcons"
    And the exported Work Studio file "icons" does not contain "showWorkTypeIcons"
    And the exported Work Studio file "icons" does not contain "workTabHeaderMode"
