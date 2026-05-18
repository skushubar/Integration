using System.Reflection;
using System.Text.Json;

namespace TestSkylight;

internal static class DeliveryInfoHandler
{
	public static async Task<DeliveryInfo> GetOrCreate()
	{
		var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		var filePath = Path.Join(folderPath, "DeliveryInfo.json");
		if (File.Exists(filePath))
			return await GetDirectoryInfo(filePath);
		else
			return await CreateDirectoryInfo(filePath);
	}

	private static async Task<DeliveryInfo> CreateDirectoryInfo(string filePath)
	{
		var deliveryInfo = new DeliveryInfo();
		await Write(deliveryInfo, filePath);
		return deliveryInfo;
	}

	private static async Task Write(DeliveryInfo deliveryInfo, string filePath)
	{
		var options = new JsonSerializerOptions { WriteIndented = true };
		string jsonString = JsonSerializer.Serialize(deliveryInfo, options);
		await File.WriteAllTextAsync(filePath, jsonString);
	}

	private static async Task<DeliveryInfo> GetDirectoryInfo(string filePath)
	{
		string jsonString = await File.ReadAllTextAsync(filePath);
		return JsonSerializer.Deserialize<DeliveryInfo>(jsonString)!;
	}
}
