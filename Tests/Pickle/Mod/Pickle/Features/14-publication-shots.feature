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
# she publishes with. It needs the pass of wsl-deps.gallery.map; every other pass skips this feature. (Moved from the
# "nelim-zen-meadow-studio" fixture on 2026-10-06, after Pickle Tools announced the final fixture.)
#
# THE GALLERY TELLS ONE STORY (PUBLISHING.md, 2026-10-06): one MORNING at the sanctuary, in the order of the scenarios below, in
# ONE place at three close hours (the rule allows it: what changes between two images is the subject and the story, not the
# place). Nelim tidies her colony's chores. EARLY MORNING (8): she opens the editor and makes a work type of her own,
# "Tidying" (1). MORNING (9): the new column is there in the Work tab, with no restart (2). LATE MORNING (10): she looks at
# the settings (3). The image names carry the number and the moment, so the order reads without opening the feature.
# One scenario is one image. Nothing is spawned or staged: the shots are the mod's own windows, "screen captures of what
# they are", and the only pawn is the fixture's, Nelim, the one colonist.
#
# THE PLACES WERE CHOSEN AMONG ALL THE NAMED ONES (SANCTUAIRE-LIEUX.md, read 2026-10-06), by what is being photographed:
#   - the place of the story is "water-garden", the pond with lilies and ducks: the Work tab (a main tab) is photographed there, at
#     its hour, with the map showing around the table. Nelim tidies the brown shore (staged decor, removed after the capture).
#   - the editor is a full-screen window (about 63% of the screen wide, 73% high): "window-backdrop-for-height", the backdrop for tall
#     frames, like the settings. PUBLISHING.md, 2026-10-06: a full-screen interface window goes on window-backdrop-for-height and is
#     cropped on the sides, the height of the frame kept whole. (It was on "window-backdrop-for-width" until that rule.)
#   - the settings window is taller than it is wide (about 47% of the screen wide, 65% high): "window-backdrop-for-height", the
#     backdrop for tall windows, validated by Virginie (2026-10-06). The editor and the settings leave the place of the story.
#   - the images of the two windows are cropped around the window afterwards (owner, 2026-10-06), with a margin of backdrop.
# No place is emptied or created; the animals are removed from the two window images only.
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
  Scenario: 1, early morning, the editor, with a type a player would have made
    Given I set the hour to 8
    When I create the work type "Tidying"
    And I move the task "HaulGeneral" into "Tidying"
    And I move the task "CleanFilth" into "Tidying"
    And I select the work type "Tidying"
    Then the task "HaulGeneral" belongs to "Tidying"
    And the task "CleanFilth" belongs to "Tidying"
    And Nelim's Pickle Tools: all animals are removed
    And Nelim's Pickle Tools: I am at the sanctuary "window-backdrop-for-height"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "workshop-1-early-morning-the-editor"
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
    # The sanctuary scene carries a tutorial box, hunger alerts and a long resource list (seen on the 2026-10-06 run). Hidden with
    # Pickle Tools' own steps, NOT with presentation mode: that one would also hide the tab bar this image keeps. The learning
    # helper is closed BEFORE the tab opens, as its author asks.
    And Nelim's Pickle Tools: the alerts are hidden
    And Nelim's Pickle Tools: the resource readout is hidden
    # THE SCENE (owner, 2026-10-06: the Work tab image needed to be more scenic): Nelim tidying the pond shore, the very work of the
    # "Tidying" type she just made. Things to haul and filth to clean lie on the brown shore west of the pond (water-garden, x 143-158,
    # z 168-176; the burrow at (149, 173) is kept out of the frame), and she stands among them. Second attempt, 2026-10-06, after the
    # owner read the first: the pawn stood idle, the filth was barely visible and the burrow drew the eye. Now she stands at the foot
    # of the logs facing them, the filth lies in a cluster around her, and the frame is shifted north so the burrow leaves it.
    # Pickle Tools had no step to make a pawn carry an item: asked of them, they wrote `"Nelim" carries the item "WoodLog"`
    # (ColonistRace, after the `stands at` step, which stops her job). The decor belongs to this image only and is removed after the capture.
    And Nelim's Pickle Tools: all animals are removed
    And Nelim's Pickle Tools: I place the decor "WoodLog" at (147, 169)
    And Nelim's Pickle Tools: I place the decor "WoodLog" at (148, 170)
    And Nelim's Pickle Tools: I place the decor "Steel" at (153, 168)
    And Nelim's Pickle Tools: I place the decor "ChunkSlagSteel" at (154, 170)
    And Nelim's Pickle Tools: I place the decor "ChunkGranite" at (146, 171)
    And Nelim's Pickle Tools: I place the decor "Filth_Trash" at (150, 169)
    And Nelim's Pickle Tools: I place the decor "Filth_Dirt" at (151, 170)
    And Nelim's Pickle Tools: I place the decor "Filth_Trash" at (152, 169)
    And Nelim's Pickle Tools: I place the decor "Filth_Trash" at (150, 171)
    And Nelim's Pickle Tools: I place the decor "Filth_Dirt" at (152, 171)
    And Nelim's Pickle Tools: I place the decor "Filth_Trash" at (151, 168)
    And Nelim's Pickle Tools: "Nelim" stands at (150, 170) facing West
    And Nelim's Pickle Tools: "Nelim" carries the item "WoodLog"
    And Nelim's Pickle Tools: I am at the sanctuary "water-garden"
    And Nelim's Pickle Tools: I frame the cell (150, 168) at zoom 4
    # Run 7ef2 showed the learning helper again ("Camera dolly", the lesson the camera move triggers): hidden here, after the frame
    # move and before the tab opens, instead of at the top of the scenario.
    And Nelim's Pickle Tools: the learning helper is hidden
    And I open the "Work" tab
    Then the Work tab has a column for "Tidying"
    # The runner starts the game with developer mode on, and its toolbar showed on the 2026-09-25 image.
    And Nelim's Pickle Tools: developer mode is turned off for the capture
    # No screenshot mode here, on purpose: the Work tab is a main tab, and the bar under it (Architect,
    # Work, Schedule...) is what tells a visitor where the panel comes from. The game does not highlight
    # the open tab. The image is the whole frame, uncropped.
    And I take a screenshot "workshop-2-morning-the-new-column"
    And Nelim's Pickle Tools: the decor is removed
    And I close all windows but the main tabs, for Work Studio

  Scenario: 3, late morning, the settings window
    Given I set the hour to 10
    When I create the work type "Tidying"
    And I close the work type editor
    And I open Work Studio's settings through Mod options
    Then window "Dialog_ModSettings" is open
    And the Mod options window is drawing Work Studio's own settings
    And Nelim's Pickle Tools: all animals are removed
    And Nelim's Pickle Tools: I am at the sanctuary "window-backdrop-for-height"
    And Nelim's Pickle Tools: screenshot mode is enabled around the open windows
    And I take a screenshot "workshop-3-late-morning-the-settings"
    And Nelim's Pickle Tools: screenshot mode is disabled
