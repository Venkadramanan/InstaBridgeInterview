using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings
{
    public class DateFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<DateField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<DateFieldSettings>("DateFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<DateFieldSettings>();

                model.Hint = settings.Hint;
                model.Required = settings.Required;
            })
                .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new DateFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
