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
					AcademicTitle = a["AcademicTitle"] == null ? null : new AcademicTitleRecord 
					{
						ID = (Guid)a["AcademicTitle"]!["ID"]!,
						Name = (string)a["AcademicTitle"]!["Name"]!,
					}
					//Name = (string)a["Name"]!
				}).ToList();
				
				return ((string)response.Response["message"]!, teachers);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<TeacherRecord>());
			}
		}

		public async Task<string> InsertTeachers(List<TeacherRecord> teachersToInsert)
		{
			try
			{
				var json = teachersToInsert.Select(a => new JsonObject() 
				{ 
					["firstName"] = a.FirstName,
					["secondName"] = a.SecondName,
					["patronymic"] = a.Patronymic,
					["academicTitleID"] = a.AcademicTitle?.ID ?? Guid.Empty
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

				var message = response.Response["message"] != null ? (string)response.Response["message"]! : "";
				return message;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid ID)> Messages)> Update(List<TeacherRecord> teachersToUpdate)
		{
			try
			{
				var jsonTitles = teachersToUpdate.Select(a => new JsonObject() 
				{ 
					["id"] = a.ID,
					["firstName"] = a.FirstName,
					["secondName"] = a.SecondName,
					["patronymic"] = a.Patronymic,
					["academicTitleID"] = a.AcademicTitle?.ID ?? Guid.Empty
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

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid TitleID)> Messages)> Delete(List<TeacherRecord> teachersToDelete)
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

				string responseMessage = (string)response.Response["message"]!;

				string message = string.Empty;
				if (responseMessage == "Успешно")
				{
					string teacherInsertResult = (string)response.Response!["teachers"]!["message"]!;
					string mainBetsInsertResult = (string)response.Response!["mainBets"]!["insertMessage"]!;
					string mainBetsUpdateResult = (string)response.Response!["mainBets"]!["updateMessage"]!;
					string excessiveBetsInsertResult = (string)response.Response!["excessiveBets"]!["insertMessage"]!;
					string excessiveBetsUpdateResult = (string)response.Response!["excessiveBets"]!["updateMessage"]!;

					message = $"Ошибки вставки преподавателей: {teacherInsertResult}\nОшибки вставки нормативных ставок: {mainBetsInsertResult}\nОшибки обновлений нормативных ставок: {mainBetsUpdateResult}\nОшибки вставки сверхнормативных ставок: {excessiveBetsInsertResult}\nОшибки обновления сверхнормативных ставок: {excessiveBetsUpdateResult}";
				}
				else
					message = responseMessage;
				
				return (message, response.IsSuccess);
			}
			catch (Exception ex)
			{
				return (ex.Message, false);
			}
		}
	}
}
