Feature: Confluence generation

Scenario: Basic scenario
	Given feature file ../../../../Tests/Parsing/BasicScenario.feature
	When Parsed
	Then result matches snapshot

Scenario: Scenario outline
	Given feature file ../../../../Tests/Parsing/ScenarioOutline.feature
	When Parsed
	Then result matches snapshot