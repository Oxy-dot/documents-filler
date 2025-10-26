namespace DocumentsFillerAPI.Controllers
{
	public record ResultMessage
	{
		public bool IsSuccess { get; init; }
		public string Message { get; init; }
	}
	public class ResultMessage<T>
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; }
		public List<T> Records { get; set; }
	}
}
