using DocumentFillerWindowApp.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
					Name = (string)a["Name"]!
				}).ToList();

				return ("", departments);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<DepartmentRecord>());
			}
		}

		public async Task<(List<string> Messages, List<DepartmentRecord> Inserted, string Message)> InsertDepartments(List<string> names)
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

				var inserted = response.Response["inserted"]!.AsArray().Select(a => new DepartmentRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!
				}).ToList();

				var messages = response.Response["notInsertedMessages"]!.AsArray().Select(a => (string)a!).ToList();
				return new(messages, inserted, "");
			}
			catch (Exception ex)
			{
				return new(new(), new(), ex.Message);
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

				var departments = response.Response["departments"]!.AsArray().Select(a => new DepartmentRecord
				{
					ID = (Guid)a["ID"]!,
					Name = (string)a["Name"]!
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
				var jsonTitles = departmentsToUpdate.Select(a => new JsonObject() { ["id"] = a.ID, ["name"] = a.Name }).ToArray();
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
					(Guid)a["TitleID"]!)).ToList();

				return new("", deleted);
			}
			catch (Exception ex)
			{
				return new(ex.Message, new());
			}
		}
	}
}
