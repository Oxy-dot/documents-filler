using DocumentsFillerAPI.ExcelWorker;
using DocumentsFillerAPI.Providers;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.UserModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DocumentsFillerAPI.Endpoints
{
	[ApiController]
	[Route("api/[controller]")]
	public class filesController : ControllerBase
	{
		private FilePostgreProvider _provider = new FilePostgreProvider();
		private ExcelFilesGenerator _excelFilesGenerator = new ExcelFilesGenerator();

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

		[HttpPost("insert")]
		public async Task<IActionResult> InsertFiles()
		{
			try
			{
				var jBody = Request.GetBodyJson();




				var filesToInsert = jBody?["insert"]?.AsArray()?.Select(a => new FileStruct
				{
					FileType = (Guid)a["fileType"]!,
					CreationDate = DateTime.UtcNow,
					//Path = (string)a["path"]!,
				}).ToList() ?? new List<FileStruct>();

				if (filesToInsert.Count == 0)
					throw new Exception("Files to insert were empty");

				var insertFilesResult = await _provider.InsertFiles(filesToInsert);
				//var filesJson = JsonSerializer.Serialize(insertFilesResult);

				var jsonResult = new JsonObject()
				{
					["message"] = insertFilesResult.Message.Message,
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.InsertedFiles))!.AsArray(),
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(insertFilesResult.NotInsertedFiles))!.AsArray(),
				};

				return Ok(jsonResult);
			}
			catch (Exception ex)
			{
				var jsonResult = new JsonObject()
				{
					["message"] = ex.Message,
					["inserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<MinimalFileInfoStruct>()))!.AsArray(), 
					["notInserted"] = JsonNode.Parse(JsonSerializer.Serialize(new List<string>()))!.AsArray(), 
				};

				return BadRequest(jsonResult);
			}
		}

		[HttpPost("delete")]
		public async Task<IActionResult> DeleteFiles()
		{
			try
			{
				var jBody = Request.GetBodyJson();
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

				var jBody = Request.GetBodyJson()["staffingTableInfo"]!;

				var data = new ExcelFilesGenerator.StaffingTemplateInputData()
				{
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
					ExternalStaff = jBody["ExternalStaff"]!.AsArray().Select(a => new ExcelFilesGenerator.StaffingTemplateRow
					{
						FullName = (string)a["fullName"]!,
						AcademicTitle = (string)a["academicTitle"]!,
						Bet = (double)a["bet"]!
					}).ToList(),
				};

				var stream = new MemoryStream();

				_excelFilesGenerator.GenerateStaffingTemplate(data).Write(stream);

				return new FileStreamResult(stream, "application/xml");
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

		[HttpPost("generateServiceMemo")]
		public async Task<IActionResult> GenerateServiceMemo()
		{
			Guid typeID = new Guid("764ee68c-c541-43cf-a4dc-f3284d950ffd");

			try
			{
				var jBody = Request.GetBodyJson()["serviceMemoInfo"]!;

				var data = new ExcelFilesGenerator.ServiceMemoInputData()
				{
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

				var stream = new MemoryStream();

				_excelFilesGenerator.GenerateServiceMemo(data).Write(stream);
				stream.Position = 0;

				return new FileStreamResult(stream, "application/xml");
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
	}
}
