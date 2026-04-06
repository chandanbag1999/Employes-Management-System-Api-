namespace EMS.Application.Common.Interfaces;

public interface IEmailService
{
    /// <summary>
    /// Welcome email bhejta hai naye employee ko — login credentials ke saath
    /// </summary>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string employeeName,
        string temporaryPassword,
        string loginUrl
    );

    /// <summary>
    /// Generic email bhejne ke liye (future use)
    /// </summary>
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody
    );

    /// <summary>
    /// Password reset email bhejta hai — reset link ke saath
    /// </summary>
    Task SendPasswordResetEmailAsync(
        string toEmail,
        string userName,
        string resetUrl
    );

    /// <summary>
    /// Email verification email bhejta hai — verification link ke saath
    /// </summary>
    Task SendEmailVerificationEmailAsync(
        string toEmail,
        string userName,
        string verificationUrl
    );
}
