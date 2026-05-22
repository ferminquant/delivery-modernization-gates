export type CheckState = "pass" | "fail" | "manual" | "missing";

export type BranchingModel = {
  name: string;
  integrationBranch: string;
  releaseBranch: string | null;
  releaseCadenceDays: number;
  typicalBranchAgeDays: number;
  automatedGateCoverage: "low" | "medium" | "high";
  rollbackStyle: "manual coordination" | "redeploy previous build" | "toggle or rollback";
};

export type ChangeSet = {
  id: string;
  title: string;
  targetBranch: string;
  branchAgeDays: number;
  linesChanged: number;
  hasFeatureFlag: boolean;
  hasDatabaseMigration: boolean;
  hasRollbackPlan: boolean;
  checks: {
    unit: CheckState;
    integration: CheckState;
    contract: CheckState;
    security: CheckState;
    smoke: CheckState;
  };
};

export type GateFinding = {
  severity: "blocker" | "risk" | "note";
  code: string;
  message: string;
};

export type DeliveryGateReport = {
  verdict: "ready-to-merge" | "needs-work" | "blocked";
  riskScore: number;
  findings: GateFinding[];
  nextAction: string;
};

export type DeliveryComparison = {
  releaseCadence: string;
  staleChangeWindow: string;
  integrationBranch: string;
  feedbackLoop: string;
  recovery: string;
};

const requiredChecks: Array<keyof ChangeSet["checks"]> = [
  "unit",
  "integration",
  "contract",
  "smoke"
];

export const trunkBasedModel: BranchingModel = {
  name: "Trunk-based delivery",
  integrationBranch: "main",
  releaseBranch: null,
  releaseCadenceDays: 1,
  typicalBranchAgeDays: 1,
  automatedGateCoverage: "high",
  rollbackStyle: "toggle or rollback"
};

export const releaseBranchModel: BranchingModel = {
  name: "Release-branch delivery",
  integrationBranch: "dev",
  releaseBranch: "release/*",
  releaseCadenceDays: 30,
  typicalBranchAgeDays: 18,
  automatedGateCoverage: "medium",
  rollbackStyle: "manual coordination"
};

export function evaluateChangeSet(change: ChangeSet): DeliveryGateReport {
  const findings: GateFinding[] = [];

  if (change.targetBranch !== trunkBasedModel.integrationBranch) {
    findings.push({
      severity: "blocker",
      code: "not_integrating_to_trunk",
      message: "The change is not integrating against main, so feedback is delayed."
    });
  }

  if (change.branchAgeDays > 2) {
    findings.push({
      severity: change.branchAgeDays > 5 ? "blocker" : "risk",
      code: "stale_branch",
      message: "Long-lived branches raise merge risk and hide integration problems."
    });
  }

  for (const check of requiredChecks) {
    if (change.checks[check] !== "pass") {
      findings.push({
        severity: "blocker",
        code: `${check}_gate_not_green`,
        message: `${check} validation must pass before the change can merge.`
      });
    }
  }

  if (change.checks.security !== "pass") {
    findings.push({
      severity: change.checks.security === "fail" ? "blocker" : "risk",
      code: "security_gate_not_green",
      message: "Security validation should be automated or explicitly reviewed before merge."
    });
  }

  if (change.hasDatabaseMigration && !change.hasRollbackPlan) {
    findings.push({
      severity: "blocker",
      code: "migration_without_rollback",
      message: "Database changes need an explicit rollback or roll-forward plan."
    });
  }

  if (change.linesChanged > 600) {
    findings.push({
      severity: "risk",
      code: "large_batch",
      message: "Large batches are harder to review, test, and recover."
    });
  }

  if (!change.hasFeatureFlag) {
    findings.push({
      severity: "note",
      code: "no_feature_flag",
      message: "A feature flag would let release timing decouple from merge timing."
    });
  }

  const blockers = findings.filter((finding) => finding.severity === "blocker").length;
  const risks = findings.filter((finding) => finding.severity === "risk").length;
  const notes = findings.filter((finding) => finding.severity === "note").length;
  const riskScore = blockers * 35 + risks * 15 + notes * 2 + Math.min(change.branchAgeDays * 3, 20);

  return {
    verdict: blockers > 0 ? "blocked" : risks > 0 ? "needs-work" : "ready-to-merge",
    riskScore,
    findings,
    nextAction:
      blockers > 0
        ? "Fix the blocked gates before merging to main."
        : risks > 0
          ? "Reduce the batch or add safeguards before merging."
          : "Merge to main and let the deployment pipeline promote the build."
  };
}

export function compareDeliveryModels(
  before: BranchingModel,
  after: BranchingModel
): DeliveryComparison {
  return {
    releaseCadence:
      `${before.releaseCadenceDays}-day model -> ${after.releaseCadenceDays}-day model`,
    staleChangeWindow:
      `${before.typicalBranchAgeDays}-day branch -> ${after.typicalBranchAgeDays}-day branch`,
    integrationBranch:
      `${before.integrationBranch}${before.releaseBranch ? ` + ${before.releaseBranch}` : ""} -> ${after.integrationBranch}`,
    feedbackLoop:
      `${before.automatedGateCoverage} automated coverage -> ${after.automatedGateCoverage} automated coverage`,
    recovery:
      `${before.rollbackStyle} -> ${after.rollbackStyle}`
  };
}
