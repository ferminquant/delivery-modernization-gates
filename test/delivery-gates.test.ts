import { describe, expect, it } from "vitest";
import {
  compareDeliveryModels,
  evaluateChangeSet,
  releaseBranchModel,
  trunkBasedModel
} from "../src/delivery-gates";
import { exampleChangeSets } from "../src/scenarios";

describe("delivery modernization gates", () => {
  it("blocks stale release-branch work that delays integration feedback", () => {
    const report = evaluateChangeSet(exampleChangeSets[0]);

    expect(report.verdict).toBe("blocked");
    expect(report.findings).toEqual(
      expect.arrayContaining([
        expect.objectContaining({ code: "not_integrating_to_trunk" }),
        expect.objectContaining({ code: "stale_branch" }),
        expect.objectContaining({ code: "contract_gate_not_green" }),
        expect.objectContaining({ code: "migration_without_rollback" })
      ])
    );
  });

  it("allows a short-lived mainline change when automated gates are green", () => {
    const report = evaluateChangeSet(exampleChangeSets[1]);

    expect(report).toMatchObject({
      verdict: "ready-to-merge",
      riskScore: 3,
      nextAction: "Merge to main and let the deployment pipeline promote the build."
    });
    expect(report.findings).toHaveLength(0);
  });

  it("blocks a mainline change when trunk-based delivery is missing validation", () => {
    const report = evaluateChangeSet(exampleChangeSets[2]);

    expect(report.verdict).toBe("blocked");
    expect(report.findings.map((finding) => finding.code)).toEqual(
      expect.arrayContaining([
        "integration_gate_not_green",
        "contract_gate_not_green",
        "smoke_gate_not_green"
      ])
    );
  });

  it("flags manual security validation as risk instead of silently accepting it", () => {
    const report = evaluateChangeSet({
      ...exampleChangeSets[1],
      checks: {
        ...exampleChangeSets[1].checks,
        security: "manual"
      }
    });

    expect(report.verdict).toBe("needs-work");
    expect(report.findings).toContainEqual(
      expect.objectContaining({
        severity: "risk",
        code: "security_gate_not_green"
      })
    );
  });

  it("summarizes the before and after delivery model in evaluator-friendly terms", () => {
    expect(compareDeliveryModels(releaseBranchModel, trunkBasedModel)).toEqual({
      releaseCadence: "30-day model -> 1-day model",
      staleChangeWindow: "18-day branch -> 1-day branch",
      integrationBranch: "dev + release/* -> main",
      feedbackLoop: "medium automated coverage -> high automated coverage",
      recovery: "manual coordination -> toggle or rollback"
    });
  });
});
