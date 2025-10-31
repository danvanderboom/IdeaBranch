# Capability Taxonomy and Section Mapping

## Capability Taxonomy

- product
- platforms
- ui
- navigation
- topic-trees
- annotations
- tag-taxonomy
- prompt-templates
- ai-assist
- search
- collaboration
- analytics-visualizations
- publishing
- tasks-scheduling
- notifications
- import-export
- versioning
- integrations
- settings
- security
- performance
- accessibility
- localization
- telemetry
- error-handling
- data-storage
- sync
- maps-geospatial
- timeline-temporal
- graph-relationships

## Mapping Rules

- Map each product-doc section to a single primary capability.
- If content is cross-cutting, pick the dominant capability and reference others inline.
- Split requirements when a single statement contains multiple concerns (avoid "AND").

## Section → Capability Mapping

- Introduction → product
- Topic Trees → topic-trees
  - Hierarchical Structure → topic-trees
  - Topic Tree Manipulation → topic-trees
  - Document Import → import-export
  - Enhanced Context and Feedback → topic-trees
- Annotations: Tags and Comments → annotations
  - Action/Numeric/Geo/Temporal Tags → annotations, maps-geospatial, timeline-temporal
- Tag Taxonomy → tag-taxonomy
- Prompt Templates → prompt-templates
- Prompt Template Hierarchy → prompt-templates
- AI-Assisted Features → ai-assist
  - Topic Nodes / Title Generation / Template Recs / Editing → ai-assist
  - Auto-generate Topic Trees / Tag Taxonomies / Templates → ai-assist
  - AI Safety → security
- Advanced Search → search
- Collaborative Research → collaboration
  - Team Creation and Management → collaboration
  - Access Levels and Permissions → collaboration, security
  - Real-time Collaborative Editing → collaboration
- Analytics and Data Visualizations → analytics-visualizations
  - Word Clouds → analytics-visualizations
  - Timelines / Dynamic Updates → timeline-temporal
  - Hierarchical Tag-based Filtering → analytics-visualizations
- Publishing Options → publishing
- Task Management and Scheduling → tasks-scheduling
  - Calendar and Timeline Interfaces → tasks-scheduling, timeline-temporal
  - Notifications → notifications
- Import and Export → import-export
  - File Formats → import-export
- Version Control and History → versioning
- External Integrations → integrations
- Software Applications (App Stores) → platforms
- Application Settings → settings
- Supplementary Q&A blocks → product (usage scenarios) and respective capabilities (as inspiration)
