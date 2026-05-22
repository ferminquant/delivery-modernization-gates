using DeliveryModernizationGates;
using Xunit;

namespace DeliveryModernizationGates.Tests;

public sealed class DeliveryGateTests
{
    [Fact]
    public void BlocksStaleReleaseBranchWorkThatDelaysIntegrationFeedback()
    {
        var report = DeliveryGate.Evaluate(Scenarios.ReleaseBranchPileup);

        Assert.Equal(DeliveryVerdict.Blocked, report.Verdict);
        Assert.Contains(report.Findings, finding => finding.Code == "not_integrating_to_trunk");
        Assert.Contains(report.Findings, finding => finding.Code == "stale_branch");
        Assert.Contains(report.Findings, finding => finding.Code == "contract_gate_not_green");
        Assert.Contains(report.Findings, finding => finding.Code == "migration_without_rollback");
    }

    [Fact]
    public void AllowsShortLivedMainlineChangeWhenAutomatedGatesAreGreen()
    {
        var report = DeliveryGate.Evaluate(Scenarios.ShortLivedMainPullRequest);

        Assert.Equal(DeliveryVerdict.ReadyToMerge, report.Verdict);
        Assert.Equal(3, report.RiskScore);
        Assert.Empty(report.Findings);
        Assert.Equal("Merge to main and let the deployment pipeline promote the build.", report.NextAction);
    }

    [Fact]
    public void BlocksMainlineChangeWhenTrunkBasedDeliveryIsMissingValidation()
    {
        var report = DeliveryGate.Evaluate(Scenarios.RecklessMainChange);
        var codes = report.Findings.Select(finding => finding.Code).ToHashSet();

        Assert.Equal(DeliveryVerdict.Blocked, report.Verdict);
        Assert.Contains("integration_gate_not_green", codes);
        Assert.Contains("contract_gate_not_green", codes);
        Assert.Contains("smoke_gate_not_green", codes);
    }

    [Fact]
    public void FlagsManualSecurityValidationAsRiskInsteadOfSilentlyAcceptingIt()
    {
        var change = Scenarios.ShortLivedMainPullRequest with
        {
            Checks = Scenarios.ShortLivedMainPullRequest.Checks with
            {
                Security = CheckState.Manual
            }
        };

        var report = DeliveryGate.Evaluate(change);

        Assert.Equal(DeliveryVerdict.NeedsWork, report.Verdict);
        Assert.Contains(
            report.Findings,
            finding => finding is { Severity: FindingSeverity.Risk, Code: "security_gate_not_green" });
    }

    [Fact]
    public void SummarizesBeforeAndAfterDeliveryModel()
    {
        var comparison = DeliveryGate.Compare(DeliveryGate.ReleaseBranchModel, DeliveryGate.TrunkBasedModel);

        Assert.Equal("30-day model -> 1-day model", comparison.ReleaseCadence);
        Assert.Equal("18-day branch -> 1-day branch", comparison.StaleChangeWindow);
        Assert.Equal("dev + release/* -> main", comparison.IntegrationBranch);
        Assert.Equal("medium automated coverage -> high automated coverage", comparison.FeedbackLoop);
        Assert.Equal("manual coordination -> toggle or rollback", comparison.Recovery);
    }
}
