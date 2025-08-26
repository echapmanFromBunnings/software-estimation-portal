using System.Collections.Generic;
using Xunit;
using software_estimator.Models;
using software_estimator.Services;

namespace SoftwareEstimator.Tests
{
    public class AssignmentTests
    {
        [Fact]
        public void AssignedResources_SerializesAndParses()
        {
            var f = new FunctionalLineItem();
            f.AssignedResources = new Dictionary<string, decimal>
            {
                ["team-a-FTE-1"] = 100m,
                ["team-b-CTR-2"] = 50m
            };

            var csv = f.AssignedResourceIds;
            Assert.Contains("team-a-FTE-1:100", csv);
            Assert.Contains("team-b-CTR-2:50", csv);

            var f2 = new FunctionalLineItem { AssignedResourceIds = csv };
            Assert.Equal(2, f2.AssignedResources.Count);
            Assert.Equal(100m, f2.AssignedResources["team-a-FTE-1"]);
            Assert.Equal(50m, f2.AssignedResources["team-b-CTR-2"]);
        }

        [Fact]
        public void CalculateFunctionalLineCost_Works()
        {
            var estimate = new Estimate { SprintLengthDays = 10 };
            estimate.ResourceRates = new System.Collections.Generic.List<ResourceRate>
            {
                new ResourceRate { SourceKey = "team-a-FTE-1", DailyRate = 1000m },
                new ResourceRate { SourceKey = "team-b-CTR-2", DailyRate = 500m }
            };

            var f = new FunctionalLineItem { Sprints = 0.3m };
            f.AssignedResources = new Dictionary<string, decimal>
            {
                ["team-a-FTE-1"] = 100m,
                ["team-b-CTR-2"] = 50m
            };

            var cost = AssignmentCostHelper.CalculateFunctionalLineCost(f, estimate);
            // team-a: 1000 * 3 * 1.0 = 3000
            // team-b: 500 * 3 * 0.5 = 750
            Assert.Equal(3750m, cost);
        }

        [Fact]
        public void Migration_MapsNumericToSourceKeys()
        {
            var estimates = new List<Estimate>();
            var e = new Estimate();
            var f = new FunctionalLineItem { Sprints = 0.1m };
            // legacy format: "1,2" meaning resource numbers 1 and 2 assigned
            f.AssignedResourceIds = "1,2";
            e.FunctionalItems = new List<FunctionalLineItem> { f };
            estimates.Add(e);

            var resourceNumberMap = new Dictionary<string, int>
            {
                ["team-a-FTE-1"] = 1,
                ["team-b-CTR-2"] = 2
            };

            AssignmentMigrationService.MigrateAssignments(estimates, resourceNumberMap);

            var first = System.Linq.Enumerable.First(e.FunctionalItems);
            Assert.Contains(":", first.AssignedResourceIds);
            var dict = first.AssignedResources;
            Assert.Equal(2, dict.Count);
            Assert.True(dict.ContainsKey("team-a-FTE-1"));
            Assert.True(dict.ContainsKey("team-b-CTR-2"));
        }
    }
}
