# Delivery Modernization Gates

A focused CI/CD modernization example for trunk-based delivery, automated gates, and release-risk reduction.

This repository backs the Delivery Modernization case study at [ferminquant.com](https://ferminquant.com/examples/delivery/trunk-based-modernization/). It is intentionally small: the point is to show delivery-system judgment, not to hide the signal inside a large service.

The primary implementation is C#/.NET because this kind of modernization work often lands in enterprise backend teams. A TypeScript version is included as a parallel implementation of the same policy.

## What It Demonstrates

- A balanced move from release branches to trunk-based delivery.
- Short-lived feature branches into `main` instead of long-lived `dev` and `release/*` integration paths.
- Automated gates for unit, integration, contract, security, and smoke validation.
- Explicit handling for stale branches, large batches, unsafe migrations, and missing rollback plans.
- Feature flags as a way to separate merge timing from customer-visible release timing.
- Tests that prove trunk-based delivery still rejects unvalidated work.

## Commands

C#/.NET:

```bash
dotnet test csharp/DeliveryModernizationGates.Tests/DeliveryModernizationGates.Tests.csproj
dotnet run --project csharp/DeliveryModernizationGates.Scenarios/DeliveryModernizationGates.Scenarios.csproj
```

TypeScript:

```bash
npm install
npm test
npm run build
npm run scenario
```

Both scenario commands print the before/after delivery model and the gate report for each sample change set.

## How To Evaluate This Repository

Start with the C# delivery policy, then compare the TypeScript version if useful:

1. Read [`csharp/DeliveryModernizationGates/DeliveryGate.cs`](csharp/DeliveryModernizationGates/DeliveryGate.cs).
   The gate encodes the delivery policy: integrate to `main`, keep branches short-lived, require automated validation, and reject unsafe migrations.
2. Read [`csharp/DeliveryModernizationGates/Scenarios.cs`](csharp/DeliveryModernizationGates/Scenarios.cs).
   The scenarios compare a stale release branch, a healthy short-lived branch into `main`, and a reckless direct-to-main change.
3. Run `dotnet test csharp/DeliveryModernizationGates.Tests/DeliveryModernizationGates.Tests.csproj`.
   The tests show that trunk-based delivery is not a shortcut around validation.
4. Read [`.github/workflows/ci.yml`](.github/workflows/ci.yml).
   The workflow validates both the C# and TypeScript implementations on `main` and pull requests.

The TypeScript implementation lives in [`src/delivery-gates.ts`](src/delivery-gates.ts), with tests in [`test/delivery-gates.test.ts`](test/delivery-gates.test.ts). It mirrors the same delivery policy so the case study can show the model across two common backend/platform stacks.

Good signals to look for:

- delivery policy is executable, not only documented
- branch strategy is connected to feedback loops and batch size
- mainline delivery is protected by gates
- release timing can be decoupled from merge timing
- unsafe database changes are blocked before merge
- tests cover both fast delivery and reckless delivery

## Delivery Model

Before:

```txt
developer branch -> dev -> release/* -> main -> production
```

Risk tends to collect near the release window:

- stale branches
- late integration
- large merge batches
- delayed QA feedback
- manual release coordination

After:

```txt
short-lived branch -> main -> promoted build -> production
```

Risk is moved earlier:

- smaller changes
- automated validation before merge
- deployable `main`
- feature flags or promotion controls
- rollback or roll-forward plans for risky changes

## Scenario Matrix

| Scenario | Expected result | Why |
| --- | --- | --- |
| Stale release branch | `blocked` | Not integrating to `main`, stale branch, missing gates, migration without rollback |
| Short-lived main PR | `ready-to-merge` | Small branch, all required gates pass, rollback path exists |
| Reckless direct-to-main change | `blocked` | Target branch is right, but validation is missing |

## Branching Stance

This example does not claim every team should remove feature branches overnight.

The recommended default is pragmatic trunk-based delivery:

- `main` is the integration branch
- branches are short-lived
- pull requests are small and reviewed quickly
- automated gates are required
- direct commits to `main` are reserved for teams with mature pairing, test trust, and operational discipline

See [docs/branching-tradeoffs.md](docs/branching-tradeoffs.md) for the tradeoffs behind that stance.

## Production Rollout Path

For a real team, the technical gate would be paired with a migration plan:

- protect `main` and require checks before merge
- reduce branch lifetime before removing release branches
- add missing contract, smoke, and security checks
- introduce feature flags for unfinished work
- establish rollback or roll-forward expectations
- track deployment frequency, lead time, escaped defects, and rollback rate
- teach the team how the delivery model changes daily behavior

The goal is not process theater. The goal is a shorter, safer feedback loop between code, tests, release, and business value.
