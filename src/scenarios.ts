import type { ChangeSet } from "./delivery-gates";

export const exampleChangeSets: ChangeSet[] = [
  {
    id: "release-branch-pileup",
    title: "Monthly release branch with stale feature work",
    targetBranch: "release/2026.05",
    branchAgeDays: 21,
    linesChanged: 1800,
    hasFeatureFlag: false,
    hasDatabaseMigration: true,
    hasRollbackPlan: false,
    checks: {
      unit: "pass",
      integration: "manual",
      contract: "missing",
      security: "manual",
      smoke: "missing"
    }
  },
  {
    id: "short-lived-main-pr",
    title: "Short-lived branch merging into main",
    targetBranch: "main",
    branchAgeDays: 1,
    linesChanged: 180,
    hasFeatureFlag: true,
    hasDatabaseMigration: false,
    hasRollbackPlan: true,
    checks: {
      unit: "pass",
      integration: "pass",
      contract: "pass",
      security: "pass",
      smoke: "pass"
    }
  },
  {
    id: "reckless-main-change",
    title: "Direct-to-main change without enough gates",
    targetBranch: "main",
    branchAgeDays: 0,
    linesChanged: 95,
    hasFeatureFlag: false,
    hasDatabaseMigration: false,
    hasRollbackPlan: true,
    checks: {
      unit: "pass",
      integration: "missing",
      contract: "missing",
      security: "pass",
      smoke: "missing"
    }
  }
];
