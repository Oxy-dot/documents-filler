using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI
{
	public static class Extensions
	{
		public static JsonNode GetBodyJson(this HttpRequest request)
		{
			try
			{
				if (request.Body.Length == 0)
					throw new Exception("Body is empty");

				return JsonNode.Parse(request.Body)!;
			}
			catch (Exception)
			{
				return new JsonObject();
			}
		}
	}
}
