using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using DocumentsFillerAPI.ExcelHelper;

namespace DocumentsFillerAPI.ExcelWorker
{
	public class ExcelFilesGenerator
	{
		private readonly string[] academicTitles = { "зав.каф.", "профессор", "доцент", "ст.препод.", "ассистент", "преподаватель" };
		#region StaffingTemplate
		public XSSFWorkbook GenerateStaffingTemplate(StaffingTemplateInputData inputData)
		{
			XSSFWorkbook xssfWorkbook = new XSSFWorkbook();
			var headerStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, true);
			var tableHeaderStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Default);
			var tableColumnsBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center);
			var tableColumnsNotBoldStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small);
			var tableColumnsBoldNotWrapStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, textWrap: false, horizontalAligment: HorizontalAlignment.Center);

			var sheet = xssfWorkbook.CreateSheet();
			sheet.AutoSizeColumn(0);

			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 2, 0, 6));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 5, 0, 0));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 4, 1, 6));

			//Создаем первую строку
			sheet.CreateRow(0).CreateCell(0, CellType.String).SetStyle(headerStyle).SetCellValue($"Штатное расписание ППС кафедры {inputData.DepartmentName}              за {inputData.FirstAcademicYear}/{inputData.SecondAcademicYear} уч. год");

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
			thirdRow.CreateCell(1, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[0]);
			thirdRow.CreateCell(2, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[1]);
			thirdRow.CreateCell(3, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[2]);
			thirdRow.CreateCell(4, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[3]);
			thirdRow.CreateCell(5, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[4]);
			thirdRow.CreateCell(6, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[5]);

			sheet.SetColumnWidth(0, 33.7 * 256);
			sheet.SetColumnWidth(1, 9.43 * 256);
			sheet.SetColumnWidth(2, 9.43 * 256);
			sheet.SetColumnWidth(3, 9.43 * 256);
			sheet.SetColumnWidth(4, 9.43 * 256);
			sheet.SetColumnWidth(5, 9.43 * 256);
			sheet.SetColumnWidth(6, 9.43 * 256);

			var mainStafFirstRow = sheet.CreateRow(6);
			mainStafFirstRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("1. Основной штат");
			mainStafFirstRow.CreateCell(1, CellType.String).SetStyle(tableColumnsBoldStyle);
			mainStafFirstRow.CreateCell(2, CellType.String).SetStyle(tableColumnsBoldStyle);
			mainStafFirstRow.CreateCell(3, CellType.String).SetStyle(tableColumnsBoldStyle);
			mainStafFirstRow.CreateCell(4, CellType.String).SetStyle(tableColumnsBoldStyle);
			mainStafFirstRow.CreateCell(5, CellType.String).SetStyle(tableColumnsBoldStyle);
			mainStafFirstRow.CreateCell(6, CellType.String).SetStyle(tableColumnsBoldStyle);

			int rowNumber = 7;
			foreach (var mainStaffRow in inputData.MainStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, mainStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			var internalStaffFirstRow = sheet.CreateRow(++rowNumber);
			internalStaffFirstRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("2. Внутренние совместители:");
			internalStaffFirstRow.CreateCell(1, CellType.String).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(2, CellType.String).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(3, CellType.String).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(4, CellType.String).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(5, CellType.String).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(6, CellType.String).SetStyle(tableColumnsBoldStyle);

			rowNumber++;
			foreach (var internalStaffRow in inputData.InternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, internalStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			var externalStaffFirstRow = sheet.CreateRow(++rowNumber);
			externalStaffFirstRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("3. Внешние совместители:");
			externalStaffFirstRow.CreateCell(1, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(2, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(3, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(4, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(5, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(6, CellType.String).SetStyle(tableColumnsBoldStyle);
			var externalStaffSecondRow = sheet.CreateRow(++rowNumber);
			externalStaffSecondRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldNotWrapStyle).SetCellValue("(представители работодателей):");
			externalStaffSecondRow.CreateCell(1, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(2, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(3, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(4, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(5, CellType.String).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(6, CellType.String).SetStyle(tableColumnsBoldStyle);
			rowNumber++;
			foreach (var externalStaffRow in inputData.ExternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, externalStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			return xssfWorkbook;
		}

		private void FillStaffForStaffingTemplate(IRow row, StaffingTemplateRow data, ICellStyle tableColumnsNotBoldStyle)
		{
			row.CreateCell(0, CellType.String).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(1, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(2, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(3, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(4, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(5, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(6, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);

			for (int i = 0; i < academicTitles.Length; i++)
			{
				if (data.AcademicTitle == academicTitles[i])
					row.First(a => a.ColumnIndex == i + 1).SetCellValue(data.Bet);
			}
		}

		public readonly record struct StaffingTemplateInputData
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
		#endregion
		#region ServiceMemoTemplate
		public XSSFWorkbook GenerateServiceMemo(ServiceMemoInputData inputData)
		{
			XSSFWorkbook xssfWorkbook = new XSSFWorkbook();
			var headerStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, textWrap: false, horizontalAligment: HorizontalAlignment.Left);
			var headerBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Default, offBorder: true);
			var textStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Distributed, isCenterVerticalAlignment: false);
			var textStyle2 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Justify, isCenterVerticalAlignment: true);
			var tableHeaderStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var tableHeaderVerySmallStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.VerySmall, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var tableHeaderBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var tableHeaderSmallBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.VerySmall, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var tableHeaderDefaultStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var tableHeaderBoldFIOStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Left, isCenterVerticalAlignment: true);

			var sheet = xssfWorkbook.CreateSheet();
			sheet.AutoSizeColumn(0);

			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 4, 1, 7));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(6, 6, 0, 7));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(7, 7, 0, 7));

			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(9, 11, 0, 0));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(9, 11, 1, 1));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(9, 11, 2, 2));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(9, 9, 3, 7));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(10, 10, 3, 4));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(10, 10, 5, 7));

			sheet.SetColumnWidth(0, 3.2 * 256);
			sheet.SetColumnWidth(1, 27 * 256);
			sheet.SetColumnWidth(2, 12 * 256);
			sheet.SetColumnWidth(3, 9.2 * 256);
			sheet.SetColumnWidth(4, 9.2 * 256);
			sheet.SetColumnWidth(5, 9.2 * 256);
			sheet.SetColumnWidth(6, 6.2 * 256);
			sheet.SetColumnWidth(7, 9.2 * 256);

			sheet.CreateRow(0).CreateCell(6, CellType.String).SetStyle(headerStyle).SetCellValue("Проректору по ОД");
			sheet.CreateRow(1).CreateCell(6, CellType.String).SetStyle(headerStyle).SetCellValue("Данейкину Ю.В.");

			sheet.CreateRow(4).CreateCell(1, CellType.String).SetStyle(tableHeaderDefaultStyle).SetCellValue("служебная записка.");
			var richText = new XSSFRichTextString($"   1. Довожу до Вашего сведения распределение учебной нормативной и сверхнормативной нагрузки по преподавателям учебного подразделения кафедра информационных технологий и систем на {inputData.FirstAcademicYear}/{inputData.SecondAcademicYear} уч.г.");
			#region font generate
			var font = xssfWorkbook.CreateFont();
			font.IsBold = true;
			font.IsItalic = true;
			font.FontHeightInPoints = 12;
			font.FontName = "Times New Roman";
			richText.ApplyFont(178, richText.Length - 1, font);
			#endregion
			var sixRow = sheet.CreateRow(6);
			sixRow.HeightInPoints = 47.5f;
			sixRow.CreateCell(0).SetStyle(textStyle1).SetCellValue(richText);

			var richText1 = new XSSFRichTextString($"    2. Прошу установить (изменить) доплаты за сверхнормативную учебную нагрузку из ФОТ УП с {inputData.StudyPeriodDateStart:dd.MM.yyyy} по {inputData.StudyPeriodDateEnd:dd.MM.yyyy} ежемесячно следующим преподавателям:");
			#region font generate
			var font1 = xssfWorkbook.CreateFont();
			font1.IsItalic = true;
			font1.FontHeightInPoints = 12;
			font1.FontName = "Times New Roman";

			richText1.ApplyFont(87, 89, font1);
			richText1.ApplyFont(90, richText1.Length - 1, textStyle1.GetFont(xssfWorkbook));
			richText1.ApplyFont(36, 87, textStyle1.GetFont(xssfWorkbook));
			
			font.IsItalic = false;
			richText1.ApplyFont(14, 34, font);
			#endregion
			var seventhRow = sheet.CreateRow(7);
			seventhRow.HeightInPoints = 46.5f;
			seventhRow.CreateCell(0).SetStyle(textStyle2).SetCellValue(richText1);

			var ninthRow = sheet.CreateRow(9);
			ninthRow.HeightInPoints = 14.25f;
			ninthRow.CreateCell(0).SetStyle(tableHeaderStyle1).SetCellValue("№ п/п");
			ninthRow.CreateCell(1).SetStyle(tableHeaderStyle1).SetCellValue("ФИО");
			ninthRow.CreateCell(2).SetStyle(tableHeaderVerySmallStyle1).SetCellValue("Должность");
			ninthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle).SetCellValue("Учебная нагрузка");
			ninthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			ninthRow.CreateCell(5).SetStyle(tableHeaderBoldStyle);
			ninthRow.CreateCell(6).SetStyle(tableHeaderBoldStyle);
			ninthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);

			var tenthRow = sheet.CreateRow(10);
			tenthRow.HeightInPoints = 33f;
			tenthRow.CreateCell(0).SetStyle(tableHeaderSmallBoldStyle);
			tenthRow.CreateCell(1).SetStyle(tableHeaderSmallBoldStyle);
			tenthRow.CreateCell(2).SetStyle(tableHeaderSmallBoldStyle);
			tenthRow.CreateCell(3).SetStyle(tableHeaderSmallBoldStyle).SetCellValue($"нормативная установленная с {inputData.StudyPeriodDateStart:dd.MM.yyyy}");
			tenthRow.CreateCell(4).SetStyle(tableHeaderSmallBoldStyle);
			tenthRow.CreateCell(5).SetStyle(tableHeaderSmallBoldStyle).SetCellValue("Сверхнормативная (из ФОТ)");
			tenthRow.CreateCell(6).SetStyle(tableHeaderSmallBoldStyle);
			tenthRow.CreateCell(7).SetStyle(tableHeaderSmallBoldStyle);

			var eleventhRow = sheet.CreateRow(11);
			eleventhRow.HeightInPoints = 38.25f;
			eleventhRow.CreateCell(0).SetStyle(tableHeaderStyle1);
			eleventhRow.CreateCell(1).SetStyle(tableHeaderStyle1);
			eleventhRow.CreateCell(2).SetStyle(tableHeaderStyle1);
			eleventhRow.CreateCell(3).SetStyle(tableHeaderStyle1).SetCellValue("часы");
			eleventhRow.CreateCell(4).SetStyle(tableHeaderStyle1).SetCellValue("доля ставки, ед.");
			eleventhRow.CreateCell(5).SetStyle(tableHeaderStyle1).SetCellValue("часы");
			eleventhRow.CreateCell(6).SetStyle(tableHeaderStyle1).SetCellValue("доля ставки, ед.");
			eleventhRow.CreateCell(7).SetStyle(tableHeaderStyle1).SetCellValue("Доплата, руб.*");

			var twelfthRow = sheet.CreateRow(12);
			twelfthRow.CreateCell(0).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("1. Основной штат:");
			twelfthRow.CreateCell(2).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(3).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(4).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(5).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(6).SetStyle(tableHeaderBoldFIOStyle);
			twelfthRow.CreateCell(7).SetStyle(tableHeaderBoldFIOStyle);

			int rowNumber = 13;
			foreach (var staff in inputData.MainStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForServiceMemo(currentRow, staff, headerStyle);
				rowNumber++;
			}

			var thirteenthRow = sheet.CreateRow(++rowNumber);
			thirteenthRow.CreateCell(0).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("2. Внутренние совместители:");
			thirteenthRow.CreateCell(2).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(3).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(4).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(5).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(6).SetStyle(tableHeaderBoldFIOStyle);
			thirteenthRow.CreateCell(7).SetStyle(tableHeaderBoldFIOStyle);
			
			rowNumber++;
			//int rowNumber = 13;
			foreach (var staff in inputData.InternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForServiceMemo(currentRow, staff, headerStyle);
				rowNumber++;
			}

			var fourteenthRow = sheet.CreateRow(++rowNumber);
			fourteenthRow.CreateCell(0).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("3. Внешние совместители:");
			fourteenthRow.CreateCell(2).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(3).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(4).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(5).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(6).SetStyle(tableHeaderBoldFIOStyle);
			fourteenthRow.CreateCell(7).SetStyle(tableHeaderBoldFIOStyle);

			rowNumber++;
			//int rowNumber = 13;
			foreach (var staff in inputData.ExternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForServiceMemo(currentRow, staff, headerStyle);
				rowNumber++;
			}

			var fifteenthRow = sheet.CreateRow(++rowNumber);
			fifteenthRow.CreateCell(0).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("4. Почасовики:");
			fifteenthRow.CreateCell(2).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(3).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(4).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(5).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(6).SetStyle(tableHeaderBoldFIOStyle);
			fifteenthRow.CreateCell(7).SetStyle(tableHeaderBoldFIOStyle);

			rowNumber++;
			//int rowNumber = 13;
			foreach (var staff in inputData.HourlyWorkers)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForServiceMemo(currentRow, staff, headerStyle);
				rowNumber++;
			}

			//Используется для формул
			int lastRowWithData = rowNumber;

			var sixteenthRow = sheet.CreateRow(++rowNumber);
			sixteenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(1).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(5).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(6).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);
			//fifteenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);

			var resultFirstStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);
			var resultSecondStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center, isCenterVerticalAlignment: true);

			var seventeenthRow = sheet.CreateRow(++rowNumber);
			seventeenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("Резерв");
			seventeenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(5).SetStyle(resultFirstStyle).SetCellFormula($"G{rowNumber}*750");
			seventeenthRow.CreateCell(6).SetStyle(resultFirstStyle).SetCellFormula($"13.2-SUM(G14:G{lastRowWithData})");
			seventeenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);

			var eighteenthRow = sheet.CreateRow(++rowNumber);
			eighteenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(1).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(5).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(6).SetStyle(tableHeaderBoldStyle);
			eighteenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);

			var ninteenthRow = sheet.CreateRow(++rowNumber);
			ninteenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			ninteenthRow.CreateCell(1).SetStyle(resultSecondStyle).SetCellValue("ИТОГО:");
			ninteenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			ninteenthRow.CreateCell(3).SetStyle(resultFirstStyle).SetCellFormula($"SUM(D14:D{lastRowWithData})");
			ninteenthRow.CreateCell(4).SetStyle(resultFirstStyle).SetCellFormula($"SUM(E14:E{lastRowWithData})");
			ninteenthRow.CreateCell(5).SetStyle(resultFirstStyle).SetCellFormula($"SUM(F14:F{lastRowWithData})");
			ninteenthRow.CreateCell(6).SetStyle(resultFirstStyle).SetCellFormula($"SUM(G14:G{lastRowWithData})");
			ninteenthRow.CreateCell(7).SetStyle(resultSecondStyle).SetCellFormula($"E{lastRowWithData}+G{lastRowWithData}");
			
			

			//sixteenthRow.HeightInPoints = 3.75f;
			return xssfWorkbook;
		}

		private void FillStaffForServiceMemo(IRow row, ServiceMemoTemplateRow data, ICellStyle tableColumnsNotBoldStyle)
		{
			row.CreateCell(0).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(1).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.FullName);
			row.CreateCell(2).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.AcademicTitle);

			row.CreateCell(3).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.MainBetInfo.Value.HoursAmount.Value);
			row.CreateCell(4).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.MainBetInfo.Value.Bet.Value);

			row.CreateCell(5).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.ExcessiveBetInfo.Value.HoursAmount.Value);
			row.CreateCell(6).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.ExcessiveBetInfo.Value.Bet.Value);
		}

		public readonly record struct ServiceMemoInputData
		{
			public int FirstAcademicYear { get; init; }
			public int SecondAcademicYear { get; init; }
			public DateTime StudyPeriodDateStart { get; init; }
			public DateTime StudyPeriodDateEnd { get; init; }
			public List<ServiceMemoTemplateRow> MainStaff { get; init; }
			public List<ServiceMemoTemplateRow> InternalStaff { get; init; }
			public List<ServiceMemoTemplateRow> ExternalStaff { get; init; }
			public List<ServiceMemoTemplateRow> HourlyWorkers { get; init; }
			public int ProtocolNumber { get; init; }
			public DateTime ProtocolDateTime { get; init; }
		}

		public readonly record struct ServiceMemoTemplateRow 
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public ServiceMemoTemplateBetStruct? MainBetInfo { get; init; }
			public ServiceMemoTemplateBetStruct? AdditionalBetInfo { get; init; }
			public ServiceMemoTemplateBetStruct? ExcessiveBetInfo { get; init; }
		}

		public readonly record struct ServiceMemoTemplateBetStruct
		{
			public double? HoursAmount { get; init; }
			public double? Bet { get; init; }
		}

		#endregion
	}
}