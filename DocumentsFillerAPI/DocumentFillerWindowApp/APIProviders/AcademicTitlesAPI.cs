using DocumentFillerWindowApp.UserModels;
using System.Text.Json.Nodes;

namespace DocumentFillerWindowApp.APIProviders
{
	internal class AcademicTitlesAPI
	{
		private readonly string className = "academicTitles";

		public async Task<(string Message, List<AcademicTitleRecord> Titles)> Get()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "get");
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var titles = response.Response["titles"]!.AsArray().Select(a => new AcademicTitleRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!
				}).ToList();

				return ("", titles);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<AcademicTitleRecord>());
			}
		}

		public async Task<(List<string> Messages, List<AcademicTitleRecord> Inserted, string Message)> InsertTitles(List<string> names)
		{
			try
			{
				var jsonNames = names.Select(a => new JsonObject() { ["name"] = a }).ToArray();

				var requestBody = new JsonObject()
				{
					["insert"] = new JsonArray(jsonNames)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "insert", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var inserted = response.Response["inserted"]!.AsArray().Select(a => new AcademicTitleRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!
				}).ToList();

				var messages = response.Response["notInsertedMessages"]!.AsArray().Select(a => (string)a!).ToList();
				return new (messages, inserted, "");
			}
			catch (Exception ex)
			{
				return new (new(), new(), ex.Message);
			}
		}

		public async Task<(List<AcademicTitleRecord> Titles, string Message)> Search(string searchText)
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "search", new List<KeyValuePair<string, string>> { new("searchText", searchText) });
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var titles = response.Response["titles"]!.AsArray().Select(a => new AcademicTitleRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!
				}).ToList();

				return new(titles, "");
			}
			catch (Exception ex)
			{
				return new(new(), ex.Message);
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, string Name)> Messages)> Update(List<AcademicTitleRecord> titlesToUpdate)
		{
			try
			{
				var jsonTitles = titlesToUpdate.Select(a => new JsonObject() { ["id"] = a.ID, ["name"] = a.Name }).ToArray();
				var requestBody = new JsonObject()
				{
					["update"] = new JsonArray(jsonTitles)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "update", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var updated = response.Response["updateResults"]!.AsArray().Select(a => (
					(string)a["Message"]!,
					(bool)a["IsSuccess"]!,
					(string)a["Name"]!)).ToList();

				return new ("", updated);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid TitleID)> Messages)> Delete(List<AcademicTitleRecord> titlesToDelete)
		{
			try
			{
				var jsonTitles = titlesToDelete.Select(a => new JsonObject() { ["id"] = a.ID }).ToArray();
				var requestBody = new JsonObject()
				{
					["delete"] = new JsonArray(jsonTitles)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "delete", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var deleted = response.Response["deleteResults"]!.AsArray().Select(a => (
					(string)a["Message"]!,
					(bool)a["IsSuccess"]!,
					(Guid)a["TitleID"]!)).ToList();

				return new ("", deleted);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}
	}
}
