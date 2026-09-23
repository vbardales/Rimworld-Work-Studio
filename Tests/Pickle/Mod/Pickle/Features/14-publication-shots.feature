# Images for the Workshop page. The state being photographed is asserted before every capture;
# composition and readability remain a visual review. Separate from the other @review
# features on purpose, because the two want opposite things — a @review shot should show the state
# a scenario built, warts and all, while a page image has to be presentable.
#
# Two rules learned from Architect Studio's own publication shots, 2026-09-20:
#
#   1. The scene is built here, never borrowed. A screenshot of whatever the test fixture happens to
#      hold puts a raw defName or a test name like "Pickle herding" on a store page, which reads as
#      debug output. Everything below is named the way a player would name it.
#   2. The interface around the windows is hidden, through the game's own screenshot mode. Until
#      2026-09-20 every capture carried Pickle's runner panel in the corner and had to be cropped by
#      hand; it does not any more.
#
# THE SCENE IS THE OWNER'S SHOWCASE COLONY, not the test fixture: a Workshop image with the fixture's
# plain desert behind it is not the one she publishes with. It is the fixture
# "nelim-zen-meadow-studio" of PickleTools/ScreenshotStudio, which only exists in a pass that stages
# that companion (wsl-deps.studio.map); every other pass skips this feature. (Chosen 2026-09-23.)
#
# RUN THESE WITH THE GAME IN ENGLISH. The Workshop page is English, and these images carry the
# mod's own labels. The @review features are the ones to run in each language.
#
# Collect them afterwards with Art/Update-WorkshopScreenshots.ps1.
@review @requires:nelim.pickletools.screenshotmode @requires:nelim.pickletools.screenshotstudio
Feature: images for the Workshop page

  Background:
    Given the save "nelim-zen-meadow-studio" is loaded
    And game speed is paused

  # The editor is what the mod IS: three columns, a type the player made, and the tasks moved into
  # it. An empty middle column would sell nothing.
  Scenario: the editor, with a type a player would have made
    When I create the work type "Hauling and tidying"
    And I move the task "HaulGeneral" into "Hauling and tidying"
    And I move the task "CleanFilth" into "Hauling and tidying"
    And I select the work type "Hauling and tidying"
    Then the task "HaulGeneral" belongs to "Hauling and tidying"
    And the task "CleanFilth" belongs to "Hauling and tidying"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "Workshop page, the editor"
    And Nelim's Pickle Tools: screenshot mode is disabled

  # The point of the previous shot, seen from the Work tab: the column exists, immediately, without
  # a restart.
  Scenario: the new column in the Work tab
    When I create the work type "Hauling and tidying"
    And I move the task "HaulGeneral" into "Hauling and tidying"
    And I close the work type editor
    And I open the "Work" tab
    Then the Work tab has a column for "Hauling and tidying"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "Workshop page, the new column"
    And Nelim's Pickle Tools: screenshot mode is disabled
    And I close all windows but the main tabs, for Work Studio

  Scenario: the settings window
    When I create the work type "Hauling and tidying"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    Then window "Dialog_ModSettings" is open
    And the Mod options window is drawing Work Studio's own settings
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "Workshop page, the settings"
    And Nelim's Pickle Tools: screenshot mode is disabled
