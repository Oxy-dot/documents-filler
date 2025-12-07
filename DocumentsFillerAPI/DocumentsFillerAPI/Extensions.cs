using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;

namespace DocumentsFillerAPI
{
	public static class Extensions
	{
		public static async Task<JsonNode> GetBodyJson(this HttpRequest request)
		{
			try
			{
				request.EnableBuffering();
				request.Body.Position = 0;

				using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
				{
					var bodyString = await reader.ReadToEndAsync();
					request.Body.Position = 0;

					if (string.IsNullOrWhiteSpace(bodyString))
						return new JsonObject();

					return JsonNode.Parse(bodyString) ?? new JsonObject();
				}
			}
			catch (Exception)
			{
				return new JsonObject();
			}
		}

		public static JsonNode GetBodyJson1(this HttpRequest request)
		{
			try
			{
				request.EnableBuffering();
				request.Body.Position = 0;

				using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
				{
					var bodyString = reader.ReadToEnd();
					request.Body.Position = 0;

					if (string.IsNullOrWhiteSpace(bodyString))
						return new JsonObject();

					return JsonNode.Parse(bodyString) ?? new JsonObject();
				}
			}
			catch (Exception)
			{
				return new JsonObject();
			}
		}
	}
}
