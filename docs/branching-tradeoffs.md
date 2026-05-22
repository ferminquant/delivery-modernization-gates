# Branching Tradeoffs

This example treats branching as a delivery-system decision, not a matter of taste.

## GitFlow-Style Release Branches

Good when:

- releases need heavy manual coordination
- automated validation is weak
- the team is not ready to keep `main` deployable
- compliance gates are still manual

Costs:

- integration feedback arrives late
- branches grow stale
- merge conflicts cluster near release windows
- fixes wait for the release train
- QA learns about combined changes when the batch is already large

## Short-Lived Branches Into Main

Good when:

- pull requests are small
- automated gates are trustworthy
- `main` is protected
- feature flags or promotion controls exist
- the team can review quickly

Costs:

- weak tests become visible immediately
- large changes need to be sliced
- teams must own broken builds quickly
- release and deployment vocabulary must be clear

## Direct Commits To Main

Good when:

- pairing or ensemble work is normal
- tests are fast and trusted
- deployment safety is strong
- the team has a strong culture of immediate repair

Costs:

- not appropriate for every team
- can look like bypassing review if the team lacks discipline
- weak gates make it reckless

## Recommended Default

Use short-lived branches into `main`.

That keeps code review and branch protection while moving integration feedback earlier. It is a practical middle ground for many enterprise teams moving away from monthly or quarterly release trains.
