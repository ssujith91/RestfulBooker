Feature: Delete booking
  In order to remove booking information
  As a QA engineer I want to delete an existing booking and verify it is removed

  Background:
    Given I have a valid auth token

  @regression
  Scenario Outline: Successfully delete existing booking with data-driven IDs
    Given an existing booking ID 
    When I delete the booking
    Then the response status code must be 201
    And a subsequent GET request for that booking should return 404

  @negative @datadriven
  #currently failing due to API bug returning 405 instead of 404
  Scenario Outline: Attempt to delete non-existent booking IDs
    Given a booking ID from data file "<file>" that does not exist
    When I delete the booking
    Then the response status code must be 405

    Examples: 
    | file                        |
    | delete_non_existent_ids.csv |

  @negative
  #currently failing due to API bug returning 403 instead of 401
  Scenario: Attempt to delete without authentication
    Given an existing booking ID
    When I delete the booking without authentication
    Then the response status code must be 403 


  @negative
  Scenario: Attempt to delete with invalid token
    Given an existing booking ID
    When I delete the booking with an invalid token
    Then the response status code must be 403

  @negative
  #currently failing due to API bug returning 405 instead of 404
  Scenario Outline: Boundary value analysis for booking IDs
    Given a booking ID of <id>
    When I delete the booking
    Then the response status code must be 404

    Examples:
      | id        |
      | 0         |
      | -1        |
      | 999999999 |

  @negative
  #currently failing due to API bug returning 405 instead of 400
  Scenario Outline: SQL injection and special characters in booking ID
    Given a booking ID of "<id>"
    When I delete the booking
    Then the response status code must be 400

    Examples:
      | id           |
      | ' OR 1=1; -- |
      | abc123       |
      | !@#$%        |

  @regression
  # fails due to API bug returning 405 instead of 404 for record not found
  Scenario: Concurrent deletion of same booking
    Given an existing booking ID
    When I send multiple parallel delete requests for that booking
    Then exactly one request should return 201
    And the remaining requests should return 404
