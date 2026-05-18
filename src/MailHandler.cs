using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TestSkylight;

internal static class MailHandler
{
	public static async Task<bool> Send(ApplicationSettings appConfig)
	{

		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(appConfig.FromAddressee, appConfig.FromAddress));
		message.To.Add(new MailboxAddress("Skylight", appConfig.SkylightAddress));
		message.Subject = $"Updated invitation: Test 7 @ Sat May 16, 2026 ({appConfig.FromAddressee})";


		var builder = new BodyBuilder
		{
			TextBody = @"Meeting"
		,
		};

		// Add an attachment
		builder.Attachments.Add(@"C:\Temp\invite7b.ics");

		message.Body = builder.ToMessageBody();


		using var client = new SmtpClient();
		try
		{
			// Connect to the SMTP server (e.g., Mailtrap, Gmail, etc.)
			await client.ConnectAsync(appConfig.SmtpHost, 25, SecureSocketOptions.None);

			// Authenticate
			//await client.AuthenticateAsync("your-username", "your-password");

			// Send the email
			await client.SendAsync(message);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error sending email: {ex.Message}");
			return false;
		}
		finally
		{
			// Always disconnect cleanly
			await client.DisconnectAsync(true);
		}
		return true;
	}
}
