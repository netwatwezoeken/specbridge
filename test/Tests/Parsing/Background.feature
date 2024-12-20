Feature: Background test

Background:
	Given a something that should happen before each scenario with table
	  | Name        | Style | Birth date | Cred |
	  | Background  | Cool  | 1972-10-09 | 50   |
	  | Achtergrond | Butch | 1977-01-01 | 500  |
	When parsed
	Then background should be rendered