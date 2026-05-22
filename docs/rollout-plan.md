# Rollout Plan

This is a public-safe rollout shape for moving a team toward trunk-based delivery.

## 1. Establish The Baseline

Measure:

- deployment frequency
- release lead time
- average branch age
- escaped defects
- rollback rate
- manual release steps

## 2. Protect Main

Before changing branching habits, make `main` trustworthy:

- required pull request checks
- unit and integration tests
- contract checks where APIs are involved
- smoke checks after packaging or deployment
- visible bypass policy

## 3. Shorten Branch Lifetime

Move in steps:

```txt
weeks -> days -> one or two days
```

Long-lived branches are usually a sign that work needs to be sliced smaller, hidden behind flags, or separated from risky migration work.

## 4. Decouple Merge From Release

Use:

- feature flags
- environment promotion
- dark launches
- backwards-compatible database changes
- rollback or roll-forward plans

This lets `main` stay deployable without forcing every merged change to be customer-visible immediately.

## 5. Remove Release Branches Gradually

Do not delete the release process before the team has replacement safety.

Start by making release branches boring:

- fewer changes accumulate there
- fixes flow through `main` first
- release branches become exceptional instead of normal

## 6. Review The Metrics

The goal is better delivery, not a prettier branch graph.

Useful signals:

- smaller batches
- faster recovery
- fewer stale changes
- fewer late-release surprises
- better confidence in automated gates
