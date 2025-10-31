## ADDED Requirements
### Requirement: Agent Framework Integration Testing
The system SHALL provide integration tests that evaluate LLM agent decision-making accuracy when given hierarchical data context through Microsoft Agent Framework.

#### Scenario: Live agent testing with Azure OpenAI
- **WHEN** tests are run with `CI_AF_LIVE=1` and Azure OpenAI configured
- **THEN** tests call live Azure OpenAI models via Agent Framework and measure success rates

#### Scenario: Local model testing with LM Studio
- **WHEN** tests are run with `CI_AF_PROVIDER=lmstudio` and LM Studio configured
- **THEN** tests call local OpenAI-compatible models and measure success rates

#### Scenario: Tree view comparison testing
- **WHEN** same hierarchical data is presented in different formats (expanded/collapsed, depth-limited, property-filtered)
- **THEN** tests assert that optimal views achieve target success rates (default 90%) and outperform suboptimal views

#### Scenario: Multiple choice question evaluation
- **WHEN** agents are given tree context and asked multiple choice questions
- **THEN** tests parse single-letter responses (A/B/C/D) and compute accuracy across multiple trials

### Requirement: Test Configuration and Scripts
The system SHALL provide PowerShell scripts and environment variable configuration for running integration tests across different providers and settings.

#### Scenario: Azure OpenAI test execution
- **WHEN** `Run-AgentTests-Azure.ps1` is executed with endpoint, deployment, and API key
- **THEN** tests run against Azure OpenAI with configured parameters

#### Scenario: LM Studio test execution
- **WHEN** `Run-AgentTests-LMStudio.ps1` is executed with endpoint and model ID
- **THEN** tests run against local LM Studio server with configured parameters

#### Scenario: Test parameterization
- **WHEN** environment variables are set for trials, temperature, and target success rate
- **THEN** tests use these parameters for execution and assertion thresholds
