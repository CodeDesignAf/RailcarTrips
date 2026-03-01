# RailcarTrips Assumptions

- Events are converted from local time to UTC before processing.
- Events are grouped by `EquipmentId` and sorted chronologically before applying the W/Z state machine.
- A trip is defined as `ReleasedW` to the next `PlacedZ` for the same equipment.
- Non W/Z event codes are ignored and are not treated as anomalies.
- `PlacedZ` without an open trip is treated as an anomaly.
- `ReleasedW` while a trip is already open is handled by trip-processing policy.
- `ReleasedW` without a matching `PlacedZ` results in an open trip.
- Cities must be pre-seeded and are auto-seeded on API startup in Development.
