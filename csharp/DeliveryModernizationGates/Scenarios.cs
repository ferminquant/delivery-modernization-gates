namespace DeliveryModernizationGates;

public static class Scenarios
{
    public static readonly ChangeSet ReleaseBranchPileup = new(
        Id: "release-branch-pileup",
        Title: "Monthly release branch with stale feature work",
        TargetBranch: "release/2026.05",
        BranchAgeDays: 21,
        LinesChanged: 1800,
        HasFeatureFlag: false,
        HasDatabaseMigration: true,
        HasRollbackPlan: false,
        Checks: new ChangeChecks(
            Unit: CheckState.Pass,
            Integration: CheckState.Manual,
            Contract: CheckState.Missing,
            Security: CheckState.Manual,
            Smoke: CheckState.Missing));

    public static readonly ChangeSet ShortLivedMainPullRequest = new(
        Id: "short-lived-main-pr",
        Title: "Short-lived branch merging into main",
        TargetBranch: "main",
        BranchAgeDays: 1,
        LinesChanged: 180,
        HasFeatureFlag: true,
        HasDatabaseMigration: false,
        HasRollbackPlan: true,
        Checks: new ChangeChecks(
            Unit: CheckState.Pass,
            Integration: CheckState.Pass,
            Contract: CheckState.Pass,
            Security: CheckState.Pass,
            Smoke: CheckState.Pass));

    public static readonly ChangeSet RecklessMainChange = new(
        Id: "reckless-main-change",
        Title: "Direct-to-main change without enough gates",
        TargetBranch: "main",
        BranchAgeDays: 0,
        LinesChanged: 95,
        HasFeatureFlag: false,
        HasDatabaseMigration: false,
        HasRollbackPlan: true,
        Checks: new ChangeChecks(
            Unit: CheckState.Pass,
            Integration: CheckState.Missing,
            Contract: CheckState.Missing,
            Security: CheckState.Pass,
            Smoke: CheckState.Missing));

    public static IReadOnlyList<ChangeSet> All { get; } =
    [
        ReleaseBranchPileup,
        ShortLivedMainPullRequest,
        RecklessMainChange
    ];
}
