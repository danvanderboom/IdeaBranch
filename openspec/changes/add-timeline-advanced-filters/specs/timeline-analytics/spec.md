## ADDED Requirements

### Requirement: Hierarchical Tag Filtering
The system SHALL provide a hierarchical tag picker that supports parent/child tags.
- Users SHALL be able to select any tag node.
- For any selected parent tag, the system SHALL provide a per-tag "Include descendants" toggle (default OFF).
- The Tags facet SHALL use OR semantics within the facet and combine with other facets using AND.

#### Scenario: Select parent with descendants OFF
- **WHEN** the user selects tag "A" with Include descendants OFF
- **THEN** results include events tagged with exactly "A" (not children)

#### Scenario: Select parent with descendants ON
- **WHEN** the user toggles Include descendants ON for tag "A"
- **THEN** results include events tagged with "A" OR any descendant tag of "A"

#### Scenario: Multiple tags selected (OR within facet)
- **WHEN** the user selects tags "A" and "B"
- **THEN** results include events tagged with "A" OR "B"

### Requirement: Event Type Filtering (Created/Updated)
The system SHALL allow filtering by event types: Created, Updated.
- Default state SHALL have both types active (no narrowing).
- The Event Type facet SHALL use OR semantics within the facet and combine with other facets using AND.

#### Scenario: Show only Created
- **WHEN** the user selects only "Created"
- **THEN** only Created-type events are shown

#### Scenario: Created OR Updated (default)
- **WHEN** both types are selected
- **THEN** no filtering is applied by event type

### Requirement: Quick Date Presets
The system SHALL provide quick presets for date range: Last 7 days, This month, This year.
- Last 7 days: rolling window [now-7d, now]
- This month: [start of current calendar month 00:00, now] in the user's timezone
- This year: [Jan 1 00:00 of current year, now] in the user's timezone
- Selecting a preset SHALL update the date range control; switching to a custom range SHALL clear the preset label.

#### Scenario: Apply Last 7 days
- **WHEN** the user selects "Last 7 days"
- **THEN** only events within the last 7×24 hours up to now are shown

#### Scenario: Apply This month preset
- **WHEN** the user selects "This month"
- **THEN** only events from the start of the current calendar month (00:00 in user timezone) to now are shown

#### Scenario: Apply This year preset
- **WHEN** the user selects "This year"
- **THEN** only events from January 1 00:00 of the current year (in user timezone) to now are shown

#### Scenario: Custom range clears preset
- **WHEN** the user manually adjusts the date range after selecting a preset
- **THEN** the preset label is cleared

### Requirement: Free-Text Search Within Timeline
The system SHALL provide a search input that filters events using case-insensitive substring matching.
- Search scope SHALL include: event title, body/message, tag names/paths, source/service name, and actor display name.
- Minimum query length SHALL be 2 characters.
- The Search facet SHALL combine with other facets using AND.

#### Scenario: Phrase match
- **WHEN** the user enters "policy update"
- **THEN** events whose searchable text contains the phrase "policy update" are shown

#### Scenario: Minimum query length
- **WHEN** the user enters a single character
- **THEN** no search filtering is applied

#### Scenario: Case-insensitive search
- **WHEN** the user enters "POLICY"
- **THEN** events matching "policy", "Policy", "POLICY", etc. are shown

### Requirement: Filter Combination Logic (Faceted Boolean)
The system SHALL apply AND across different facets and OR within a facet for multi-select enumerations.
- Facets include: Date range/preset, Source type, Tags, Event type, Search.
- Clearing a facet SHALL remove its constraint from the query.

#### Scenario: Combine tags + event type + date
- **WHEN** the user selects tags A or B; selects event type Created; selects preset Last 7 days
- **THEN** results satisfy (A OR B) AND Created AND within Last 7 days

#### Scenario: Combine search + tags
- **WHEN** the user enters search query "update" and selects tag "A"
- **THEN** results satisfy (search matches "update") AND (tag matches "A")

#### Scenario: Clear facet removes constraint
- **WHEN** the user clears all tag selections
- **THEN** the Tags facet constraint is removed from the query

