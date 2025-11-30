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
				var jBodyRequest = new JsonObject
				{
					["staffingTableInfo"] = new JsonObject()
					{
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
						["internallStaff"] = new JsonArray(data.InternallStaff.Select(a => new JsonObject()
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
			public List<StaffingTemplateRowData> InternallStaff { get; init; }
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
    }
}
