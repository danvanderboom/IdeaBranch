## ADDED Requirements

### Requirement: Search by content type
The system SHALL allow users to search for different types of content including topic nodes, node annotations (tags and comments), tags, and prompt templates.

#### Scenario: Search topic nodes
- **WHEN** a user searches for content type "topic nodes"
- **THEN** only topic nodes matching other search criteria are returned

#### Scenario: Search annotations
- **WHEN** a user searches for content type "annotations"
- **THEN** only annotations matching other search criteria are returned

#### Scenario: Search tags
- **WHEN** a user searches for content type "tags"
- **THEN** only tags matching other search criteria are returned

#### Scenario: Search prompt templates
- **WHEN** a user searches for content type "prompt templates"
- **THEN** only prompt templates matching other search criteria are returned

### Requirement: Tag-based filtering
The system SHALL allow users to filter search results by tags, including single tags, multiple tags, and tag exclusion.

#### Scenario: Filter by single tag
- **WHEN** a user filters search results by a single tag
- **THEN** only items with that tag are returned

#### Scenario: Filter by multiple tags
- **WHEN** a user filters search results by multiple tags
- **THEN** only items with all selected tags are returned (AND logic)

#### Scenario: Exclude results with tags
- **WHEN** a user excludes results with specific tags
- **THEN** items with those tags are filtered out from results
- **AND** remaining results match other search criteria

### Requirement: Tag expressions
The system SHALL allow users to create complex tag expressions using AND, OR, and BUT-NOT-IF operators to precisely define search criteria.

#### Scenario: Simple tag expression (AND)
- **WHEN** a user creates a tag expression "tag1 AND tag2"
- **THEN** only items with both tag1 and tag2 are returned

#### Scenario: Complex tag expression (OR)
- **WHEN** a user creates a tag expression "tag1 OR tag2"
- **THEN** items with either tag1 or tag2 are returned

#### Scenario: Complex tag expression with BUT-NOT-IF
- **WHEN** a user creates a tag expression "tag1 AND (tag2 OR tag3) BUT-NOT-IF (tag4 OR tag5)"
- **THEN** items matching the positive criteria (tag1 AND (tag2 OR tag3)) are returned
- **AND** items with tag4 or tag5 are excluded

#### Scenario: Nested tag expressions
- **WHEN** a user creates nested tag expressions with parentheses
- **THEN** the expression is evaluated according to operator precedence and parentheses grouping
- **AND** items matching the expression are returned

### Requirement: Tag weight range queries
The system SHALL allow users to search for items with tags having numeric values within specified ranges.

#### Scenario: Filter by tag weight greater than value
- **WHEN** a user searches with "tag1 > 2"
- **THEN** only items with tag1 having a numeric value greater than 2 are returned

#### Scenario: Filter by tag weight less than value
- **WHEN** a user searches with "tag2 < 9"
- **THEN** only items with tag2 having a numeric value less than 9 are returned

#### Scenario: Filter by tag weight range
- **WHEN** a user searches with "tag1 > 2 AND tag2 < 9"
- **THEN** only items matching both weight criteria are returned

#### Scenario: Filter by tag weight OR condition
- **WHEN** a user searches with "tag1 > 2 OR tag2 < 9"
- **THEN** items matching either weight criterion are returned

### Requirement: Text search
The system SHALL allow users to search for exact or similar text in prompts, responses, or comments.

#### Scenario: Search exact text in prompts
- **WHEN** a user searches for exact text in prompts
- **THEN** only topic nodes with prompts containing the exact text are returned

#### Scenario: Search exact text in responses
- **WHEN** a user searches for exact text in responses
- **THEN** only topic nodes with responses containing the exact text are returned

#### Scenario: Search exact text in comments
- **WHEN** a user searches for exact text in annotation comments
- **THEN** only annotations with comments containing the exact text are returned

#### Scenario: Search similar text
- **WHEN** a user searches for similar text
- **THEN** items with text similar to the search term are returned (fuzzy matching)
- **AND** similarity results are ordered by relevance

### Requirement: Edit time range filtering
The system SHALL allow users to search for content that was edited within a specified time range.

#### Scenario: Filter by edit time range
- **WHEN** a user searches for content edited between two dates
- **THEN** only items with edit timestamps within the specified range are returned

#### Scenario: Filter by recent edits
- **WHEN** a user searches for content edited in the last N days
- **THEN** only items edited within the specified timeframe are returned

### Requirement: Historical time range filtering
The system SHALL allow users to search for content relevant to specific historical time periods (based on temporal annotations).

#### Scenario: Filter by historical time range
- **WHEN** a user searches for content relevant to a specific time period
- **THEN** only items with temporal annotations within the specified range are returned

#### Scenario: Filter by historical date range
- **WHEN** a user searches for content relevant to a date range (e.g., "1861-1865")
- **THEN** only items with temporal annotations matching the date range are returned

