Feature: Confluence generation

Scenario: Basic scenario
	Given feature file ../../../../Tests/Parsing/BasicScenario.feature
	When Parsed
	Then feature title is Showing basic gherkin syntax
    Then result contains scenario title <h3>Using And and But</h3>
	And result contains a Code Block with these entries
	  | Line                                                   |
	  | Given the initial state of the application is Running  |
	  | And I have authorization to ask application state      |
	  | When I ask what the application state is               |
	  | Then I should see Running as the answer                |
	  | And I should see the time of the application           |
	  | But the state of the application should not be Stopped |

Scenario: Scenario outline
	Given feature file ../../../../Tests/Parsing/ScenarioOutline.feature
	When Parsed
    Then result contains a html table with
      | number 1 | number 2 | result |
      | 10       | 20       | 30     |
      | 20       | 20       | 40     |
      | 20       | 30       | 50     |
	And result contains a html table with
      | number 1 | number 2 | result |
      | 100      | 20       | 120    |
      | 1000     | 20       | 1020   |
	And result contains a html table with
	  | number 1 | number 2 | result |
	  | 10       | 20       | 30     |
	  | 20       | 20       | 40     |
	  | 20       | 30       | 50     |
	And result contains a html table with
	  | number 1 | number 2 | result |
	  | 100      | 20       | 120    |
	  | 1000     | 20       | 1020   |

Scenario: Rule
	Given feature file ../../../../Tests/Parsing/FeatureWithRule.feature
	When Parsed
	Then result contains a Info Panel with Rule: Add should calculate the sum of the entered numbers

Scenario: Scenario data tables
	Given feature file ../../../../Tests/Parsing/DataTables.feature
	When Parsed
	Then result contains a html table with
	  | Name   | Style | Birth date | Cred |
	  | Marcus | Cool  | 1972-10-09 | 50   |
	  | Anders | Butch | 1977-01-01 | 500  |
	  | Jocke  | Soft  | 1974-04-04 | 1000 |

Scenario: All in one scenario
	Given feature file ../../../../Tests/Parsing/AllInOne.feature
	When Parsed
	Then result should match the reference

Scenario: Background
	Given feature file ../../../../Tests/Parsing/Background.feature
	When Parsed
	Then feature title is Background test
	And result contains a Code Block with these entries
	  | Line                                                                 |
	  | Given a something that should happen before each scenario with table |
	  | When parsed                                                          |
	  | Then background should be rendered                                   |
	Then result contains a html table with
	  | Name        | Style | Birth date | Cred |
	  | Background  | Cool  | 1972-10-09 | 50   |
	  | Achtergrond | Butch | 1977-01-01 | 500  |