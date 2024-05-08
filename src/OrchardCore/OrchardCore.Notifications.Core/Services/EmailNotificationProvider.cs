using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class EmailNotificationProvider : INotificationMethodProvider
{
    private readonly IEmailService _emailService;
    protected readonly IStringLocalizer S;

    public EmailNotificationProvider(
        IEmailService emailService,
        IStringLocalizer<EmailNotificationProvider> stringLocalizer)
    {
        _emailService = emailService;
        S = stringLocalizer;
    }

    public string Method { get; } = "Email";

    public LocalizedString Name => S["Email Notifications"];

    public async Task<bool> TrySendAsync(object notify, INotificationMessage message)
    {
        var user = notify as User;

        if (string.IsNullOrEmpty(user?.Email))
        {
            return false;
        }

        var mailMessage = new MailMessage()
        {
            To = user.Email,
            Subject = message.Subject,
        };

        if (message.IsHtmlPreferred && !string.IsNullOrWhiteSpace(message.HtmlBody))
        {
            mailMessage.Body = message.HtmlBody;
            mailMessage.IsHtmlBody = true;
        }
        else
        {
            mailMessage.Body = message.TextBody;
            mailMessage.IsHtmlBody = false;
        }

        var result = await _emailService.SendAsync(mailMessage);

        return result.Succeeded;
    }
}
