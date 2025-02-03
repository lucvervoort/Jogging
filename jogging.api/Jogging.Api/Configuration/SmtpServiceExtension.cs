using System.Net;
using System.Net.Mail;
using Jogging.Domain.Configuration;

namespace Jogging.Api.Configuration;

public static class SmtpServiceExtension
{
    public static void AddSmtpEmailClient(this IServiceCollection services, IConfiguration configuration)
    {
        /*
        var emailConfiguration = configuration.GetSection("email").Get<EmailConfiguration>();
        if (emailConfiguration == null)
        {
            throw new InvalidOperationException("Email configuration is missing.");
        }
        */
        EmailConfiguration emailConfiguration = new EmailConfiguration() { Email = "luc.vervoort@hogent.be", Host = "smtp.sendgrid.net", Password = "", UserName = "apikey", Port = 587 };

        services.AddSingleton(emailConfiguration);
        services.AddTransient(sp => new SmtpClient(emailConfiguration.Host)
        {
            Port = emailConfiguration.Port,
            Credentials = new NetworkCredential(emailConfiguration.UserName, emailConfiguration.Password),
            EnableSsl = true,
            Timeout = 10000
        });
    }
}