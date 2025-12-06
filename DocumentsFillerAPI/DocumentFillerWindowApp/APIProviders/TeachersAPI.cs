using DocumentFillerWindowApp.UserModels;
using System.Text.Json.Nodes;

namespace DocumentFillerWindowApp.APIProviders
{
	internal class TeachersAPI
	{
		private readonly string className = "teachers";

		public async Task<(string Message, List<TeacherRecord> Teachers)> GetFullInfo()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "getFullInfo");
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var teachers = response.Response["teachers"]!.AsArray().Select(a => new TeacherRecord
				{
					ID = (Guid)a["ID"]!,
					FirstName = (string)a["FirstName"]!,
					SecondName = (string)a["SecondName"]!,
					Patronymic = (string)a["Patronymic"]!,
					AcademicTitle = new AcademicTitleRecord 
					{
						ID = (Guid)a["AcademicTitle"]!["ID"]!,
						Name = (string)a["AcademicTitle"]!["Name"]!,
					}
					//Name = (string)a["Name"]!
				}).ToList();

				return ("", teachers);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<TeacherRecord>());
			}
		}

		public async Task<(string Message, List<MinimalTeacherRecord> Teachers)> Get()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "get");
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var teachers = response.Response["teachers"]!.AsArray().Select(a => new MinimalTeacherRecord
				{
					ID = (Guid)a["ID"]!,
					FirstName = (string)a["FirstName"]!,
					SecondName = (string)a["SecondName"]!,
					Patronymic = (string)a["Patronymic"]!,
				}).ToList();

				return ("", teachers);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<MinimalTeacherRecord>());
			}
		}

		public async Task<(List<string> Messages, List<MinimalTeacherRecord> Inserted, string Message)> InsertTeachers(List<MinimalTeacherRecord> teachersToInsert)
		{
			try
			{
				var json = teachersToInsert.Select(a => new JsonObject() 
				{ 
					["firstName"] = a.FirstName,
					["secondName"] = a.SecondName,
					["patronymic"] = a.Patronymic,
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

				var inserted = response.Response["inserted"]!.AsArray().Select(a => new MinimalTeacherRecord
				{
					ID = (Guid)a["ID"]!,
					FirstName = (string)a["FirstName"]!,
					SecondName = (string)a["SecondName"]!,
					Patronymic = (string)a["Patronymic"]!,
				}).ToList();

				var messages = response.Response["notInsertedMessages"]!.AsArray().Select(a => (string)a!).ToList();
				return new(messages, inserted, "");
			}
			catch (Exception ex)
			{
				return new(new(), new(), ex.Message);
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid ID)> Messages)> Update(List<MinimalTeacherRecord> teachersToUpdate)
		{
			try
			{
				var jsonTitles = teachersToUpdate.Select(a => new JsonObject() 
				{ 
					["id"] = a.ID,
					["firstName"] = a.FirstName,
					["secondName"] = a.SecondName,
					["patronymic"] = a.Patronymic,
				}).ToArray();
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
					(Guid)a["ID"]!)).ToList();

				return new("", updated);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid TitleID)> Messages)> Delete(List<MinimalTeacherRecord> teachersToDelete)
		{
			try
			{
				var jsonTitles = teachersToDelete.Select(a => new JsonObject() { ["id"] = a.ID }).ToArray();
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
					(Guid)a["TeacherID"]!)).ToList();

				return new("", deleted);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}

		public async Task<(string Message, bool IsSuccess)> InsertTeachersFullInfo(Guid departmentID, List<(string FullName, double? MainBet, int? MainBetHours, double? ExcessiveBet, int? ExcessiveBetHours)> teachersInfo)
		{
			try
			{
				var jsonTeachers = teachersInfo.Select(a => new JsonObject()
				{
					["fullName"] = a.FullName,
					["mainBet"] = a.MainBet.HasValue ? a.MainBet.Value : null,
					["mainBetHours"] = a.MainBetHours.HasValue ? a.MainBetHours.Value : null,
					["excessiveBet"] = a.ExcessiveBet.HasValue ? a.ExcessiveBet.Value : null,
					["excessiveBetHours"] = a.ExcessiveBetHours.HasValue ? a.ExcessiveBetHours.Value : null
				}).ToArray();

				var requestBody = new JsonObject()
				{
					["departmentID"] = departmentID,
					["insertTeachersFullInfo"] = new JsonArray(jsonTeachers)
				};

				var response = await StaticHttpClient.Post<JsonObject>(className, "insertTeachersFullInfo", requestBody);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var message = response.Response["message"] != null ? (string)response.Response["message"]! : "";
				return (message, response.IsSuccess);
			}
			catch (Exception ex)
			{
				return (ex.Message, false);
			}
		}
	}
}
