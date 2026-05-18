using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using TestSkylight;

var builder = Host.CreateApplicationBuilder(args);
var appSettings = builder.Configuration.GetSection("app").Get<ApplicationSettings>();
if (appSettings is null)
	throw new Exception("Failed to load configuration.");

var deliveryInfo = await DeliveryInfoHandler.GetOrCreate();

var processingInfo = new ProcessingInfo();
var lookup = new DeliveryInfoLookup(deliveryInfo, appSettings, processingInfo);
var (success, shouldSend) = await lookup.GetDeliveryInfo();

if (shouldSend)
	Console.WriteLine("Sending email...");
else
	Console.WriteLine("No changes.");

//var result = await MailHandler.Send(appConfig);