Feature: Replicate folder sturcture
	Any .feature file and folder on disk is replicated to confluence
	in the form of pages and children pages to have the exact same navigational structure

Scenario: Create new folder and feature
	Given these pages exist
	  | PageId   | ParentPageId | Title     | Content |
	  | mainpage | null         | Main page | ""      |
	And specification directory ../../../../Tests/Base
	When syncing to mainpage
	Then a directory page named 00BasicGherkin should be created under mainpage
	Then a feature page named Showing basic gherkin syntax should be created under 00BasicGherkin
	
Scenario: Update existing feature
	Given these pages exist
	  | PageId   | ParentPageId | Title                        | Content |
	  | mainpage | null         | Main page                    | ""      |
	  | 1        | mainpage     | 00BasicGherkin               | ""      |
	  | 2        | 1            | Showing basic gherkin syntax | ""      |
	And specification directory ../../../../Tests/Base
	When syncing to mainpage
	Then feature pageId 2 named Showing basic gherkin syntax is updated
