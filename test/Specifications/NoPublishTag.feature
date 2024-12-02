Feature: Skip publishing certain features
	Any feature that is tagged with @no_publish is not published or removed if already present.

Scenario: Skip feature that is tagged
	Given these pages exist
	  | PageId   | ParentPageId | Title          | Content |
	  | mainpage | null         | Main page      | ""      |
	  | 1        | mainpage     | 00BasicGherkin | ""      |
	And specification directory ../../../../Tests/BaseWithIgnoreTag
	When syncing to mainpage
	Then a feature page named Showing basic gherkin syntax should be created under 00BasicGherkin
	And number of pages created is 1

Scenario: Delete feature that is tagged
	Given these pages exist
	  | PageId   | ParentPageId | Title                          | Content |
	  | mainpage | null         | Main page                      | ""      |
	  | 1        | mainpage     | 00BasicGherkin                 | ""      |
	  | 2        | 1            | Showing basic gherkin syntax   | ""      |
	  | 3        | 1            | This feature should be deleted | ""      |
	And specification directory ../../../../Tests/BaseWithIgnoreTag
	When syncing to mainpage
	Then feature pageId 3 named This feature should be deleted is deleted