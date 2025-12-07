using DocumentsFillerAPI.Controllers;
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
		private BetPostgreProvider _betProvider = new BetPostgreProvider();

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

		[HttpPost("insertTeachersFullInfo")]
		public async Task<IActionResult> InsertTeacherFullInfo()
		{
			try
			{
				var jBody = Request.GetBodyJson();
				var departmentID = (Guid)jBody?["departmentID"]!;
				var teachersInfoToInsert = jBody?["insertTeachersFullInfo"]?.AsArray()?.Select(a => new
				{
					FullName = (string)a["fullName"]!,
					MainBet = (double?)a["mainBet"],
					MainBetHours = (int?)a["mainBetHours"],
					ExcessiveBet = (double?)a["excessiveBet"],
					ExcessiveBetHours = (int?)a["excessiveBetHours"]
				})?.ToList() ?? new();

				if (teachersInfoToInsert.Count == 0)
					throw new Exception("Teachers are empty");

				var currentTeachers = (await _provider.FullList(0, 0)).Teachers.Select(a => $"{a.SecondName} {a.FirstName.First()} {a.Patronymic.First()}").ToList();

				var teachersNamesToInsert = teachersInfoToInsert.Select(a => a.FullName).Except(currentTeachers).ToList();
				var teachersBetsToInsert = (from teacherInfo in teachersInfoToInsert
											join teacherToInsertName in teachersNamesToInsert on teacherInfo.FullName equals teacherToInsertName
											select teacherInfo).ToList();

				var teachersBetsToUpdate = (from teacherInfo in teachersInfoToInsert
											join teacherToUpdateName in teachersNamesToInsert.Intersect(currentTeachers) on teacherInfo.FullName equals teacherToUpdateName
											select teacherInfo).ToList();

				//ResultMessage? mainBetInsertResult = default;
				//ResultMessage? excessiveBetInsertResult = default;

				List<TeacherStruct> insertedTeachers = new List<TeacherStruct>();
				List<string> notInsertedTeachers = new List<string>();
				string teachersInsertMessage = string.Empty;

				string insertMainBetMessage = string.Empty;
				List<BetPostgreProvider.UpdateBetStruct> updatedMainBets = new List<BetPostgreProvider.UpdateBetStruct>();
				string updateMainBetMessage = string.Empty;

				string insertExcessiveBetMessage = string.Empty;
				List<BetPostgreProvider.UpdateBetStruct> updatedExcessiveBets = new List<BetPostgreProvider.UpdateBetStruct>();
				string updateExcessiveBetMessage = string.Empty;


				if (teachersNamesToInsert.Count > 0)
				{
					List<TeacherStruct> tempTeachersStructs = new List<TeacherStruct>();
					foreach (var teacherName in teachersNamesToInsert)
					{
						try
						{
							var teacherNameArray = teacherName.Split(' ');
							var firstNameWithPatronymic = teacherNameArray[1].Split('.');

							tempTeachersStructs.Add(new TeacherStruct
							{
								FirstName = firstNameWithPatronymic[0],
								SecondName = teacherNameArray[0],
								Patronymic = firstNameWithPatronymic.Length == 2 ? firstNameWithPatronymic[1] : ""
							});
						}
						catch { }
					}

					var insertResult = await _provider.Insert(tempTeachersStructs);
					insertedTeachers = insertResult.Inserted;
					notInsertedTeachers = insertResult.NotInserted;
					teachersInsertMessage = insertResult.Message.Message;
				}

				//Update main bets
				var mainBetsToUpdate = teachersBetsToUpdate.Where(a => a.MainBetHours != null && a.MainBet != null).ToList();
				if (mainBetsToUpdate.Count > 0)
				{
					var toUpdate = new List<BetStruct>();
					foreach (var mainBet in mainBetsToUpdate)
					{
						if (mainBet.MainBetHours == null || mainBet.MainBet != null)
							continue;

						var teacherFindResult = await _provider.FindTeacherByShortName(mainBet.FullName);
						if (teacherFindResult.Result.IsSuccess)
						{
							toUpdate.Add(new BetStruct
							{
								DepartmentID = departmentID,
								BetAmount = mainBet.MainBet!.Value,
								HoursAmount = mainBet.MainBetHours.Value,
								IsExcessive = false
							});
						}
					}

					if (toUpdate.Count > 0)
					{
						var updateResult = await _betProvider.Update(toUpdate);
						updatedMainBets = updateResult.BetsResult;
						updateMainBetMessage = updateResult.Message.Message;
					}
				}

				//Insert main bets
				var mainBetsToInsert = teachersBetsToInsert.Where(a => a.MainBetHours != null && a.MainBet != null).ToList();
				if (mainBetsToInsert.Count > 0)
				{
					var toInsert = new List<BetStruct>();
					foreach (var mainBet in mainBetsToInsert)
					{
						if (mainBet.MainBetHours == null || mainBet.MainBet != null)
							continue;

						var teacherFindResult = await _provider.FindTeacherByShortName(mainBet.FullName);
						if (teacherFindResult.Result.IsSuccess)
						{
							toInsert.Add(new BetStruct
							{
								DepartmentID = departmentID,
								BetAmount = mainBet.MainBet!.Value,
								HoursAmount = mainBet.MainBetHours.Value,
								IsExcessive = false
							});
						}
					}

					if (toInsert.Count > 0)
					{
						insertMainBetMessage = (await _betProvider.Insert(toInsert)).Message;
					}
				}

				//Update excessive bets
				var excessiveBetsToUpdate = teachersBetsToUpdate.Where(a => a.ExcessiveBet != null && a.ExcessiveBetHours != null).ToList();
				if (excessiveBetsToUpdate.Count > 0)
				{
					var toUpdate = new List<BetStruct>();
					foreach (var excessiveBet in excessiveBetsToUpdate)
					{
						if (excessiveBet.ExcessiveBetHours == null || excessiveBet.ExcessiveBet != null)
							continue;

						var teacherFindResult = await _provider.FindTeacherByShortName(excessiveBet.FullName);
						if (teacherFindResult.Result.IsSuccess)
						{
							toUpdate.Add(new BetStruct
							{
								DepartmentID = departmentID,
								BetAmount = excessiveBet.ExcessiveBet!.Value,
								HoursAmount = excessiveBet.ExcessiveBetHours!.Value,
								IsExcessive = true
							});
						}
					}

					if (toUpdate.Count > 0)
					{
						var updateResult = await _betProvider.Update(toUpdate);
						updatedExcessiveBets = updateResult.BetsResult;
						updateExcessiveBetMessage = updateResult.Message.Message;
					}
				}

				//Insert excessive bets
				var excessiveBetsToInsert = teachersBetsToInsert.Where(a => a.ExcessiveBetHours != null && a.ExcessiveBet != null).ToList();
				if (excessiveBetsToInsert.Count > 0)
				{
					var toInsert = new List<BetStruct>();
					foreach (var excessiveBet in excessiveBetsToInsert)
					{
						if (excessiveBet.ExcessiveBetHours == null || excessiveBet.ExcessiveBet != null)
							continue;

						var teacherFindResult = await _provider.FindTeacherByShortName(excessiveBet.FullName);
						if (teacherFindResult.Result.IsSuccess)
						{
							toInsert.Add(new BetStruct
							{
								DepartmentID = departmentID,
								BetAmount = excessiveBet.ExcessiveBet!.Value,
								HoursAmount = excessiveBet.ExcessiveBetHours!.Value,
								IsExcessive = true
							});
						}
					}

					if (toInsert.Count > 0)
					{
						insertExcessiveBetMessage = (await _betProvider.Insert(toInsert)).Message;
					}
				}

				var jsonResult = new JsonObject
				{
					["message"] = "Success",
					["teachers"] = new JsonObject
					{
						["message"] = teachersInsertMessage,
						["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertedTeachers)),
						["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(notInsertedTeachers))
					},
					["mainBets"] = new JsonObject
					{
						["insertMessage"] = insertMainBetMessage,
						["updateMessage"] = updateMainBetMessage,
						["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(updatedMainBets))
					},
					["excessiveBets"] = new JsonObject
					{
						["insertMessage"] = insertExcessiveBetMessage,
						["updateMessage"] = updateExcessiveBetMessage,
						["updateResults"] = JsonNode.Parse(JsonSerializer.Serialize(updatedExcessiveBets))
					}
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject
				{
					["message"] = ex.Message,
				};
				return BadRequest(jsonResult);
			}
		}
	}
}
