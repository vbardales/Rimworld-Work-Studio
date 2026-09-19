# TESTING.md scenario 13, the visual half. 13 asserts that both doors lead to the same settings and
# that the reset asks first; what no assertion covers is whether that window reads correctly.
# Nothing here is asserted.
#
# What to look for:
#   - the intro line, "Open the work type editor", "Import / export a setup" and the save note all
#     read as text, with no raw key and nothing clipped;
#   - "Reset the whole setup" is offered only when there is something to reset - the Background
#     leaves a configuration in place, so it should be there;
#   - the same window drawn through Mod options -> Work Studio looks the same, since it is the same
#     method drawing the same settings instance.
@review
Feature: what the settings window looks like

  Background:
    Given the save "test-colony" is loaded
    When I create the work type "Pickle herding"
    And I close the work type editor

  Scenario: through the MainButtons shortcut
    When I open Work Studio's settings through the MainButtons shortcut
    And I take a screenshot "settings window, opened by the MainButtons shortcut"

  Scenario: through Mod options
    When I open Work Studio's settings through Mod options
    And I take a screenshot "settings window, opened through Mod options"
