using DocumentsFillerAPI.Providers;
using DocumentsFillerAPI.Structures;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class teachersController : ControllerBase
	{
		private TeacherProvider _provider = new TeacherProvider();

		[HttpGet("get")]
		public async Task<IActionResult> GetTeachers()
		{
			var teachers = await _provider.List(0, 0);

			var teachersJson = JsonSerializer.Serialize(teachers.Teachers);
			var jsonResult = new JsonObject()
			{
				["message"] = teachers.Message.Message,
				["teachers"] = JsonNode.Parse(teachersJson)!.AsArray()
			};

			return Ok(jsonResult);
		}

		[HttpGet("getFullInfo")]
		public async Task<IActionResult> GetTeachersFullInfo()
		{
			var teachers = await _provider.FullList(0, 0);

			var teachersJson = JsonSerializer.Serialize(teachers.Teachers);
			var jsonResult = new JsonObject()
			{
				["message"] = teachers.Message.Message,
				["teachers"] = JsonNode.Parse(teachersJson)!.AsArray()
			};

			return Ok(jsonResult);
		}

		[HttpPost("insert")]
		public async Task<IActionResult> InsertTitles()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var teachersToInsert = jBody?["insert"]?.AsArray()?.Select(a => new TeacherStruct
				{
					FirstName = (string)a["firstName"]!,
					SecondName = (string)a["secondName"]!,
					Patronymic = (string)a["patronymic"]!
				}).ToList() ?? new List<TeacherStruct>();

				if (teachersToInsert.Count == 0)
					throw new Exception("Teachers to insert count = 0");

				var result = await _provider.Insert(teachersToInsert);

				var jsonResult = new JsonObject()
				{
					["message"] = result.Message.Message,
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(result.Inserted))!.AsArray(),
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(result.NotInserted))!.AsArray(),
				};

				if (result.Message.IsSuccess)
					return Ok(jsonResult);
				else
					return BadRequest(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<TeacherStruct>()))!.AsArray(),
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(),
					["message"] = ex.Message
				};

				return BadRequest(ex.Message);
			}
		}

		[HttpPost("update")]
		public async Task<IActionResult> UpdateTeachers()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var teachersToUpdate = jBody?["update"]?.AsArray()?.Select(a => new TeacherStruct
				{
					ID = (Guid)a["id"]!,
					FirstName = (string)a["firstName"]!,
					SecondName = (string)a["secondName"]!,
					Patronymic = (string)a["patronymic"]!,
					MainBetID = (Guid)a["mainBetID"]!,
					SecondBetID = (Guid)a["secondBetID"]!,
					ExcessiveBetID = (Guid)a["excessiveBetID"]!,
				}).ToList() ?? new List<TeacherStruct>();

				if (teachersToUpdate.Count == 0)
					throw new Exception("Teachers are empty");

				var result = await _provider.Update(teachersToUpdate);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.TeachersResult.Select(a => new { Message = a.Message, a.IsSuccess, a.Teacher.ID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<TeacherStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteTeachers()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var teachersToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a["id"]!).ToList() ?? new List<Guid>();

				if (teachersToDelete.Count == 0)
					throw new Exception("Teachers are empty");

				var result = await _provider.Delete(teachersToDelete);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.DeleteResults.Select(a => new { Message = a.Message, a.IsSuccess, a.TeacherID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<TeacherStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}
	}
}
