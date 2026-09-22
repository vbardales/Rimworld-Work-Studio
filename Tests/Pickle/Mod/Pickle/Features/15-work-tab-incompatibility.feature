# This feature is selected only when Fluffy.WorkTab is staged. Work Tab captures
# Controller.allColumns during implied-def generation, before Work Studio can create a type from
# saved/player configuration. Its tab later restores that list. The missing custom column is the
# stable conflict; an unrelated click failure or log error is not used as evidence.
@requires:Fluffy.WorkTab
Feature: the declared incompatibility with Fluffy's Work Tab

  Scenario: Work Tab's startup column snapshot cannot contain a type created at runtime
    When I create the work type "Pickle conflict"
    Then Fluffy's Work Tab startup snapshot omits the Work Studio type "Pickle conflict"
