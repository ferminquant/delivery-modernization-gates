using System.Text.Json;
using DeliveryModernizationGates;

var comparison = DeliveryGate.Compare(DeliveryGate.ReleaseBranchModel, DeliveryGate.TrunkBasedModel);

Console.WriteLine("Delivery model comparison");
Console.WriteLine(JsonSerializer.Serialize(comparison, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine();
Console.WriteLine("Scenario gate results");
foreach (var changeSet in Scenarios.All)
{
    var report = DeliveryGate.Evaluate(changeSet);
    Console.WriteLine();
    Console.WriteLine(changeSet.Title);
    Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
}
