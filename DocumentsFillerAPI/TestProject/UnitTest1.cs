using DocumentsFillerAPI.ExcelHelper;
using DocumentsFillerAPI.ExcelWorker;
using NPOI.XSSF.UserModel;
using System.Diagnostics;
namespace TestProject
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void Test1()
		{
			string path = string.Join("\\", Directory.GetParent("./").FullName.Split("\\").SkipLast(3)) + "\\PPSTest.xlsm";

			using (FileStream fs = new FileStream(path, FileMode.Open))
			{
				var result = new ExcelFilesParser().ParsePPSExcelFile(fs);
			}

				Assert.Pass();
		}

		[Test]
		public void Test2()
		{
			var testInfo = new ExcelFilesGenerator.StaffingTemplateInputData()
			{
				FirstAcademicYear = 2024,
				SecondAcademicYear = 2025,
				DepartmentName = "»“—",
				MainStaff = new(),
				InternalStaff = new(),
				ExternalStaff = new(),
				ProtocolNumber = 1,
				ProtocolDate = DateTime.Now,
				HeadDepartment = "À.Õ. ÷˚Ï·‡Î˛Í"
			};

			var workBook = new ExcelFilesGenerator().GenerateStaffingTemplate(testInfo);
			File.Delete("./testStaffingTemplate.xlsx");
			using (FileStream fs = new FileStream("./testStaffingTemplate.xlsx", FileMode.Create))
			{
				workBook.Write(fs);
				//fs.Flush();
				fs.Close();
			}
		}

		[Test]
		public void Test3()
		{
			var testInfo = new ExcelFilesGenerator.ServiceMemoInputData()
			{
				FirstAcademicYear = 2024,
				SecondAcademicYear = 2025,
				ProtocolDateTime = DateTime.Now,
				ProtocolNumber = 1,
				StudyPeriodDateEnd = new DateTime(2025, 01, 01),
				StudyPeriodDateStart = new DateTime(2024, 01, 01),
				MainStaff = new(),
				InternalStaff = new(),
				ExternalStaff = new(),
				HourlyWorkers = new()
			};

			var workBook = new ExcelFilesGenerator().GenerateServiceMemo(testInfo);
			File.Delete("./serviceMemoTemplate.xlsx");
			using (FileStream fs = new FileStream("./serviceMemoTemplate.xlsx", FileMode.Create))
			{
				workBook.Write(fs);
				fs.Close();
			}
		}
	}
}