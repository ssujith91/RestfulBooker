Feature: Update booking
  In order to modify booking information
  As a QA engineer I want to update individual or multiple fields and validate responses

  Background:
    Given I have a valid auth token

  @regression @datadriven @UpdateBooking
  Scenario: Update individual fields (data-driven)
    Given an existing booking ID
    When I update the booking with:
      | field     | value    |
      | firstname | JohnTest |
    Then the response status code should be 200
    And the field "firstname" should be updated to "JohnTest"
    And other fields should remain unchanged
        

  @regression @datadriven @UpdateBooking
  Scenario Outline: Update multiple fields in single request (data-driven)
    Given an existing booking ID
    When I update the booking with multiple fields from file "update_multiple_fields.json"
    Then the response status code should be 200
    And all specified fields should be updated accordingly
    And other fields should remain unchanged

  @regression @datadriven @UpdateBooking
  Scenario Outline: Update nested objects (bookingdates)
    Given an existing booking ID
    When I update the booking with nested bookingdates from file "update_bookingdates.json"
    Then the response status code should be 200
    And the bookingdates should be updated correctly

  @negative @datadriven @UpdateBooking
  # currently failing due to API bug returning 405 instead of 404
  Scenario: Update with invalid booking ID returns 404
    Given a non-existent booking ID
    When I update the booking with:
      | field     | value    |
      | firstname | JohnTest |
    Then the response status code should be 404

   
  @negative @datadriven @UpdateBooking
  # currently failing due to API bug returning 200 instead of 400
  Scenario: Update with invalid data types returns 400
    Given an existing booking ID
    When I update the booking with:
      | field      | value |
      | totalprice | abc   |
    Then the response status code should be 400
   

  @negative @datadriven @UpdateBooking
  # currently failing due to API bug returning 403 instead of 401
  Scenario: Update without authentication returns 401
    Given an existing booking ID
    When I update the booking without authentication using:
      | field     | value |
      | firstname | henry |
    Then the response status code should be 401
    

  @negative @datadriven @UpdateBooking
  #currently failing due to API bug returning 200 instead of 400
  Scenario: SQL injection and special characters should be handled
    Given an existing booking ID
    When I update the booking with:
      | field     | value                      |
      | firstname | '; DROP TABLE bookings; -- |
      | lastname  | !@#$%^&*()                 |
    Then the response status code should be 400

  @negative @datadriven @UpdateBooking
  Scenario Outline: Missing required fields and malformed JSON
    Given an existing booking ID
    When I send malformed or incomplete JSON from file "<file>"
    Then the response status code should be 400

    Examples:
      | file                        |
      | negative_missing_fields.json |
      | negative_malformed.json      |

  @regression @UpdateBooking
  Scenario: Verify idempotency of updates
    Given an existing booking ID
    When I update the booking with:
      | field     | value      |
      | firstname | Idempotent |
    And I update the booking again with the same payload
    Then both responses status codes should be 200
    And the final state of the booking should match the last update

