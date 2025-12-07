using Microsoft.AspNetCore.Mvc;
using DocumentsFillerAPI.Structures;
using DocumentsFillerAPI.Providers;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class academicTitlesController : ControllerBase
	{
		private AcademicTitlePostgreProvider _provider = new AcademicTitlePostgreProvider();
		
		[HttpGet("get")]
		public async Task<IActionResult> GetTitles()
		{
			var titles = await _provider.List(0, 0);

			var titlesJson = JsonSerializer.Serialize(titles.Item2);
			var jsonResult = new JsonObject()
			{
				["message"] = titles.Item1.Message,
				["titles"] = JsonNode.Parse(titlesJson)!.AsArray()/*new JsonArray(titles.Item2.Select(a => new JsonObject { [] })) { titles.Item2 }*/

			};
			return Ok(jsonResult);
		}

		[HttpPost("insert")]
		public async Task<IActionResult> InsertTitles()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var titlesToInsert = jBody?["insert"]?.AsArray()?.Select(a => new AcademicTitleStruct
				{ 
					Name = (string)a["name"]!
				}).ToList() ?? new List<AcademicTitleStruct>();

				if (titlesToInsert.Count == 0)
					throw new Exception("Titles to insert count = 0");

				var result = await _provider.Insert(titlesToInsert);

				var jsonResult = new JsonObject()
				{
					["messages"] = result.Message
				};

				if (result.IsSuccess)
					return Ok(jsonResult);
				else
					return BadRequest(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<AcademicTitleStruct>()))!.AsArray(),
					["notInsertedMessages"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(),
					["message"] = ex.Message
				};

				return BadRequest(ex.Message);
			}
		}

		[HttpGet("search")]
		public async Task<IActionResult> SearchTitles(string searchText)
		{
			try
			{
				var result = await _provider.Search(searchText);

				var jsonResult = new JsonObject()
				{
					["titles"] = JsonNode.Parse(JsonSerializer.Serialize(result.Titles))!.AsArray()
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[HttpPost("update")]
		public async Task<IActionResult> UpdateTitles()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var titlesToUpdate = jBody?["update"]?.AsArray()?.Select(a => new AcademicTitleStruct
				{
					ID = (Guid)a["id"]!,
					Name = (string)a["name"]!
				}).ToList() ?? new List<AcademicTitleStruct>();

				if (titlesToUpdate.Count == 0)
					throw new Exception("Titles is empty");

				var result = await _provider.Update(titlesToUpdate);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.AcademicTitlesResult.Select(a => new { Message = a.Message, a.IsSuccess, a.Title.Name }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<AcademicTitleStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteTitles()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var titlesToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a["id"]!).ToList() ?? new List<Guid>();

				if (titlesToDelete.Count == 0)
					throw new Exception("Titles is empty");

				var result = await _provider.Delete(titlesToDelete);

				var jsonResult = new JsonObject
				{
					["message"] = result.Message.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(result.Results.Select(a => new { Message = a.Message, a.IsSuccess, a.TitleID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<AcademicTitleStruct>()))
				};
				return BadRequest(jsonResult);
			}
		}
	}
}
