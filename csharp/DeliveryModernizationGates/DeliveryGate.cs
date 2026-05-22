namespace DeliveryModernizationGates;

public static class DeliveryGate
{
    private static readonly string[] RequiredChecks = ["unit", "integration", "contract", "smoke"];

    public static readonly BranchingModel TrunkBasedModel = new(
        Name: "Trunk-based delivery",
        IntegrationBranch: "main",
        ReleaseBranch: null,
        ReleaseCadenceDays: 1,
        TypicalBranchAgeDays: 1,
        AutomatedGateCoverage: "high",
        RollbackStyle: "toggle or rollback");

    public static readonly BranchingModel ReleaseBranchModel = new(
        Name: "Release-branch delivery",
        IntegrationBranch: "dev",
        ReleaseBranch: "release/*",
        ReleaseCadenceDays: 30,
        TypicalBranchAgeDays: 18,
        AutomatedGateCoverage: "medium",
        RollbackStyle: "manual coordination");

    public static DeliveryGateReport Evaluate(ChangeSet change)
    {
        var findings = new List<GateFinding>();

        if (!string.Equals(change.TargetBranch, TrunkBasedModel.IntegrationBranch, StringComparison.Ordinal))
        {
            findings.Add(new GateFinding(
                FindingSeverity.Blocker,
                "not_integrating_to_trunk",
                "The change is not integrating against main, so feedback is delayed."));
        }

        if (change.BranchAgeDays > 2)
        {
            findings.Add(new GateFinding(
                change.BranchAgeDays > 5 ? FindingSeverity.Blocker : FindingSeverity.Risk,
                "stale_branch",
                "Long-lived branches raise merge risk and hide integration problems."));
        }

        foreach (var check in RequiredChecks)
        {
            if (GetCheckState(change.Checks, check) != CheckState.Pass)
            {
                findings.Add(new GateFinding(
                    FindingSeverity.Blocker,
                    $"{check}_gate_not_green",
                    $"{check} validation must pass before the change can merge."));
            }
        }

        if (change.Checks.Security != CheckState.Pass)
        {
            findings.Add(new GateFinding(
                change.Checks.Security == CheckState.Fail ? FindingSeverity.Blocker : FindingSeverity.Risk,
                "security_gate_not_green",
                "Security validation should be automated or explicitly reviewed before merge."));
        }

        if (change.HasDatabaseMigration && !change.HasRollbackPlan)
        {
            findings.Add(new GateFinding(
                FindingSeverity.Blocker,
                "migration_without_rollback",
                "Database changes need an explicit rollback or roll-forward plan."));
        }

        if (change.LinesChanged > 600)
        {
            findings.Add(new GateFinding(
                FindingSeverity.Risk,
                "large_batch",
                "Large batches are harder to review, test, and recover."));
        }

        if (!change.HasFeatureFlag)
        {
            findings.Add(new GateFinding(
                FindingSeverity.Note,
                "no_feature_flag",
                "A feature flag would let release timing decouple from merge timing."));
        }

        var blockers = findings.Count(finding => finding.Severity == FindingSeverity.Blocker);
        var risks = findings.Count(finding => finding.Severity == FindingSeverity.Risk);
        var notes = findings.Count(finding => finding.Severity == FindingSeverity.Note);
        var riskScore = blockers * 35 + risks * 15 + notes * 2 + Math.Min(change.BranchAgeDays * 3, 20);

        var verdict = blockers > 0
            ? DeliveryVerdict.Blocked
            : risks > 0
                ? DeliveryVerdict.NeedsWork
                : DeliveryVerdict.ReadyToMerge;

        var nextAction = verdict switch
        {
            DeliveryVerdict.Blocked => "Fix the blocked gates before merging to main.",
            DeliveryVerdict.NeedsWork => "Reduce the batch or add safeguards before merging.",
            _ => "Merge to main and let the deployment pipeline promote the build."
        };

        return new DeliveryGateReport(verdict, riskScore, findings, nextAction);
    }

    public static DeliveryComparison Compare(BranchingModel before, BranchingModel after)
    {
        var beforeBranch = before.ReleaseBranch is null
            ? before.IntegrationBranch
            : $"{before.IntegrationBranch} + {before.ReleaseBranch}";

        return new DeliveryComparison(
            ReleaseCadence: $"{before.ReleaseCadenceDays}-day model -> {after.ReleaseCadenceDays}-day model",
            StaleChangeWindow: $"{before.TypicalBranchAgeDays}-day branch -> {after.TypicalBranchAgeDays}-day branch",
            IntegrationBranch: $"{beforeBranch} -> {after.IntegrationBranch}",
            FeedbackLoop: $"{before.AutomatedGateCoverage} automated coverage -> {after.AutomatedGateCoverage} automated coverage",
            Recovery: $"{before.RollbackStyle} -> {after.RollbackStyle}");
    }

    private static CheckState GetCheckState(ChangeChecks checks, string name) =>
        name switch
        {
            "unit" => checks.Unit,
            "integration" => checks.Integration,
            "contract" => checks.Contract,
            "smoke" => checks.Smoke,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown delivery gate check.")
        };
}
