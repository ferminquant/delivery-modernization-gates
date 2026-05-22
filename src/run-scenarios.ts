import {
  compareDeliveryModels,
  evaluateChangeSet,
  releaseBranchModel,
  trunkBasedModel
} from "./delivery-gates";
import { exampleChangeSets } from "./scenarios";

const comparison = compareDeliveryModels(releaseBranchModel, trunkBasedModel);

console.log("Delivery model comparison");
console.table(comparison);

console.log("\nScenario gate results");
for (const changeSet of exampleChangeSets) {
  const report = evaluateChangeSet(changeSet);
  console.log(`\n${changeSet.title}`);
  console.log(JSON.stringify(report, null, 2));
}
