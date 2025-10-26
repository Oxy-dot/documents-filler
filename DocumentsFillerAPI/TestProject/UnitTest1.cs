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
				DepartmentName = "хря",
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
	}
}