using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using DocumentsFillerAPI.ExcelHelper;

namespace DocumentsFillerAPI.ExcelWorker
{
	public class ExcelFilesGenerator
	{
		public XSSFWorkbook GenerateStaffingTemplate(StaffingTemplateInputData inputData)
		{
			XSSFWorkbook xssfWorkbook = new XSSFWorkbook();
			var headerStyle = xssfWorkbook.GenerateDefaultStyle(true, false, true);
			var tableHeaderStyle = xssfWorkbook.GenerateDefaultStyle(true, true);
			var tableColumnsBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, false);
			var tableColumnsNotBoldStyle = xssfWorkbook.GenerateDefaultStyle(false, false);

			var sheet = xssfWorkbook.CreateSheet();
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 2, 0, 6));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 5, 0, 0));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 4, 1, 6));

			sheet.AutoSizeColumn(0);
			sheet.SetColumnWidth(0, 33.7 * 256);
			sheet.SetColumnWidth(1, 8.43 * 256);
			sheet.SetColumnWidth(2, 8.43 * 256);
			sheet.SetColumnWidth(3, 8.43 * 256);
			sheet.SetColumnWidth(4, 8.43 * 256);
			sheet.SetColumnWidth(5, 8.43 * 256);
			sheet.SetColumnWidth(6, 8.43 * 256);

			//Создаем первую строку
			var firstRow = sheet.CreateRow(0);
			var firstCell = firstRow.CreateCell(0, CellType.String).SetStyle(headerStyle).SetCellValue($"Штатное расписание ППС кафедры {inputData.DepartmentName}              за {inputData.FirstAcademicYear}/{inputData.SecondAcademicYear} уч. год");

			//TODO Спросить надо ли добавлять увеличивание кол-ва должностей

			//Теперь создаем таблицу
			var secondRow = sheet.CreateRow(4);
			var fioCell = secondRow.CreateCell(0, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("ФИО");
			var academicTitlesCell = secondRow.CreateCell(1, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("Должность");
			secondRow.CreateCell(2, CellType.String).SetStyle(tableHeaderStyle);
			secondRow.CreateCell(3, CellType.String).SetStyle(tableHeaderStyle);
			secondRow.CreateCell(4, CellType.String).SetStyle(tableHeaderStyle);
			secondRow.CreateCell(5, CellType.String).SetStyle(tableHeaderStyle);
			secondRow.CreateCell(6, CellType.String).SetStyle(tableHeaderStyle);

			var thirdRow = sheet.CreateRow(5);
			thirdRow.CreateCell(0).SetStyle(tableHeaderStyle);
			thirdRow.CreateCell(1, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("зав.каф.");
			thirdRow.CreateCell(2, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("профессор");
			thirdRow.CreateCell(3, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("доцент");
			thirdRow.CreateCell(4, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("ст.препод.");
			thirdRow.CreateCell(5, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("ассистент");
			thirdRow.CreateCell(6, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("преподаватель");


			return xssfWorkbook;
		}

		public record struct StaffingTemplateInputData
		{
			public string DepartmentName { get; init; }
			public List<StaffingTemplateRow> MainStaff { get; init; }
			public List<StaffingTemplateRow> InternalStaff { get; init; }
			public List<StaffingTemplateRow> ExternalStaff { get; init; }
			public int FirstAcademicYear { get; init; }
			public int SecondAcademicYear { get; init; }
			public int ProtocolNumber { get; init; }
			public DateTime ProtocolDate { get; init; }
		}

		public record struct StaffingTemplateRow
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public double Bet { get; init; }
		}
	}
}
