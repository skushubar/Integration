using System.Text.RegularExpressions;

namespace TestSkylight;

internal class DeliveryInfoLookup
{
	private ApplicationSettings AppSettings { get; }
	private Uri BaseUri { get; }
	private ProcessingInfo ProcessingInfo { get; }

	private HttpClient HttpClient { get; }
	private HttpClientHandler HttpClientHandler { get; }

	public DeliveryInfoLookup(DeliveryInfo deliveryInfo, ApplicationSettings settings, ProcessingInfo processInfo)
	{
		AppSettings = settings;
		ProcessingInfo = processInfo;
		BaseUri = new Uri(AppSettings.BaseUrl);
		HttpClientHandler = new HttpClientHandler
		{
			UseCookies = true
		};
		HttpClient = new HttpClient(HttpClientHandler);
	}

	public async Task<(bool success, bool shouldSend)> GetDeliveryInfo()
	{
		var nextDeliveryDate = await GetNextDeliveryDate();
		return (true, false);
	}

	private async Task<bool> GetNextDeliveryDate()
	{
		var success = await ParseLoginPageForUserSecret();
		return false;
	}

	private async Task<bool> ParseLoginPageForUserSecret()
	{
		var response = await Client.GetAsync(new Uri(BaseUri, "en/login"));
		string responseBody = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
		{
			throw new InvalidOperationException(responseBody);
		}
		return await CheckLinks(responseBody);
	}

	private async Task<bool> CheckLinks(string responseBody)
	{
		var links = GetLinks(responseBody);
		return await ParseLinks(links);

		static IEnumerable<string> GetLinks(string responseBody)
		{
			string pattern = @"\<script src=""([^""]*)""";
			var rgx = new Regex(pattern);
			var links = rgx.Matches(responseBody).Select(m => m.Groups[1].Value);
			return links;
		}

		async Task<bool> ParseLinks(IEnumerable<string> links)
		{
			foreach (var link in links)
			{
				var success = await ParseLink(link);
				if (success)
				{
					return true;
				}
			}
			throw new Exception("User secret not found.");

			async Task<bool> ParseLink(string link)
			{
				var response = await Client.GetAsync(new Uri(BaseUri, link));
				string responseBody = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception(responseBody);
				}

				return await ParseLinkFile(responseBody);
			}

			async Task<bool> ParseLinkFile(string responseBody)
			{
				string pattern = @"client_secret:""([^""]*)""";
				var rgx = new Regex(pattern);
				var match = rgx.Match(responseBody);
				if (match.Success)
				{
					ProcessingInfo.ClientSecret = match.Groups[1].Value;
				}
				return match.Success;
			}
		}
	}

	private static async Task<(bool Success, string? ErrorMessage)> LoginAsync(string username, string password)
	{
		var content = new FormUrlEncodedContent(new[]
		{
			new KeyValuePair<string, string>("username", username),
			new KeyValuePair<string, string>("password", password),
		});
		var response = await Client.PostAsync("https://www.readyrefresh.com/en/login", content);
		if (response.IsSuccessStatusCode)
		{
			return (true, null);
		}
		else
		{
			string errorMessage = await response.Content.ReadAsStringAsync();
			return (false, errorMessage);
		}
	}

	private static readonly HttpClient Client = new()
	{
		BaseAddress = new Uri("https://www.readyrefresh.com/en/login"),
	};
}
