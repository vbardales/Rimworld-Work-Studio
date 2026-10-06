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
#   2. An option window of the mod (the editor, the settings) is taken with the interface around it
#      hidden, through the game's own screenshot mode, and cropped tight; a game window the mod
#      changes (the Work tab) is taken with the full interface, uncropped. The rules and the reasons
#      are in PUBLICATION.md, "Workshop screenshots". Until
#      2026-09-20 every capture carried Pickle's runner panel in the corner and had to be cropped by
#      hand; it does not any more.
#
# THE SCENE IS THE SHARED FIXTURE OF EVERY MOD'S GALLERY, Nelim's sanctuary ("Nelims-tribe", PickleTools docs/GALERIE.md and
# docs/SANCTUAIRE-LIEUX.md), not the test fixture: a Workshop image with the fixture's plain desert behind it is not the one
# she publishes with. It needs the pass of wsl-deps.sanctuary.map; every other pass skips this feature. (Moved from the
# "nelim-zen-meadow-studio" fixture on 2026-10-06, after Pickle Tools announced the final fixture.)
#
# THE GALLERY TELLS ONE STORY (PUBLISHING.md, 2026-10-06): a day at the sanctuary, in the order of the scenarios below.
# Nelim tidies her colony's chores. At DAWN she opens the editor and makes a work type of her own, "Tidying" (1); in the
# MORNING the new column is there in the Work tab, with no restart (2); in the EVENING she looks at the settings (3).
# One scenario is one image. Nothing is spawned or staged for them: the shots are the mod's own windows, "screen captures of
# what they are", and the only pawn is the fixture's, Nelim, the one colonist. The hour changes from one image to the next.
#
# THE PLACES WERE CHOSEN AMONG ALL THE NAMED ONES (SANCTUAIRE-LIEUX.md, read 2026-10-06), by what is being photographed:
#   - the editor is a full-screen window: "exhibition-zone" (alias grand-place), the plain orange carpet, is the place the
#     rule gives to those (PUBLISHING.md), with no building to compete with the window.
#   - the Work tab (a main tab) and the settings window stay in the place of the story, at its hour: "water-garden", the pond
#     with lilies and ducks, which sells the game behind the table.
# No place is emptied or created.
#
# RUN THESE WITH THE GAME IN ENGLISH. The Workshop page is English, and these images carry the
# mod's own labels. The @review features are the ones to run in each language.
#
# Copy the chosen captures afterwards, by hand, from the run's evidence folder into Art/Gallery/ as 1-, 2-, 3-... (PUBLISHING.md).
@review @requires:nelim.pickletools.screenshotmode @requires:nelim.pickletools.screenshotstudio
Feature: images for the Workshop page

  Background:
    Given the save "Nelims-tribe" is loaded
    And game speed is paused
    And I close all dialogs
    And I set the weather to "Clear"

  # The editor is what the mod IS: three columns, a type the player made, and the tasks moved into
  # it. An empty middle column would sell nothing.
  Scenario: 1, dawn, the editor, with a type a player would have made
    Given I set the hour to 6
    When I create the work type "Tidying"
    And I move the task "HaulGeneral" into "Tidying"
    And I move the task "CleanFilth" into "Tidying"
    And I select the work type "Tidying"
    Then the task "HaulGeneral" belongs to "Tidying"
    And the task "CleanFilth" belongs to "Tidying"
    And Nelim's Pickle Tools: I am at the sanctuary "exhibition-zone"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "workshop-1-dawn-the-editor"
    And Nelim's Pickle Tools: screenshot mode is disabled

  # The point of the previous shot, seen from the Work tab: the column exists, immediately, without
  # a restart. The showcase colony is heavy for the software-rendered Linux game: opening the tab
  # took 10 s on 2026-09-24 and died on the default five-second step budget, hence the tag.
  @timeout:30
  Scenario: 2, morning, the new column in the Work tab
    Given I set the hour to 9
    When I create the work type "Tidying"
    And I move the task "HaulGeneral" into "Tidying"
    And I close the work type editor
    And I open the "Work" tab
    Then the Work tab has a column for "Tidying"
    And Nelim's Pickle Tools: I am at the sanctuary "water-garden"
    # The runner starts the game with developer mode on, and its toolbar showed on the 2026-09-25 image.
    And Nelim's Pickle Tools: developer mode is turned off for the capture
    # No screenshot mode here, on purpose: the Work tab is a main tab, and the bar under it (Architect,
    # Work, Schedule...) is what tells a visitor where the panel comes from. The game does not highlight
    # the open tab. The image is the whole frame, uncropped.
    And I take a screenshot "workshop-2-morning-the-new-column"
    And I close all windows but the main tabs, for Work Studio

  Scenario: 3, evening, the settings window
    Given I set the hour to 18
    When I create the work type "Tidying"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    Then window "Dialog_ModSettings" is open
    And the Mod options window is drawing Work Studio's own settings
    And Nelim's Pickle Tools: I am at the sanctuary "water-garden"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "workshop-3-evening-the-settings"
    And Nelim's Pickle Tools: screenshot mode is disabled
