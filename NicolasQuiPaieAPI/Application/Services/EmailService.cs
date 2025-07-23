using NicolasQuiPaieAPI.Application.Interfaces;

namespace NicolasQuiPaieAPI.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // TODO: Implement actual email sending with SMTP or email service provider
            // For now, log the email details
            _logger.LogInformation("Email would be sent to {To} with subject: {Subject}", to, subject);
            
            // Simulate async operation
            await Task.Delay(100);
            
            // In production, you would use:
            // - SMTP client
            // - SendGrid, Mailgun, or similar service
            // - Azure Communication Services
            // - AWS SES
        }

        public async Task SendWelcomeEmailAsync(string to, string displayName)
        {
            var subject = "Bienvenue sur Nicolas Qui Paie !";
            var body = $@"
                <h1>Bienvenue {displayName} !</h1>
                <p>Votre compte Nicolas Qui Paie a �t� cr�� avec succ�s.</p>
                <p>Vous pouvez maintenant :</p>
                <ul>
                    <li>Cr�er des propositions fiscales</li>
                    <li>Voter sur les propositions de la communaut�</li>
                    <li>Participer aux d�bats citoyens</li>
                </ul>
                <p>Ensemble, reprenons le contr�le de nos finances publiques !</p>
                <p><strong>L'�quipe Nicolas Qui Paie</strong></p>
            ";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "R�initialisation de votre mot de passe - Nicolas Qui Paie";
            var body = $@"
                <h1>R�initialisation de mot de passe</h1>
                <p>Vous avez demand� la r�initialisation de votre mot de passe.</p>
                <p>Cliquez sur le lien suivant pour d�finir un nouveau mot de passe :</p>
                <p><a href='{resetLink}'>R�initialiser mon mot de passe</a></p>
                <p>Ce lien expirera dans 24 heures.</p>
                <p>Si vous n'avez pas demand� cette r�initialisation, ignorez cet email.</p>
                <p><strong>L'�quipe Nicolas Qui Paie</strong></p>
            ";

            await SendEmailAsync(to, subject, body);
        }
    }
}