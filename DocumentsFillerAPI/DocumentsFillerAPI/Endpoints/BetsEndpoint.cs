using DocumentsFillerAPI.Providers;
using DocumentsFillerAPI.Structures;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class betsController : ControllerBase
	{
		private BetPostgreProvider _provider = new BetPostgreProvider();

		[HttpGet("get")]
		public async Task<IActionResult> GetBets(uint count, uint startIndex)
		{
			var bets = await _provider.List(count, startIndex);

			var betsJson = JsonSerializer.Serialize(bets.Item2);
			var jsonResult = new JsonObject()
			{
				["message"] = bets.Item1.Message,
				["bets"] = JsonNode.Parse(betsJson)!.AsArray()
			};
			return Ok(jsonResult);
		}

		[HttpPost("insert")]
		public async Task<IActionResult> InsertBets()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var betsToInsert = jBody?["insert"]?.AsArray()?.Select(a => new BetStruct
				{
					BetAmount = (double)a["betAmount"]!,
					HoursAmount = (int)a["hoursAmount"]!,
					TeacherID = (Guid)a["teacherID"]!,
					DepartmentID = (Guid)a["departmentID"]!,
					IsExcessive = (bool)a["isExcessive"]!,
				}).ToList() ?? new List<BetStruct>();

				if (betsToInsert.Count == 0)
					throw new Exception("Bets to insert were empty");

				var insertBetsResult = await _provider.Insert(betsToInsert);

				var jsonResult = new JsonObject()
				{
					["message"] = insertBetsResult.Message,
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
				};

				return BadRequest(jsonResult);
			}
		}

		[HttpPost("update")]
		public async Task<IActionResult> UpdateBets()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var betsToUpdate = jBody?["update"]?.AsArray()?.Select(a => new BetStruct
				{
					ID = (Guid)a["id"]!,
					BetAmount = (double)a["betAmount"]!,
					HoursAmount = (int)a["hoursAmount"]!,
					TeacherID = (Guid)a["teacherID"]!,
					DepartmentID = (Guid)a["departmentID"]!,
					IsExcessive = (bool)a["isExcessive"]!,
				}).ToList() ?? new List<BetStruct>();

				if (betsToUpdate.Count == 0)
					throw new Exception("Bets to update were empty");

				var updateBetsResult = await _provider.Update(betsToUpdate);

				var jsonResult = new JsonObject()
				{
					["message"] = updateBetsResult.Message.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(updateBetsResult.BetsResult.Select(a => new { Message = a.Message, a.IsSuccess, BetID = a.Bet.ID }).ToList()))!.AsArray()
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<object>())),
				};

				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteBets()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var betsToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a!)?.ToList() ?? new List<Guid>();

				if (betsToDelete.Count == 0)
					throw new Exception("Bets to delete were empty");

				var deleteBetsResult = await _provider.Delete(betsToDelete);

				var jsonResult = new JsonObject()
				{
					["message"] = deleteBetsResult.Message,
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
				};

				return BadRequest(jsonResult);
			}
		}
	}
}





