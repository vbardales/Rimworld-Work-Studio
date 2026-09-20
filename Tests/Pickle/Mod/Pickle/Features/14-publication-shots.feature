# Images for the Workshop page, not a test: nothing here is asserted. Separate from the @review
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
# RUN THESE WITH THE GAME IN ENGLISH. The Workshop page is English, and these images carry the
# mod's own labels. The @review features are the ones to run in each language.
#
# Collect them afterwards with Art/Update-WorkshopScreenshots.ps1.
@review
Feature: images for the Workshop page

  Background:
    Given the save "test-colony" is loaded

  # The editor is what the mod IS: three columns, a type the player made, and the tasks moved into
  # it. An empty middle column would sell nothing.
  Scenario: the editor, with a type a player would have made
    When I create the work type "Hauling and tidying"
    And I move the task "HaulGeneral" into "Hauling and tidying"
    And I move the task "CleanFilth" into "Hauling and tidying"
    And I select the work type "Hauling and tidying"
    And I hide the interface around Work Studio's windows
    And I take a screenshot "Workshop page, the editor"
    And I bring the interface back around Work Studio's windows

  # The point of the previous shot, seen from the Work tab: the column exists, immediately, without
  # a restart.
  Scenario: the new column in the Work tab
    When I create the work type "Hauling and tidying"
    And I move the task "HaulGeneral" into "Hauling and tidying"
    And I close the work type editor
    And I open the "Work" tab
    And I hide the interface around Work Studio's windows
    And I take a screenshot "Workshop page, the new column"
    And I bring the interface back around Work Studio's windows
    And I close all windows but the main tabs, for Work Studio

  Scenario: the settings window
    When I create the work type "Hauling and tidying"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    And I hide the interface around Work Studio's windows
    And I take a screenshot "Workshop page, the settings"
    And I bring the interface back around Work Studio's windows
