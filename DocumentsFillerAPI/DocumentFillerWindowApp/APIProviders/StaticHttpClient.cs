using System.Net.Http;
using System.Text.Json.Nodes;

namespace DocumentFillerWindowApp.APIProviders
{
	internal static class StaticHttpClient
	{
		private readonly static string ACCESS_TOKEN = "mt2YbHUGpj1EdxJ3LN5RjSZWBZOCCtUQ0Z0gClliq8tnB6MEKbTcEUzXIU7TAsGs";
		//TODO Insert into cfg maybe
		private readonly static string SERVER_URL = "http://localhost:5263/api";
		private static HttpClient _httpClient = new HttpClient();

		static StaticHttpClient()
		{
			_httpClient.Timeout = TimeSpan.FromSeconds(10);
			_httpClient.DefaultRequestHeaders.Add("access_token", ACCESS_TOKEN);
		}

		public static async Task<(T Response, string Message, bool IsSuccess)> Get<T>(string controller, string method, List<KeyValuePair<string, string>> queryParams = default, CancellationToken ct = default) where T : JsonNode
		{
			try
			{
				string uri = $"{SERVER_URL}/{controller.Trim()}/{method.Trim()}";
				if (queryParams != null && queryParams.Count != 0)
				{
					uri += "?";
					uri += string.Join("&", queryParams.Select(param => $"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}"));
				}
				var result = _httpClient.GetAsync(uri, ct).Result;
				var resultString = await result.Content.ReadAsStringAsync();

				if (!result.IsSuccessStatusCode)
					return new(default, $"StatusCode: {result.StatusCode}; Message: {resultString}", false);

				//TODO Скорее всего сломается, но похуй
				return new((T)JsonNode.Parse(resultString), "Success", true);
			}
			catch (Exception ex)
			{
				return new(default, ex.Message, false);
			}
		}

		public static async Task<(T Response, string Message, bool IsSuccess)> Post<T>(string controller, string method, JsonNode jBody, List<KeyValuePair<string, string>> queryParams = default, CancellationToken ct = default) where T : JsonNode
		{
			string uri = $"{SERVER_URL}/{controller.Trim()}/{method.Trim()}";
			if (queryParams != null && queryParams.Count != 0)
			{
				uri += "?";
				uri += string.Join("&", queryParams.Select(param => $"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}"));
			}

			HttpContent httpContent = new StringContent(jBody.ToString());

			var result = await _httpClient.PostAsync(uri, httpContent, ct);
			var resultString = await result.Content.ReadAsStringAsync();

			if (!result.IsSuccessStatusCode)
				return new(default, $"StatusCode: {result.StatusCode}; Message: {resultString}", false);

			//TODO Скорее всего сломается, но похуй
			return new((T)JsonNode.Parse(resultString), "Success", true);
		}
	}
}
