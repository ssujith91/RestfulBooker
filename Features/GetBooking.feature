Feature: Get booking
  In order to validate booking listing and filtering capabilities
  As a QA Engineer I want to retrieve booking ids with various filters and validate responses

  Background:
    Given I have a valid auth token
    And I create a new booking with the following data:
    | firstname | lastname | totalprice | depositpaid | checkin    | checkout   |
    | John1     | Doe      | 150        | true        | 2025-01-01 | 2025-01-05 |
    | Alice     | Smith    | 200        | false       | 2025-02-10 | 2025-02-15 |
    | Mary      | Jones    | 300        | true        | 2024-01-01 | 2025-12-31 |
    Then the booking should be created successfully

  @smoke @regression
  Scenario: Retrieve all booking IDs without filters
    When I retrieve booking ids without filters
    Then the response status code should be 200
    And the response should contain an array of objects with "bookingid"
    And the response should match the expected data types
    And the response time should be less than 2000 ms

  @regression @datadriven
  Scenario Outline: Filter by firstname only
    When I retrieve booking ids with filters:
      | firstname | <firstname> |
    Then the response status code should be 200
  Examples:
     | firstname |
     | John1     |
     | Alice     |

  @regression @datadriven
  Scenario Outline: Filter by lastname only
    When I retrieve booking ids with filters:
      | lastname | <lastname> |
    Then the response status code should be 200
    Examples:
      | lastname |
      | Doe      |
      | Smith    | 

  @regression @datadriven
  Scenario Outline: Filter by checkin date (>=)
    When I retrieve booking ids with filters:
      | checkin | <checkin> |
    Then the response status code should be 200
    And every result should have checkin date greater than or equal to "<checkin>"
    Examples:
      | checkin    |
      | 2025-01-01 |
      | 2025-02-15 |
      | 2025-03-10 |

  @regression @datadriven
  Scenario Outline: Filter by checkout date (<=)
    When I retrieve booking ids with filters:
      | checkout | <checkout> |
    Then the response status code should be 200
    And every result should have checkout date less than or equal to "<checkout>"
    Examples:
      | checkout   |
      | 2018-04-01 |
      | 2000-05-15 |
      | 2015-06-30 |

  @regression
  Scenario: Combine multiple filters
    When I retrieve booking ids with filters:
      | Key       | Value      |
      | firstname | Mary       |
      | lastname  | Jones      |
      | checkin   | 2024-01-01 |
      | checkout  | 2025-12-31 |
    Then the response status code should be 200
   
  @negative
  #The API currently returns 500 instead of 400 for invalid date formats
  Scenario: Invalid date format returns 400
    When I retrieve booking ids with filters:
    | Key     | Value      |
    | checkin | 2020-31-32 |
    Then the response status code should be 400

  @negative
  #The API currently returns 200 instead of 400 for invalid parameters
  Scenario: Invalid parameter value returns 400
    When I retrieve booking ids with filters:
    | Key          | Value  |
    | invalidparam | random |
    Then the response status code should be 400

  @negative
  #The API currently returns 200 instead of 400 and hence the test fails
  Scenario: SQL injection attempt should be safely handled
    When I retrieve booking ids with filters:
		| Key       | Value        |
		| firstname | ' OR 1=1; -- |
    Then the response status code should be 400

  @negative
  #The API currently returns 200 instead of 400 and hence the test fails
  Scenario: Special characters should be handled gracefully
    When I retrieve booking ids with filters:
		| Key       | Value      |
		| firstname | !@#$%^&*() |
    Then the response status code should be 400

  @negative
  #The API currently returns 500 instead of 400 for missing required fields in the request
  Scenario: Missing required fields in request
    When I send an empty request to booking endpoint
    Then the response status code should be 400

  @negative
  Scenario: Malformed JSON request returns 400
    When I send malformed JSON to booking endpoint
    Then the response status code should be 400

  @schema
  Scenario: Verify response structure and data types
    When I retrieve booking ids without filters
    Then the response should match the expected data types

  @performance
  Scenario: Performance validation of GET booking
    When I retrieve booking ids without filters
    Then the response time should be less than 2000 ms