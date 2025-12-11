using DocumentsFillerAPI.ExcelWorker;
using DocumentsFillerAPI.ExcelHelper;
using DocumentsFillerAPI.Helper;
using DocumentsFillerAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class filesController : ControllerBase
	{
		private FilePostgreProvider _provider = new FilePostgreProvider();
		private TeacherProvider _teacherProvider = new TeacherProvider();
		private BetPostgreProvider _betProvider = new BetPostgreProvider();
		private DepartmentProvider _departmentProvider = new DepartmentProvider();

		private ExcelFilesGenerator _excelFilesGenerator = new ExcelFilesGenerator();
		private ExcelFilesParser _excelFilesParser = new ExcelFilesParser();

		[HttpGet("get")]
		public async Task<IActionResult> GetFiles(uint count, uint startIndex)
		{
			var files = await _provider.List(count, startIndex);

			var filesJson = JsonSerializer.Serialize(files.Files);
			var jsonResult = new JsonObject()
			{
				["message"] = files.Message.Message,
				["files"] = JsonNode.Parse(filesJson)!.AsArray()
			};
			return Ok(jsonResult);
		}

		[HttpGet("download")]
		public async Task<IActionResult> DownloadFile(Guid fileID)
		{
			try
			{
				var files = await _provider.List(0, 0);
				var file = files.Files.FirstOrDefault(f => f.FileID == fileID);

				if (file == null || string.IsNullOrEmpty(file.Path))
					return NotFound(new JsonObject() { ["message"] = "File not found" });

				if (!System.IO.File.Exists(file.Path))
					return NotFound(new JsonObject() { ["message"] = "File not found on disk" });

				var fileStream = new FileStream(file.Path, FileMode.Open, FileAccess.Read);
				var fileName = Path.GetFileName(file.Path);
				
				// Извлекаем оригинальное имя файла (убираем GUID префикс)
				if (fileName.Contains('_'))
				{
					fileName = fileName.Substring(fileName.IndexOf('_') + 1);
				}

				return File(fileStream, "application/octet-stream", fileName);
			}
			catch (Exception ex)
			{
				return BadRequest(new JsonObject() { ["message"] = ex.Message });
			}
		}

		//[HttpPost("insert")]
		//public async Task<IActionResult> InsertFiles()
		//{
		//	try
		//	{
		//		var jBody = Request.GetBodyJson();

		//		var filesToInsert = jBody?["insert"]?.AsArray()?.Select(a => new FileStruct
		//		{
		//			FileType = (Guid)a["fileType"]!,
		//			CreationDate = DateTime.UtcNow,
		//			//Path = (string)a["path"]!,
		//		}).ToList() ?? new List<FileStruct>();

		//		if (filesToInsert.Count == 0)
		//			throw new Exception("Files to insert were empty");

		//		var insertFilesResult = await _provider.InsertFiles(filesToInsert);
		//		//var filesJson = JsonSerializer.Serialize(insertFilesResult);

		//		var jsonResult = new JsonObject()
		//		{
		//			["message"] = insertFilesResult.Message.Message,
		//			["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.InsertedFiles))!.AsArray(),
		//			["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.NotInsertedFiles))!.AsArray(),
		//		};

		//		return Ok(jsonResult);
		//	}
		//	catch (Exception ex)
		//	{
		//		var jsonResult = new JsonObject()
		//		{
		//			["message"] = ex.Message,
		//			["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<MinimalFileInfoStruct>()))!.AsArray(), 
		//			["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(), 
		//		};

		//		return BadRequest(jsonResult);
		//	}
		//}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteFiles()
		{
			try
			{
				var jBody = await Request.GetBodyJson();
				var filesToDelete = jBody?["delete"]?.AsArray()?.Select(a => (Guid)a!)?.ToList() ?? new List<Guid>();

				if (filesToDelete.Count == 0)
					throw new Exception("Files to delete were empty");

				var deleteFilesResult = await _provider.DeleteFiles(filesToDelete);

				var jsonResult = new JsonObject()
				{
					["message"] = deleteFilesResult.Message.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(deleteFilesResult.DeleteResults.Select(a => new { Message = a.Message, a.IsSuccess, a.FileID }).ToList()))
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["deleteResults"] = JsonNode.Parse(JsonSerializer.Serialize(new List<DeleteFilesStruct>())),
				};

				return BadRequest(jsonResult);
			}
		}

		[HttpPost("generateStaffingTable")]
		public async Task<IActionResult> GenerateStaffingTable()
		{
			try
			{
				Guid typeID = new Guid("456dff6f-a4a1-48ef-b5c3-993083bb237d\r\n");

				var jBody = (await Request.GetBodyJson())["staffingTableInfo"]!;
				string fileName = (string)jBody["fileName"]!;
				var data = new ExcelFilesGenerator.StaffingTemplateInputData()
				{
					HeadDepartment = "Л.Н. Цымбалюк"/*(string)jBody["headDepartment"]!*/,
					DepartmentName = (string)jBody["departmentName"]!,
					FirstAcademicYear = (int)jBody["firstAcademicYear"]!,
					SecondAcademicYear = (int)jBody["secondAcademicYear"]!,
					ProtocolNumber = (int)jBody["protocolNumber"]!,
					ProtocolDate = (DateTime)jBody["protocolDate"]!,
					MainStaff = jBody["mainStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.StaffingTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						Bet = (double)a["bet"]!
					}).ToList(),
					InternalStaff = jBody["internalStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.StaffingTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						Bet = (double)a["bet"]!
					}).ToList(),
					ExternalStaff = jBody["externalStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.StaffingTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						Bet = (double)a["bet"]!
					}).ToList(),
				};
				Guid? departmentID = (await _departmentProvider.Get(data.DepartmentName)).Item2?.ID;

				if (departmentID == null)
					throw new Exception("Не удалось найти название кафедры");

				foreach (var staff in data.MainStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.Bet = 0;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.Bet = 0;
						continue;
					}

					staff.Bet = bet.Item2.BetAmount;
				}

				foreach (var staff in data.ExternalStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.Bet = 0;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.Bet = 0;
						continue;
					}

					staff.Bet = bet.Item2.BetAmount;
				}

				foreach (var staff in data.InternalStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.Bet = 0;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.Bet = 0;
						continue;
					}

					staff.Bet = bet.Item2.BetAmount;
				}


				var stream = new MemoryStream();

				_excelFilesGenerator.GenerateStaffingTemplate(data).Write(stream, true);
				stream.Position = 0;

				Stream streamToPg = new MemoryStream();
				stream.CopyTo(streamToPg);
				streamToPg.Position = 0;

				await FileHelper.AddNewFile(streamToPg, fileName, typeID);

				stream.Position = 0;
				return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
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

		[HttpPost("generateServiceMemo")]
		public async Task<IActionResult> GenerateServiceMemo()
		{
			Guid typeID = new Guid("764ee68c-c541-43cf-a4dc-f3284d950ffd");

			try
			{
				var jBody = (await Request.GetBodyJson())["serviceMemoInfo"]!;
				string fileName = (string)jBody["fileName"]!;
				var data = new ExcelFilesGenerator.ServiceMemoInputData()
				{
					DepartmentName = (string)jBody["departmentName"]!,
					Reserve = (double)jBody["reserve"]!,
					FirstAcademicYear = (int)jBody["firstAcademicYear"]!,
					SecondAcademicYear = (int)jBody["secondAcademicYear"]!,
					StudyPeriodDateStart = (DateTime)jBody["studyPeriodDateStart"]!,
					StudyPeriodDateEnd = (DateTime)jBody["studyPeriodDateEnd"]!,
					ProtocolNumber = (int)jBody["protocolNumber"]!,
					ProtocolDateTime = (DateTime)jBody["protocolDateTime"]!,
					MainStaff = jBody["mainStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.ServiceMemoTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						MainBetInfo = a["mainBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["mainBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["mainBetInfo"]!["bet"]!,
						} : null,
						ExcessiveBetInfo = a["excessiveBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["excessiveBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["excessiveBetInfo"]!["bet"]!,
						} : null,
					}).ToList(),
					InternalStaff = jBody["internallStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.ServiceMemoTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						MainBetInfo = a["mainBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["mainBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["mainBetInfo"]!["bet"]!,
						} : null,
						ExcessiveBetInfo = a["excessiveBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["excessiveBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["excessiveBetInfo"]!["bet"]!,
						} : null,
					}).ToList(),
					ExternalStaff = jBody["externalStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.ServiceMemoTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						MainBetInfo = a["mainBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["mainBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["mainBetInfo"]!["bet"]!,
						} : null,
						ExcessiveBetInfo = a["excessiveBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["excessiveBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["excessiveBetInfo"]!["bet"]!,
						} : null,
					}).ToList(),
					HourlyWorkers = jBody["hourlyWorkers"]!.AsArray().Select(a => new ExcelFilesGenerator.ServiceMemoTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						MainBetInfo = a["mainBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["mainBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["mainBetInfo"]!["bet"]!,
						} : null,
						ExcessiveBetInfo = a["excessiveBetInfo"] != null ? new ExcelFilesGenerator.ServiceMemoTemplateBetStruct()
						{
							HoursAmount = (int)a["excessiveBetInfo"]!["hoursAmount"]!,
							Bet = (double)a["excessiveBetInfo"]!["bet"]!,
						} : null,
					}).ToList(),
				};

				Guid? departmentID = (await _departmentProvider.Get(data.DepartmentName)).Item2?.ID;

				if (departmentID == null)
					throw new Exception("Не удалось найти название кафедры");

				foreach (var staff in data.MainStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.MainBetInfo = null;
						staff.ExcessiveBetInfo = null;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.MainBetInfo = null;
					}
					else
					{
						staff.MainBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = bet.Item2.BetAmount,
							HoursAmount = bet.Item2.HoursAmount,
						};
					}

					var excessiveBet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, true);
					if (excessiveBet.Item2?.BetAmount == null)
					{
						staff.ExcessiveBetInfo = null;
					}
					else
					{
						staff.ExcessiveBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = excessiveBet.Item2!.BetAmount,
							HoursAmount = excessiveBet.Item2!.HoursAmount,
						};
					}
				}

				foreach (var staff in data.HourlyWorkers)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.MainBetInfo = null;
						staff.ExcessiveBetInfo = null;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.MainBetInfo = null;
					}
					else
					{
						staff.MainBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = bet.Item2.BetAmount,
							HoursAmount = bet.Item2.HoursAmount,
						};
					}

					var excessiveBet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, true);
					if (excessiveBet.Item2?.BetAmount == null)
					{
						staff.ExcessiveBetInfo = null;
					}
					else
					{
						staff.ExcessiveBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = excessiveBet.Item2!.BetAmount,
							HoursAmount = excessiveBet.Item2!.HoursAmount,
						};
					}
				}

				foreach (var staff in data.InternalStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.MainBetInfo = null;
						staff.ExcessiveBetInfo = null;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.MainBetInfo = null;
					}
					else
					{
						staff.MainBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = bet.Item2.BetAmount,
							HoursAmount = bet.Item2.HoursAmount,
						};
					}

					var excessiveBet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, true);
					if (excessiveBet.Item2?.BetAmount == null)
					{
						staff.ExcessiveBetInfo = null;
					}
					else
					{
						staff.ExcessiveBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = excessiveBet.Item2!.BetAmount,
							HoursAmount = excessiveBet.Item2!.HoursAmount,
						};
					}
				}

				foreach (var staff in data.ExternalStaff)
				{
					var teacherID = await _teacherProvider.FindTeacherByShortName(staff.FullName);
					if (teacherID.TeacherID == Guid.Empty)
					{
						staff.MainBetInfo = null;
						staff.ExcessiveBetInfo = null;
						continue;
					}

					var bet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, false);
					if (bet.Item2?.BetAmount == null)
					{
						staff.MainBetInfo = null;
					}
					else
					{
						staff.MainBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = bet.Item2.BetAmount,
							HoursAmount = bet.Item2.HoursAmount,
						};
					}

					var excessiveBet = await _betProvider.Get(teacherID.TeacherID, departmentID.Value, true);
					if (excessiveBet.Item2?.BetAmount == null)
					{
						staff.ExcessiveBetInfo = null;
					}
					else
					{
						staff.ExcessiveBetInfo = new ExcelFilesGenerator.ServiceMemoTemplateBetStruct
						{
							Bet = excessiveBet.Item2!.BetAmount,
							HoursAmount = excessiveBet.Item2!.HoursAmount,
						};
					}
				}

				var stream = new MemoryStream();

				_excelFilesGenerator.GenerateServiceMemo(data).Write(stream, true);
				stream.Position = 0;

				Stream streamToPg = new MemoryStream();
				stream.CopyTo(streamToPg);
				streamToPg.Position = 0;

				await FileHelper.AddNewFile(streamToPg, fileName, typeID);

				stream.Position = 0;
				return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
				};
				return BadRequest(ex);
			}
		}

		[HttpPost("parsePPSExcelFile")]
		public async Task<IActionResult> ParsePPSExcelFile(IFormFile file)
		{
			string tempFilePath = null;
			try
			{
				if (file == null || file.Length == 0)
				{
					return BadRequest(new JsonObject()
					{
						["message"] = "File is required"
					});
				}

				// Создаем временный файл
				tempFilePath = Path.GetTempFileName();
				
				using (var stream = new MemoryStream())
				{
					await file.CopyToAsync(stream);
					stream.Position = 0;

					// Сохраняем во временный файл
					using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite))
					{
						await stream.CopyToAsync(fileStream);
					}
				}

				// Открываем файл для чтения и парсинга
				using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
				{
					var result = _excelFilesParser.ParsePPSExcelFile(fileStream);

					var jsonResult = new JsonObject()
					{
						["message"] = result.Message,
						["rows"] = JsonNode.Parse(JsonSerializer.Serialize(result.Item2))!.AsArray()
					};

					return Ok(jsonResult);
				}
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["rows"] = JsonNode.Parse(JsonSerializer.Serialize(new List<ExcelFilesParser.PPSParsedRow>()))!.AsArray()
				};
				return BadRequest(jsonResult);
			}
			finally
			{
				// Удаляем временный файл
				if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
				{
					try
					{
						System.IO.File.Delete(tempFilePath);
					}
					catch { }
				}
			}
		}
	}
}
