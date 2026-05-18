namespace TestSkylight;

internal class DeliveryInfo
{
	public DateOnly DeliveryDate { get; set; }
	public int Sequence { get; set; } = -1;
	public DateTime Timestamp { get; set; } = DateTime.Now;
}
