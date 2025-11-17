using DocumentsFillerAPI.Providers;
using DocumentsFillerAPI.Structures;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class departmentsController : ControllerBase
	{
		private DepartmentProvider _provider = new DepartmentProvider();

		[HttpGet("get")]
		public async Task<IActionResult> GetDepartments()
		{
			var departments = await _provider.List(0, 0);

			var departmentsJson = JsonSerializer.Serialize(departments.Item2);
			var jsonResult = new JsonObject()
			{
				["message"] = departments.Item1.Message,
				["departments"] = JsonNode.Parse(departmentsJson)!.AsArray()/*new JsonArray(titles.Item2.Select(a => new JsonObject { [] })) { titles.Item2 }*/

			};
			return Ok(jsonResult);
		}

		[HttpPost("insert")]
		public async Task<IActionResult> InsertTitles()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var departmentsToInsert = jBody?["insert"]?.AsArray()?.Select(a => new DepartmentStruct
				{
					Name = (string)a["name"]!
				}).ToList() ?? new List<DepartmentStruct>();

				if (departmentsToInsert.Count == 0)
					throw new Exception("Departments to insert count = 0");

				var result = await _provider.Insert(departmentsToInsert);

				var jsonResult = new JsonObject()
				{
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(result.Inserted))!.AsArray(),
					["notInsertedMessages"] = JsonNode.Parse(JsonSerializer.Serialize(result.NotInserted))!.AsArray(),
					["result"] = JsonNode.Parse(JsonSerializer.Serialize(result.Result))!.AsObject()
				};

				if (result.Result.IsSuccess)
					return Ok(jsonResult);
				else
					return BadRequest(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<DepartmentStruct>()))!.AsArray(),
					["notInsertedMessages"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(),
					["message"] = ex.Message
				};

				return BadRequest(ex.Message);
			}
		}

		[HttpGet("search")]
		public async Task<IActionResult> SearchDepartments(string searchText)
		{
			try
			{
				var result = await _provider.Search(searchText);

				var jsonResult = new JsonObject()
				{
					["titles"] = JsonNode.Parse(JsonSerializer.Serialize(result.Departments))!.AsArray()
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost("update")]
		public async Task<IActionResult> UpdateDepartments()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var departmentsToUpdate = jBody?["update"]?.AsArray()?.Select(a => new DepartmentStruct
				{
					ID = (Guid)a["id"]!,
					Name = (string)a["name"]!
				}).ToList() ?? new List<DepartmentStruct>();

				if (departmentsToUpdate.Count == 0)
					throw new Exception("Departments is empty");

				var result = await _provider.Update(departmentsToUpdate);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.DepartmentsResults.Select(a => new { Message = a.Message, a.IsSuccess, a.Department.Name }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<DepartmentStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteDepartments()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var departmentsToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a["id"]!).ToList() ?? new List<Guid>();

				if (departmentsToDelete.Count == 0)
					throw new Exception("Departments is empty");

				var result = await _provider.Delete(departmentsToDelete);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.Results.Select(a => new { Message = a.Message, a.IsSuccess, a.DepartmentID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<DepartmentStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}
	}
}
