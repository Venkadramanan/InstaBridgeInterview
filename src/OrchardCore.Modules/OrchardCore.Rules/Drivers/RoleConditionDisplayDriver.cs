using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;
using OrchardCore.Rules.ViewModels;

namespace OrchardCore.Rules.Drivers
{
    public class RoleConditionDisplayDriver : DisplayDriver<Condition, RoleCondition>
    {
        private readonly ConditionOperatorOptions _options;

        public RoleConditionDisplayDriver(IOptions<ConditionOperatorOptions> options)
        {
            _options = options.Value;
        }

        public override IDisplayResult Display(RoleCondition condition)
        {
            return
                Combine(
                    View("RoleCondition_Fields_Summary", condition).Location("Summary", "Content"),
                    View("RoleCondition_Fields_Thumbnail", condition).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(RoleCondition condition)
        {
            return Initialize<RoleConditionViewModel>("RoleCondition_Fields_Edit", m =>
            {
                if (condition.Operation != null && _options.ConditionOperatorOptionByType.TryGetValue(condition.Operation.GetType(), out var option))
                {
                    m.SelectedOperation = option.Factory.Name;
                }
                m.Value = condition.Value;
                m.Condition = condition;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(RoleCondition condition, IUpdateModel updater)
        {
            var model = new RoleConditionViewModel();
            await updater.TryUpdateModelAsync(model, Prefix);
            condition.Value = model.Value;

            if (!string.IsNullOrEmpty(model.SelectedOperation) && _options.Factories.TryGetValue(model.SelectedOperation, out var factory))
            {
                condition.Operation = factory.Create();
            }

            return Edit(condition);
        }
    }
}
