using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public abstract class NotifyUserTaskActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity, TEditViewModel>
    where TActivity : NotifyUserTaskActivity
    where TEditViewModel : NotifyUserTaskActivityViewModel, new()
{
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly NotificationOptions _notificationOptions;

    protected readonly IStringLocalizer S;

    protected virtual string EditShapeType => $"{nameof(NotifyUserTaskActivity)}_Fields_Edit";

    public NotifyUserTaskActivityDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer stringLocalizer)
    {
        _htmlSanitizerService = htmlSanitizerService;
        _liquidTemplateManager = liquidTemplateManager;
        _notificationOptions = notificationOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(TActivity model)
    {
        return Initialize<TEditViewModel>(EditShapeType, viewModel =>
        {
            return EditActivityAsync(model, viewModel);
        }).Location("Content");
    }

    public async override Task<IDisplayResult> UpdateAsync(TActivity model, IUpdateModel updater)
    {
        var viewModel = new TEditViewModel();
        await updater.TryUpdateModelAsync(viewModel, Prefix);
        
        if (!_liquidTemplateManager.Validate(viewModel.Subject, out var subjectErrors))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.Subject), S["Subject field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', subjectErrors)]);
        }

        if (!_liquidTemplateManager.Validate(viewModel.Summary, out var summaryErrors))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.Summary), S["Summary field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', summaryErrors)]);
        }

        if (!_liquidTemplateManager.Validate(viewModel.TextBody, out var textBodyErrors))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.TextBody), S["Text Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', textBodyErrors)]);
        }

        if (!_liquidTemplateManager.Validate(viewModel.HtmlBody, out var htmlBodyErrors))
        {
            updater.ModelState.AddModelError(Prefix, nameof(viewModel.HtmlBody), S["HTML Body field does not contain a valid Liquid expression. Details: {0}", string.Join(' ', htmlBodyErrors)]);
        }

        if (updater.ModelState.IsValid)
        {
            await UpdateActivityAsync(viewModel, model);
        }

        return Edit(model);
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override ValueTask EditActivityAsync(TActivity activity, TEditViewModel model)
    {
        EditActivity(activity, model);

        return new ValueTask();
    }

    /// <summary>
    /// Edit the view model before it's used in the editor.
    /// </summary>
    protected override void EditActivity(TActivity activity, TEditViewModel model)
    {
        model.Subject = activity.Subject.Expression;
        model.Summary = activity.Summary.Expression;
        model.TextBody = activity.TextBody.Expression;
        model.HtmlBody = activity.HtmlBody.Expression;
        model.IsHtmlPreferred = activity.IsHtmlPreferred;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override Task UpdateActivityAsync(TEditViewModel model, TActivity activity)
    {
        UpdateActivity(model, activity);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Updates the activity when the view model is validated.
    /// </summary>
    protected override void UpdateActivity(TEditViewModel model, TActivity activity)
    {
        activity.Subject = new WorkflowExpression<string>(model.Subject);
        activity.Summary = new WorkflowExpression<string>(_htmlSanitizerService.Sanitize(model.Summary));
        activity.TextBody = new WorkflowExpression<string>(model.TextBody);
        activity.HtmlBody = new WorkflowExpression<string>(_notificationOptions.DisableNotificationHtmlBodySanitizer ? model.HtmlBody : _htmlSanitizerService.Sanitize(model.HtmlBody));
        activity.IsHtmlPreferred = model.IsHtmlPreferred;
    }

    public override IDisplayResult Display(TActivity activity)
    {
        return Combine(
            Shape($"{typeof(TActivity).Name}_Fields_Thumbnail", new ActivityViewModel<TActivity>(activity))
                .Location("Thumbnail", "Content"),
            Shape($"{typeof(TActivity).Name}_Fields_Design", new ActivityViewModel<TActivity>(activity))
                .Location("Design", "Content")
        );
    }
}

public abstract class NotifyUserTaskActivityDisplayDriver<TActivity> : NotifyUserTaskActivityDisplayDriver<TActivity, NotifyUserTaskActivityViewModel>
    where TActivity : NotifyUserTaskActivity
{
    public NotifyUserTaskActivityDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, notificationOptions, stringLocalizer)
    {
    }
}
