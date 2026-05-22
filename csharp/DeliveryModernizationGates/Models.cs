namespace DeliveryModernizationGates;

public enum CheckState
{
    Pass,
    Fail,
    Manual,
    Missing
}

public enum FindingSeverity
{
    Blocker,
    Risk,
    Note
}

public enum DeliveryVerdict
{
    ReadyToMerge,
    NeedsWork,
    Blocked
}

public sealed record BranchingModel(
    string Name,
    string IntegrationBranch,
    string? ReleaseBranch,
    int ReleaseCadenceDays,
    int TypicalBranchAgeDays,
    string AutomatedGateCoverage,
    string RollbackStyle);

public sealed record ChangeChecks(
    CheckState Unit,
    CheckState Integration,
    CheckState Contract,
    CheckState Security,
    CheckState Smoke);

public sealed record ChangeSet(
    string Id,
    string Title,
    string TargetBranch,
    int BranchAgeDays,
    int LinesChanged,
    bool HasFeatureFlag,
    bool HasDatabaseMigration,
    bool HasRollbackPlan,
    ChangeChecks Checks);

public sealed record GateFinding(
    FindingSeverity Severity,
    string Code,
    string Message);

public sealed record DeliveryGateReport(
    DeliveryVerdict Verdict,
    int RiskScore,
    IReadOnlyList<GateFinding> Findings,
    string NextAction);

public sealed record DeliveryComparison(
    string ReleaseCadence,
    string StaleChangeWindow,
    string IntegrationBranch,
    string FeedbackLoop,
    string Recovery);
