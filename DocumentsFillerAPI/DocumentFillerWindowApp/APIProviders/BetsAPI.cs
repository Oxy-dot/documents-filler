using DocumentFillerWindowApp.UserModels;
using System.Text.Json.Nodes;

namespace DocumentFillerWindowApp.APIProviders
{
	internal class BetsAPI
	{
		private readonly string className = "bets";

		public async Task<(string Message, List<BetRecord> Bets)> Get()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "get", new List<KeyValuePair<string, string>> 
				{ 
					new("count", "0"),
					new("startIndex", "0")
				});
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var bets = response.Response["bets"]!.AsArray().Select(a => new BetRecord
				{
					ID = (Guid)a["ID"]!,
					BetAmount = (double)a["BetAmount"]!,
					HoursAmount = (int)a["HoursAmount"]!,
					TeacherID = a["TeacherID"] != null ? (Guid)a["TeacherID"]! : Guid.Empty,
					DepartmentID = a["DepartmentID"] != null ? (Guid)a["DepartmentID"]! : Guid.Empty,
					IsExcessive = a["IsExcessive"] != null ? (bool)a["IsExcessive"]! : false
				}).ToList();

				return ((string)response.Response["message"]!, bets);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<BetRecord>());
			}
		}

		public async Task<(string Message, List<BetRecord> Inserted)> Insert(List<BetRecord> betsToInsert)
		{
			try
			{
				var json = betsToInsert.Select(a => new JsonObject() 
				{ 
					["betAmount"] = a.BetAmount,
					["hoursAmount"] = a.HoursAmount,
					["teacherID"] = a.TeacherID,
					["departmentID"] = a.DepartmentID,
					["isExcessive"] = a.IsExcessive
				}).ToArray();

				var requestBody = new JsonObject()
				{
					["insert"] = new JsonArray(json)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "insert", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				return ((string)response.Response["message"]!, new List<BetRecord>());
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<BetRecord>());
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid BetID)> Messages)> Update(List<BetRecord> betsToUpdate)
		{
			try
			{
				var json = betsToUpdate.Select(a => new JsonObject() 
				{ 
					["id"] = a.ID,
					["betAmount"] = a.BetAmount,
					["hoursAmount"] = a.HoursAmount,
					["teacherID"] = a.TeacherID,
					["departmentID"] = a.DepartmentID,
					["isExcessive"] = a.IsExcessive
				}).ToArray();

				var requestBody = new JsonObject()
				{
					["update"] = new JsonArray(json)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "update", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var updated = response.Response["updateResults"]!.AsArray().Select(a => (
					(string)a["Message"]!,
					(bool)a["IsSuccess"]!,
					(Guid)a["BetID"]!)).ToList();

				return new((string)response.Response["message"]!, updated);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}

		public async Task<string> Delete(List<BetRecord> betsToDelete)
		{
			try
			{
				var jsonIds = betsToDelete.Select(a => JsonValue.Create(a.ID.ToString())).ToArray();

				var requestBody = new JsonObject()
				{
					["delete"] = new JsonArray(jsonIds)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "delete", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				return (string)response.Response["message"]!;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}

