Feature: Booking end-to-end tests
  In order to verify system functionality
  I want to test the complete booking cycle from creation to deletion

  Background:
    Given I have a valid auth token

  @integration @regression
  Scenario: Complete booking lifecycle (Create → Update → Verify → Delete)
    When I create a new booking with the following data:
      | firstname | lastname | totalprice | depositpaid | checkin    | checkout   |
      | John      | Doe      | 250        | true        | 2025-01-01 | 2025-01-10 |
    Then the response statuscode should be 200
    And the booking should be created successfully

    When I update the booking with:
      | field     | value   |
      | firstname | Updated |
      | lastname  | User    |
    Then the response statuscode should be 200
    And the fields should be updated accordingly

    When I retrieve the booking by ID
    Then the booking details should reflect the latest updates

    When I delete booking
    Then the response statuscode should be 200
    And a subsequent GET request for that bookingId should return 404

  @integration @bulk
  Scenario: Bulk create, filter, update, and delete bookings
    When I create multiple bookings from data file "bulk_bookings.json"
    Then the response status code should be 200 for all bookings

    When I filter bookings by firstname "TestUser"
    Then at least one booking ID should be returned

    When I update all filtered bookings with:
      | field      | value   |
      | totalprice | 999     |
    Then all bookings should be updated with the new totalprice

    When I delete all filtered bookings
    Then the response status code should be 201 for all deletions
    And all bookings should no longer exist when retrieved
