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
}
