using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers
{
    public class ContentMenuItemPartDisplayDriver : ContentPartDisplayDriver<ContentMenuItemPart>
    {
        public override IDisplayResult Display(ContentMenuItemPart part, BuildPartDisplayContext context)
        {
            return Combine(
                Dynamic("ContentMenuItemPart_Admin", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Admin", "Content:10"),
                Dynamic("ContentMenuItemPart_Thumbnail", shape =>
                {
                    shape.MenuItemPart = part;
                })
                .Location("Thumbnail", "Content:10")
            );
        }

        public override IDisplayResult Edit(ContentMenuItemPart part)
        {
            return Initialize<ContentMenuItemPartEditViewModel>("ContentMenuItemPart_Edit", model =>
            {
                model.Name = part.ContentItem.DisplayText;
                model.MenuItemPart = part;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentMenuItemPart part, IUpdateModel updater)
        {
            var model = new ContentMenuItemPartEditViewModel();

            await updater.TryUpdateModelAsync(model, Prefix);

            part.ContentItem.DisplayText = model.Name;

            return Edit(part);
        }
    }
}
