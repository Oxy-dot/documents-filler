using DocumentFillerWindowApp.UserModels;
using System.Text.Json.Nodes;

namespace DocumentFillerWindowApp.APIProviders
{
	internal class DepartmentsAPI
	{
		private readonly string className = "departments";

		public async Task<(string Message, List<DepartmentRecord> Departments)> Get()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "get");
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var departments = response.Response["departments"]!.AsArray().Select(a => new DepartmentRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!,
					FullName = a["FullName"] != null ? (string)a["FullName"]! : ""
				}).ToList();

				return ((string)response.Response["message"]!, departments);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<DepartmentRecord>());
			}
		}

		public async Task<string> InsertDepartments(List<DepartmentRecord> departments)
		{
			try
			{
				var jsonNames = departments.Select(a => new JsonObject() { ["name"] = a.Name, ["fullName"] = a.FullName }).ToArray();

				var requestBody = new JsonObject()
				{
					["insert"] = new JsonArray(jsonNames)
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

		public async Task<(List<DepartmentRecord> Departments, string Message)> Search(string searchText)
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "search", new List<KeyValuePair<string, string>> { new("searchText", searchText) });
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var departments = response.Response["titles"]!.AsArray().Select(a => new DepartmentRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!,
					FullName = a["FullName"] != null ? (string)a["FullName"]! : ""
				}).ToList();

				return new(departments, "");
			}
			catch (Exception ex)
			{
				return new(new(), ex.Message);
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, string Name)> Messages)> Update(List<DepartmentRecord> departmentsToUpdate)
		{
			try
			{
				var jsonTitles = departmentsToUpdate.Select(a => new JsonObject() 
				{ 
					["id"] = a.ID, 
					["name"] = a.Name,
					["fullName"] = a.FullName
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
					(string)a["Name"]!)).ToList();

				return new("", updated);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}

		public async Task<(string Message, List<(string Message, bool IsSuccess, Guid TitleID)> Messages)> Delete(List<DepartmentRecord> departmentsToDelete)
		{
			try
			{
				var jsonTitles = departmentsToDelete.Select(a => new JsonObject() { ["id"] = a.ID }).ToArray();
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
					(Guid)a["DepartmentID"]!)).ToList();

				return new("", deleted);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}
	}
}
