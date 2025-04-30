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
            $$"""
            Write notification with the following details:
            1. Greet the user warmly, with a friendly and exciting tone.
            2. Provide these templates:
               - Username: [EMAIL]
               - Your Password: [PASSWORD]
            3. Add a reminder for the user to change their password after logging in for added security.
            4. Use emoticons to enhance the tone.
            5. Do not include any HTML tags, additional text, instructions, or words like 'sorry', 'remember to replace', or any notes.
            6. Return the plain text only.
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
            Write notification with the following details:
            1. Confirm the user that their password has been reset.
            2. Provide these templates:
               - Username: [EMAIL]
               - New Password: [PASSWORD]
            3. Add a reminder for the user to change their password after logging in for added security.
            4. Use emoticons to enhance the tone.
            5. Do not include any HTML tags, additional text, instructions, or words like 'sorry', 'remember to replace', or any notes.
            6. Return the plain text only.
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
            "", // No password shown
            "✅ Your Password Was Successfully Changed",
            $"""
            Write notification with the following details:
            1. Confirm that the user has successfully changed their password.
            2. Provide these templates:
               - Username: [EMAIL]
            3. Encourage them to reach out if this was not them.
            4. Use a positive and professional tone with a hint of friendliness.
            5. Use emoticons.
            6. Do not include any HTML tags or extra text.
            7. Return only plain text.
            """
        );
    }

    protected override string GetFooter() => "Thanks for keeping your account secure! 🔐";
}
