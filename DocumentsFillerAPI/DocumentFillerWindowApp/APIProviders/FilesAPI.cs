using DocumentFillerWindowApp.UserModels;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace DocumentFillerWindowApp.APIProviders
{
    internal class FilesAPI
    {
		private readonly string className = "files";

		public async Task<(string Message, Stream? Result)> GenerateStaffingTable(StaffingTemplateData data, string fileName)
		{
			try
			{
				// Добавляем расширение .xlsx, если его нет
				if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
				{
					fileName = fileName + ".xlsx";
				}

				var jBodyRequest = new JsonObject
				{
					["staffingTableInfo"] = new JsonObject()
					{
						["fileName"] = fileName,
						["departmentName"] = data.DepartmentName,
						["firstAcademicYear"] = data.FirstAcademicYear,
						["secondAcademicYear"] = data.SecondAcademicYear,
						["protocolNumber"] = data.ProtocolNumber,
						["protocolDate"] = data.ProtocolDate,
						["mainStaff"] = new JsonArray(data.MainStaff.Select(a => new JsonObject() 
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["bet"] = a.Bet
						}).ToArray()),
						["internalStaff"] = new JsonArray(data.InternalStaff.Select(a => new JsonObject()
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["bet"] = a.Bet
						}).ToArray()),
						["externalStaff"] = new JsonArray(data.ExternalStaff.Select(a => new JsonObject()
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["bet"] = a.Bet
						}).ToArray()),
					}
				};

				var response = await StaticHttpClient.PostStream(className, "generateStaffingTable", jBodyRequest);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				return new("", response.Response);
			}
			catch (Exception ex)
			{
				return new(ex.Message, default(Stream?));
			}
		}

		public readonly record struct StaffingTemplateData
		{
			public string DepartmentName { get; init; }
			public List<StaffingTemplateRowData> MainStaff { get; init; }
			public List<StaffingTemplateRowData> InternalStaff { get; init; }
			public List<StaffingTemplateRowData> ExternalStaff { get; init; }
			public int FirstAcademicYear { get; init; }
			public int SecondAcademicYear { get; init; }
			public int ProtocolNumber { get; init; }
			public DateTime ProtocolDate { get; init; }
		}

		public readonly record struct StaffingTemplateRowData
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public double Bet { get; init; }
		}

		public async Task<(string Message, Stream? Result)> GenerateServiceMemo(ServiceMemoTemplateData data, string fileName)
		{
			try
			{
				// Добавляем расширение .xlsx, если его нет
				if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
				{
					fileName = fileName + ".xlsx";
				}

				var jBodyRequest = new JsonObject
				{
					["serviceMemoInfo"] = new JsonObject()
					{
						["fileName"] = fileName,
						["departmentName"] = data.DepartmentName,
						["reserve"] = data.Reserve,
						["firstAcademicYear"] = data.FirstAcademicYear,
						["secondAcademicYear"] = data.SecondAcademicYear,
						["studyPeriodDateStart"] = data.StudyPeriodDateStart,
						["studyPeriodDateEnd"] = data.StudyPeriodDateEnd,
						["protocolNumber"] = data.ProtocolNumber,
						["protocolDateTime"] = data.ProtocolDateTime,
						["mainStaff"] = new JsonArray(data.MainStaff.Select(a => new JsonObject() 
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["mainBetInfo"] = a.MainBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.MainBetInfo.Value.HoursAmount,
								["bet"] = a.MainBetInfo.Value.Bet
							} : null,
							["excessiveBetInfo"] = a.ExcessiveBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.ExcessiveBetInfo.Value.HoursAmount,
								["bet"] = a.ExcessiveBetInfo.Value.Bet
							} : null,
						}).ToArray()),
						["internallStaff"] = new JsonArray(data.InternallStaff.Select(a => new JsonObject()
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["mainBetInfo"] = a.MainBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.MainBetInfo.Value.HoursAmount,
								["bet"] = a.MainBetInfo.Value.Bet
							} : null,
							["excessiveBetInfo"] = a.ExcessiveBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.ExcessiveBetInfo.Value.HoursAmount,
								["bet"] = a.ExcessiveBetInfo.Value.Bet
							} : null,
						}).ToArray()),
						["externalStaff"] = new JsonArray(data.ExternalStaff.Select(a => new JsonObject()
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["mainBetInfo"] = a.MainBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.MainBetInfo.Value.HoursAmount,
								["bet"] = a.MainBetInfo.Value.Bet
							} : null,
							["excessiveBetInfo"] = a.ExcessiveBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.ExcessiveBetInfo.Value.HoursAmount,
								["bet"] = a.ExcessiveBetInfo.Value.Bet
							} : null,
						}).ToArray()),
						["hourlyWorkers"] = new JsonArray(data.HourlyWorkers.Select(a => new JsonObject()
						{
							["fullName"] = a.FullName,
							["academicTitle"] = a.AcademicTitle,
							["mainBetInfo"] = a.MainBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.MainBetInfo.Value.HoursAmount,
								["bet"] = a.MainBetInfo.Value.Bet
							} : null,
							["excessiveBetInfo"] = a.ExcessiveBetInfo != null ? new JsonObject()
							{
								["hoursAmount"] = a.ExcessiveBetInfo.Value.HoursAmount,
								["bet"] = a.ExcessiveBetInfo.Value.Bet
							} : null,
						}).ToArray()),
					}
				};

				var response = await StaticHttpClient.PostStream(className, "generateServiceMemo", jBodyRequest);
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				return new("", response.Response);
			}
			catch (Exception ex)
			{
				return new(ex.Message, default(Stream?));
			}
		}

		public readonly record struct ServiceMemoTemplateData
		{
			public string DepartmentName { get; init; }
			public double Reserve { get; init; }
			public int FirstAcademicYear { get; init; }
			public int SecondAcademicYear { get; init; }
			public DateTime StudyPeriodDateStart { get; init; }
			public DateTime StudyPeriodDateEnd { get; init; }
			public List<ServiceMemoTemplateRowData> MainStaff { get; init; }
			public List<ServiceMemoTemplateRowData> InternallStaff { get; init; }
			public List<ServiceMemoTemplateRowData> ExternalStaff { get; init; }
			public List<ServiceMemoTemplateRowData> HourlyWorkers { get; init; }
			public int ProtocolNumber { get; init; }
			public DateTime ProtocolDateTime { get; init; }
		}

		public readonly record struct ServiceMemoTemplateRowData
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public ServiceMemoTemplateBetStructData? MainBetInfo { get; init; }
			public ServiceMemoTemplateBetStructData? ExcessiveBetInfo { get; init; }
		}

		public readonly record struct ServiceMemoTemplateBetStructData
		{
			public int HoursAmount { get; init; }
			public double Bet { get; init; }
		}

		public async Task<(string Message, List<PPSParsedRow> Rows)> ParsePPSExcelFile(string filePath)
		{
			try
			{
				using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				using (var memoryStream = new MemoryStream())
				{
					await fileStream.CopyToAsync(memoryStream);
					memoryStream.Position = 0;

					var response = await StaticHttpClient.PostFile<JsonObject>(className, "parsePPSExcelFile", memoryStream, Path.GetFileName(filePath));
					if (!response.IsSuccess)
						throw new Exception(response.Message);

					if (response.Response == null)
						throw new Exception("Response is null");

					var message = response.Response["message"] != null ? (string)response.Response["message"]! : "";
					var rows = response.Response["rows"]!.AsArray().Select(a => new PPSParsedRow
					{
						MainBet = a["MainBet"] != null ? (double?)a["MainBet"]! : null,
						MainBetHours = a["MainBetHours"] != null ? (int?)a["MainBetHours"]! : null,
						ExcessiveBetHours = a["ExcessiveBetHours"] != null ? (double?)a["ExcessiveBetHours"]! : null,
						ExcessibeBet = a["ExcessibeBet"] != null ? (double?)a["ExcessibeBet"]! : null,
						ShortFullName = a["ShortFullName"] != null ? (string)a["ShortFullName"]! : ""
					}).ToList();

					return (message, rows);
				}
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<PPSParsedRow>());
			}
		}

		public readonly record struct PPSParsedRow
		{
			public double? MainBet { get; init; }
			public int? MainBetHours { get; init; }
			public double? ExcessiveBetHours { get; init; }
			public double? ExcessibeBet { get; init; }
			public string ShortFullName { get; init; }
		}

		public async Task<(string Message, List<FileRecord> Files)> Get()
		{
			try
			{
				var response = await StaticHttpClient.Get<JsonObject>(className, "get", new List<KeyValuePair<string, string>> 
				{ 
					new("count", "0"),
					new("startIndex", "0")
				});
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				if (response.Response == null)
					throw new Exception("Response is null");

				var files = response.Response["files"]!.AsArray().Select(a => new FileRecord
				{
					FileID = (Guid)a["FileID"]!,
					CreationDate = (DateTime)a["CreationDate"]!,
					Path = (string)a["Path"]!,
					FileType = (string)a["FileType"]!
				}).ToList();

				return ("", files);
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<FileRecord>());
			}
		}

		public async Task<(string Message, Stream? Stream)> DownloadFile(Guid fileID)
		{
			try
			{
				var response = await StaticHttpClient.GetStream(className, "download", new List<KeyValuePair<string, string>> 
				{ 
					new("fileID", fileID.ToString())
				});
				if (!response.IsSuccess)
					throw new Exception(response.Message);

				return ("", response.Response);
			}
			catch (Exception ex)
			{
				return (ex.Message, null);
			}
		}
    }
}
