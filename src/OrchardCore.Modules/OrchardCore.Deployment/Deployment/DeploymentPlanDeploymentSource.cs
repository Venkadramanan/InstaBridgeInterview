using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Json;

namespace OrchardCore.Deployment.Deployment
{
    public class DeploymentPlanDeploymentSource : IDeploymentSource
    {
        private readonly IDeploymentPlanService _deploymentPlanService;
        private readonly IEnumerable<IDeploymentStepFactory> _deploymentStepFactories;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DeploymentPlanDeploymentSource(
            IDeploymentPlanService deploymentPlanService,
            IEnumerable<IDeploymentStepFactory> deploymentStepFactories,
            IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
        {
            _deploymentPlanService = deploymentPlanService;
            _deploymentStepFactories = deploymentStepFactories;
            _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        }

        public async Task ProcessDeploymentStepAsync(DeploymentStep deploymentStep, DeploymentPlanResult result)
        {
            if (deploymentStep is not DeploymentPlanDeploymentStep deploymentPlanStep)
            {
                return;
            }

            if (!await _deploymentPlanService.DoesUserHavePermissionsAsync())
            {
                return;
            }

            var deploymentStepFactories = _deploymentStepFactories.ToDictionary(f => f.Name);

            var deploymentPlans = deploymentPlanStep.IncludeAll
                ? (await _deploymentPlanService.GetAllDeploymentPlansAsync()).ToArray()
                : (await _deploymentPlanService.GetDeploymentPlansAsync(deploymentPlanStep.DeploymentPlanNames)).ToArray();

            var plans = (from plan in deploymentPlans
                         select new
                         {
                             plan.Name,
                             Steps = (from step in plan.DeploymentSteps
                                      select new
                                      {
                                          Type = GetStepType(deploymentStepFactories, step),
                                          Step = step
                                      }).ToArray(),
                         }).ToArray();

            // Adding deployment plans.
            result.Steps.Add(new JsonObject
            {
                ["name"] = "deployment",
                ["Plans"] = JArray.FromObject(plans, _jsonSerializerOptions),
            });
        }

        /// <summary>
        /// A Site Settings Step is generic and the name is mapped to the <see cref="IDeploymentStepFactory.Name"/> so its 'Type' should be determined though a lookup.
        /// A normal steps name is not mapped to the <see cref="IDeploymentStepFactory.Name"/> and should use its type.
        /// </summary>
        private static string GetStepType(Dictionary<string, IDeploymentStepFactory> deploymentStepFactories, DeploymentStep step)
        {
            if (deploymentStepFactories.TryGetValue(step.Name, out var deploymentStepFactory))
            {
                return deploymentStepFactory.Name;
            }

            return step.GetType().Name;
        }
    }
}
