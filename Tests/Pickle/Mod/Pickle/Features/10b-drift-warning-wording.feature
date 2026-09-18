# TESTING.md scenario 10, the wording half. 10 asserts that the dialog opens once and not twice;
# what it cannot judge is whether the text inside it reads correctly. This dialog is the most
# text-heavy thing the mod ever shows, it is built from four separate keys and fills a {0}
# placeholder in each, so a translation defect shows here before anywhere else. Nothing is asserted.
#
# What to look for, in whichever language the game is running:
#   - no raw key: a literal "WorkStudio.Drift.Intro" or similar means the key is missing there;
#   - the {0} placeholders are filled - the vanished type "PickleVanishedWork" is named, the moved
#     task is named - and not left as a literal {0};
#   - nothing is clipped or overflowing the dialog, which French makes likelier than English;
#   - the title, the body and the button read as sentences, not as a concatenation.
@review
Feature: the drift warning's wording

  Background:
    Given the save "test-colony" is loaded
    When I create the work type "Pickle herding"
    And I move the task "Milk" into "Pickle herding"

  Scenario: the dialog a changed mod list produces
    Given the last startup saw a work type "PickleVanishedWork" that is gone now
    When I close the work type editor
    And Work Studio runs its startup check
    And I let the interface draw
    And I take a screenshot "drift warning dialog"
