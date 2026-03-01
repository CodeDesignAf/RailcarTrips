# RailcarTrips Assumptions

Placeholder for assumptions, constraints, and initial scope notes.

Events are converted from local time to UTC using City.TimeZoneId, then grouped by EquipmentId and sorted by EventUtcTime before applying the W/Z state machine.
Trips are defined as the interval between a ReleasedW event and the next PlacedZ event for the same equipment.
