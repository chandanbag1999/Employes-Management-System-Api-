using EMS.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    // Config keys
    private string Host => _config["EmailSettings:Host"] ?? "smtp.gmail.com";
    private int Port => int.Parse(_config["EmailSettings:Port"] ?? "587");
    private string SenderEmail => _config["EmailSettings:SenderEmail"] ?? "";
    private string SenderName => _config["EmailSettings:SenderName"] ?? "EMS System";
    private string AppPassword => _config["EmailSettings:AppPassword"] ?? "";

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    // ── Welcome Email ─────────────────────────────────────────────────────────

    public async Task SendWelcomeEmailAsync(
        string toEmail,
        string employeeName,
        string temporaryPassword,
        string loginUrl)
    {
        var subject = "🎉 Welcome to EMS — Your Account is Ready!";
        var htmlBody = BuildWelcomeEmailTemplate(
            employeeName, toEmail, temporaryPassword, loginUrl);

        await SendEmailAsync(toEmail, subject, htmlBody);
    }

    // ── Generic Send ──────────────────────────────────────────────────────────

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var message = new MimeMessage();

            // From
            message.From.Add(new MailboxAddress(SenderName, SenderEmail));

            // To
            message.To.Add(new MailboxAddress(string.Empty, toEmail));

            // Subject
            message.Subject = subject;

            // Body — HTML
            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            // Send via Gmail SMTP
            using var client = new SmtpClient();

            await client.ConnectAsync(Host, Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(SenderEmail, AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation(
                "[EmailService] Email sent successfully to {Email} | Subject: {Subject}",
                toEmail, subject);
        }
        catch (Exception ex)
        {
            // Email fail hone pe app crash nahi hona chahiye
            // Log karo aur continue karo
            _logger.LogError(ex,
                "[EmailService] Failed to send email to {Email} | Subject: {Subject}",
                toEmail, subject);

            // Re-throw karo taaki caller decide kare
            throw;
        }
    }

    // ── Email Template ────────────────────────────────────────────────────────

    private static string BuildWelcomeEmailTemplate(
        string employeeName,
        string email,
        string temporaryPassword,
        string loginUrl)
    {
        return $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
            <title>Welcome to EMS</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f1f5f9;font-family:'Segoe UI',Arial,sans-serif;">

          <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f1f5f9;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0"
                  style="background:#ffffff;border-radius:16px;overflow:hidden;
                         box-shadow:0 4px 24px rgba(0,0,0,0.08);max-width:600px;width:100%;">

                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#6366f1 0%,#8b5cf6 100%);
                               padding:40px 48px;text-align:center;">
                      <h1 style="margin:0;color:#ffffff;font-size:28px;font-weight:700;
                                 letter-spacing:-0.5px;">
                        Employee Management System
                      </h1>
                      <p style="margin:8px 0 0;color:rgba(255,255,255,0.85);font-size:14px;">
                        Your workplace, simplified.
                      </p>
                    </td>
                  </tr>

                  <!-- Greeting -->
                  <tr>
                    <td style="padding:40px 48px 0;">
                      <h2 style="margin:0 0 12px;color:#1e293b;font-size:22px;font-weight:600;">
                        Welcome aboard, {employeeName}! 👋
                      </h2>
                      <p style="margin:0;color:#64748b;font-size:15px;line-height:1.6;">
                        Your employee account has been created. Use the credentials below
                        to log in for the first time. You'll be asked to set a new password
                        immediately after logging in.
                      </p>
                    </td>
                  </tr>

                  <!-- Credentials Box -->
                  <tr>
                    <td style="padding:32px 48px;">
                      <table width="100%" cellpadding="0" cellspacing="0"
                        style="background:#f8fafc;border:1px solid #e2e8f0;
                               border-radius:12px;overflow:hidden;">
                        <tr>
                          <td style="padding:24px 28px;">
                            <p style="margin:0 0 6px;color:#94a3b8;font-size:11px;
                                      font-weight:600;text-transform:uppercase;
                                      letter-spacing:0.8px;">
                              Login Email
                            </p>
                            <p style="margin:0 0 20px;color:#1e293b;font-size:16px;
                                      font-weight:600;">
                              {email}
                            </p>

                            <p style="margin:0 0 6px;color:#94a3b8;font-size:11px;
                                      font-weight:600;text-transform:uppercase;
                                      letter-spacing:0.8px;">
                              Temporary Password
                            </p>
                            <p style="margin:0;color:#6366f1;font-size:20px;
                                      font-weight:700;letter-spacing:2px;
                                      font-family:monospace;">
                              {temporaryPassword}
                            </p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- Warning -->
                  <tr>
                    <td style="padding:0 48px 28px;">
                      <table width="100%" cellpadding="0" cellspacing="0"
                        style="background:#fef3c7;border-left:4px solid #f59e0b;
                               border-radius:0 8px 8px 0;">
                        <tr>
                          <td style="padding:14px 18px;">
                            <p style="margin:0;color:#92400e;font-size:13px;
                                      font-weight:500;line-height:1.5;">
                              ⚠️ <strong>Security Notice:</strong> This is a temporary
                              password. You must change it immediately after your first
                              login. Do not share this email with anyone.
                            </p>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- CTA Button -->
                  <tr>
                    <td style="padding:0 48px 40px;text-align:center;">
                      <a href="{loginUrl}"
                        style="display:inline-block;background:linear-gradient(135deg,#6366f1,#8b5cf6);
                               color:#ffffff;text-decoration:none;padding:16px 48px;
                               border-radius:50px;font-size:15px;font-weight:600;
                               letter-spacing:0.3px;box-shadow:0 4px 14px rgba(99,102,241,0.4);">
                        Login to EMS →
                      </a>
                    </td>
                  </tr>

                  <!-- Steps -->
                  <tr>
                    <td style="padding:0 48px 40px;">
                      <p style="margin:0 0 16px;color:#1e293b;font-size:14px;
                                font-weight:600;">
                        Getting Started — 3 Simple Steps:
                      </p>
                      <table width="100%" cellpadding="0" cellspacing="0">
                        <tr>
                          <td style="padding:8px 0;color:#64748b;font-size:13px;">
                            <span style="display:inline-block;width:24px;height:24px;
                                         background:#ede9fe;color:#6366f1;border-radius:50%;
                                         text-align:center;line-height:24px;font-weight:700;
                                         font-size:12px;margin-right:10px;">1</span>
                            Click the login button above
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:8px 0;color:#64748b;font-size:13px;">
                            <span style="display:inline-block;width:24px;height:24px;
                                         background:#ede9fe;color:#6366f1;border-radius:50%;
                                         text-align:center;line-height:24px;font-weight:700;
                                         font-size:12px;margin-right:10px;">2</span>
                            Use the temporary password above to sign in
                          </td>
                        </tr>
                        <tr>
                          <td style="padding:8px 0;color:#64748b;font-size:13px;">
                            <span style="display:inline-block;width:24px;height:24px;
                                         background:#ede9fe;color:#6366f1;border-radius:50%;
                                         text-align:center;line-height:24px;font-weight:700;
                                         font-size:12px;margin-right:10px;">3</span>
                            Set your new secure password when prompted
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="background:#f8fafc;border-top:1px solid #e2e8f0;
                               padding:24px 48px;text-align:center;">
                      <p style="margin:0;color:#94a3b8;font-size:12px;line-height:1.6;">
                        This is an automated message from EMS. Please do not reply to
                        this email.<br/>
                        If you did not expect this email, please contact your HR department.
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>

        </body>
        </html>
        """;
    }
}
