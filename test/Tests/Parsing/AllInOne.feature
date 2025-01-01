Feature: All in one
	In order to see that gherkin is a very simple language  
	As a SpecFlow evangelist  
	I want to show that basic syntax

Rule: Add should calculate the sum of the entered numbers

Background:
	Given a global administrator named "Greg"
	And a blog named "Greg's anti-tax rants"
	And a customer named "Dr. Bill"
	And a blog named "Expensive Therapy" owned by "Dr. Bill"

Scenario: Using And and But
	Given the initial state of the application is Running
		And I have authorization to ask application state
	When I ask what the application state is
	Then I should see Running as the answer
		And I should see the time of the application
		But the state of the application should not be Stopped

Scenario: Using tables
	Given I have the following persons
	  | Name   | Style | Birth date | Cred |
	  | Marcus | Cool  | 1972-10-09 | 50   |
	  | Anders | Butch | 1977-01-01 | 500  |
	  | Jocke  | Soft  | 1974-04-04 | 1000 |
	When I search for Jocke
	Then the following person should be returned

Scenario Outline: Add two negative numbers with many examples
	Given I enter <number 1> into the calculator
	And I enter <number 2> into the calculator
	When I perform add
	Then the result should be <result>

	Examples: less than 100
	  | number 1 | number 2 | result |
	  | 10       | 20       | 30     |
	  | 20       | 20       | 40     |
	  | 20       | 30       | 50     |