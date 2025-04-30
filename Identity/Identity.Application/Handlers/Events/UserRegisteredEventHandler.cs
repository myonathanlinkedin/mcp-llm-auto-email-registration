using MCPClient.MCPClientServices;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

public abstract class EmailNotificationHandlerBase<TEvent> : IEventHandler<TEvent>
        where TEvent : IDomainEvent
{
    private readonly IEmailSender emailSenderService;
    private readonly IMCPServerRequester mcpServerRequester;
    private readonly ILogger logger;

    protected EmailNotificationHandlerBase(
        IEmailSender emailSenderService,
        IMCPServerRequester mcpServerRequester,
        ILogger logger)
    {
        this.emailSenderService = emailSenderService;
        this.mcpServerRequester = mcpServerRequester;
        this.logger = logger;
    }

    public async Task Handle(TEvent domainEvent)
    {
        var (email, password, subject, prompt) = GetEmailData(domainEvent);

        logger.LogInformation("Requesting email body generation for: {Email}", email);
        var result = await mcpServerRequester.RequestAsync(prompt, ChatRole.System, false);

        if (!result.Succeeded)
        {
            logger.LogError("Failed to generate email body for: {Email}. Reason: {Errors}", email, result.Errors);
            return;
        }

        var body = result.Data.Replace("[EMAIL]", email);
        if (!string.IsNullOrEmpty(password))
            body = body.Replace("[PASSWORD]", password).Replace("[NEW_PASSWORD]", password);

        var fullHtml = $"""
                        <html>
                            <body>
                                <p>{body}</p>
                                <footer><p>{GetFooter()}</p></footer>
                            </body>
                        </html>
                        """;

        logger.LogInformation("Sending email to {Email}", email);
        await emailSenderService.SendEmailAsync(email, subject, fullHtml);
        logger.LogInformation("Email successfully sent to {Email}", email);
    }

    protected abstract (string Email, string Password, string Subject, string Prompt) GetEmailData(TEvent domainEvent);
    protected abstract string GetFooter();
}

public class UserRegisteredEventHandler : EmailNotificationHandlerBase<UserRegisteredEvent>
{
    public UserRegisteredEventHandler(
        IEmailSender emailSenderService,
        IMCPServerRequester mcpServerRequester,
        ILogger<UserRegisteredEventHandler> logger)
        : base(emailSenderService, mcpServerRequester, logger) { }

    protected override (string, string, string, string) GetEmailData(UserRegisteredEvent e)
    {
        return (
            e.Email,
            e.Password,
            "🎉 Hooray! Your Account is Ready 🎉",
            $"""
            Write a plain text email. Follow these strict rules:

            1. Greet the user with positivity.
            2. Confirm that the account has been successfully created.
            3. Include the following information:
               - Username: [EMAIL]
               - Your Password: [PASSWORD]
            4. Avoid words like "sorry", "issue", or anything implying a problem.
            5. Do **not** offer advice or instructions.
            6. Do **not** include HTML tags or formatting.
            7. Use emojis to maintain a friendly tone.
            8. Return only the plain text message (no additional formatting or details).
            9. Only return the plain text message. No extra content.
            """
        );
    }

    protected override string GetFooter() => "Welcome aboard! 😊";
}

public class PasswordResetEventHandler : EmailNotificationHandlerBase<PasswordResetEvent>
{
    public PasswordResetEventHandler(
        IEmailSender emailSenderService,
        IMCPServerRequester mcpServerRequester,
        ILogger<PasswordResetEventHandler> logger)
        : base(emailSenderService, mcpServerRequester, logger) { }

    protected override (string, string, string, string) GetEmailData(PasswordResetEvent e)
    {
        return (
            e.Email,
            e.NewPassword,
            "🔒 Your Password Has Been Reset",
            $"""
            Write a plain text email. Follow these strict rules:

            1. Confirm that the password has been successfully reset.
            2. Include the following information:
               - Username: [EMAIL]
               - New Password: [PASSWORD]
            3. Avoid words like "sorry", "issue", or anything implying a problem.
            4. Do **not** offer advice or instructions.
            5. Do **not** include HTML tags or formatting.
            6. Use emojis to maintain a friendly tone.
            7. Return only the plain text message (no additional formatting or details).
            8. Only return the plain text message. No extra content.
            """
        );
    }

    protected override string GetFooter() => "Stay secure and take care! 😊";
}

public class ChangePasswordEventHandler : EmailNotificationHandlerBase<PasswordChangedEvent>
{
    public ChangePasswordEventHandler(
        IEmailSender emailSenderService,
        IMCPServerRequester mcpServerRequester,
        ILogger<ChangePasswordEventHandler> logger)
        : base(emailSenderService, mcpServerRequester, logger) { }

    protected override (string, string, string, string) GetEmailData(PasswordChangedEvent e)
    {
        return (
            e.Email,
            e.NewPassword,
            "✅ Your Password Was Successfully Changed",
            $"""
            Write a plain text email. Follow these strict rules:

            1. Greet the user with positivity.
            2. Confirm that their password has been successfully changed.
            3. Include the following information:
               - Username: [EMAIL]
               - New Password: [PASSWORD]
            4. If this action was not initiated by them, advise them to reach out.
            5. Avoid words like "sorry", "issue", or anything implying a problem.
            6. Do **not** offer advice or instructions.
            7. Do **not** include HTML tags or formatting.
            8. Use emojis to maintain a friendly tone.
            9. Only return the plain text message. No extra content.
            """
        );
    }

    protected override string GetFooter() => "Thanks for keeping your account secure! 🔐";
}
