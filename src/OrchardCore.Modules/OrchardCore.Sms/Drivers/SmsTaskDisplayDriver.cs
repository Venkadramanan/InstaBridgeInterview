using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Sms.Drivers;

public class SmsTaskDisplayDriver : ActivityDisplayDriver<SmsTask, SmsTaskViewModel>
{
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    protected readonly IStringLocalizer S;

    public SmsTaskDisplayDriver(
        IPhoneFormatValidator phoneFormatValidator,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<SmsTaskDisplayDriver> stringLocalizer
        )
    {
        _phoneFormatValidator = phoneFormatValidator;
        _liquidTemplateManager = liquidTemplateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(SmsTask activity, SmsTaskViewModel model)
    {
        model.PhoneNumber = activity.PhoneNumber.Expression;
        model.Body = activity.Body.Expression;
    }

    public async override Task<IDisplayResult> UpdateAsync(SmsTask activity, IUpdateModel updater)
    {
        var viewModel = new SmsTaskViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        if (string.IsNullOrWhiteSpace(viewModel.PhoneNumber))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Phone number requires a value."]);
        }
        else if (!_phoneFormatValidator.IsValid(viewModel.PhoneNumber))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.PhoneNumber), S["Invalid phone number used."]);
        }

        if (string.IsNullOrWhiteSpace(viewModel.Body))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), S["Message Body requires a value."]);
        }
        else if (!_liquidTemplateManager.Validate(viewModel.Body, out var bodyErrors))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.Body), string.Join(' ', bodyErrors));
        }

        activity.PhoneNumber = new WorkflowExpression<string>(viewModel.PhoneNumber);
        activity.Body = new WorkflowExpression<string>(viewModel.Body);

        return Edit(activity);
    }
}
