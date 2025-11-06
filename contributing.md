# Contributing Guide

This guide explains how to set up, code, test, review, and release so contributions meet our Definition of Done.

## Getting Started

* **Unity version:** 6
* **IDE:** Visual Studio or JetBrains Rider recommended
* **Setup:**

  1. Clone the repository
  2. Open the project in Unity
  3. Configure any required environment variables or secrets (if applicable)
  4. Run the project locally to ensure everything works

## Code of Conduct

* For now, no formal code of conduct. Team members are expected to treat each other with respect.
* A formal Code of Conduct will be added as the team grows.

## Branching & Workflow

* Each feature or fix requires a **new branch**.
* Branch naming convention: `feature/your-feature-name` or `bugfix/issue-name`.
* Create a Pull Request (PR) when your work is complete.
* Once merged, delete your branch.
* Default branch is `main`.
* Rebase feature branches before merging to keep history clean.

## Issues & Planning

* Use GitHub Issues for all bugs, tasks, and feature requests.
* Use issue templates and labels (to be defined).
* Assign issues to team members and provide estimates when possible.

## Commit Messages

* Must have a **subject line** and **body**, separated by a blank line.
* Subject line should be **≤ 50 characters**.
* Body should explain major changes in detail.
* Example:

  ```
  Add player movement controller

  Implemented keyboard and gamepad support for player movement.
  Updated PlayerController script and added input manager settings.
  ```

## Code Style, Linting & Formatting

* Use **Roslyn Analyzers / StyleCop Analyzers**.
* Ensure all code adheres to the `.editorconfig` rules.
* Run linters and formatters before committing.

## Testing

* Unit and integration testing will be added in the future.
* When writing new code, consider testability and structure for future testing.

## Pull Requests & Reviews

* **No one commits directly to `main`.**
* Every PR must be reviewed and approved before merging.
* PRs should be small and focused, with clear descriptions of changes.

## CI/CD

* GitHub Actions automatically build the project when PRs are merged to `main`.
* Future enhancements may include automated tests and deployment steps.

## Security & Secrets

* Do not commit secrets or sensitive data.
* Report vulnerabilities to the team immediately.
* Dependency updates and security scanning will be implemented as the project grows.

## Documentation Expectations

* Code should be self-documenting.
* Major mechanics and systems should be documented in the README or `docs/` folder.
* Include docstrings and comments where clarity is needed.

## Release Process

* Use semantic versioning: `MAJOR.MINOR.PATCH`.
* Tag releases in GitHub.
* Maintain a changelog for significant updates.
* Packaging/publishing steps and rollback procedures will be added as the release process evolves.

## Support & Contact

* Discord is the main communication channel for questions and support.
* Team members are expected to respond within a reasonable timeframe.
* Additional support channels may be added in the future.
