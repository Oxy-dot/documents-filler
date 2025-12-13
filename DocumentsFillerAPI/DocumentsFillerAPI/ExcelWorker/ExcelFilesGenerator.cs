using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

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
			var firstHeaderStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Default, offBorder: true, textWrap: false, horizontalAligment: HorizontalAlignment.Left, verticalAligment: VerticalAlignment.None);
			var tableHeaderStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small);
			var tableColumnsBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Default, horizontalAligment: HorizontalAlignment.Left, textWrap: false, verticalAligment: VerticalAlignment.Bottom);
			var tableColumnsNotBoldStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, horizontalAligment: HorizontalAlignment.Right, verticalAligment: VerticalAlignment.Bottom);
			var tableColumnsBoldNotWrapStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, textWrap: false, horizontalAligment: HorizontalAlignment.Center);
			var footerStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, textWrap: false, horizontalAligment: HorizontalAlignment.Left, verticalAligment: VerticalAlignment.Bottom);

			var numericFormat = xssfWorkbook.CreateDataFormat().GetFormat("0.00");
			tableColumnsNotBoldStyle.DataFormat = numericFormat;
			var sheet = xssfWorkbook.CreateSheet();

			sheet.SetColumnWidth(0, 5266);
			sheet.SetColumnWidth(1, 2816);
			sheet.SetColumnWidth(2, 2816);
			sheet.SetColumnWidth(3, 2816);
			sheet.SetColumnWidth(4, 2816);
			sheet.SetColumnWidth(5, 2816);
			sheet.SetColumnWidth(6, 2816);

			// Добавляем объединенные области как в шаблоне
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(2, 2, 1, 6));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 5, 0, 0));
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(4, 4, 1, 6));

			// Строка 0 - пустая строка с высотой
			var row0 = sheet.CreateRow(0);
			row0.Height = 675; // Высота в единицах 1/20 точки

			// Строка 1 - заголовок "Штатное расписание ППС кафедры" и "на год уч.год"
			var row1 = sheet.CreateRow(1);
			row1.CreateCell(1).SetCellValue("Штатное расписание ППС кафедры").SetStyle(firstHeaderStyle);
			row1.CreateCell(5).SetCellValue($"на {inputData.FirstAcademicYear}/{inputData.SecondAcademicYear} уч.год").SetStyle(firstHeaderStyle);

			// Строка 2 - объединенная ячейка с названием кафедры
			var row2 = sheet.CreateRow(2);
			row2.CreateCell(1).SetCellValue(inputData.DepartmentName).SetStyle(firstHeaderStyle);

			// Строка 3 - пустая строка с высотой
			var row3 = sheet.CreateRow(3);
			row3.Height = 1185; // Высота в единицах 1/20 точки

			// Строка 4 - заголовки таблицы: "ФИО" и "Должность"
			var row4 = sheet.CreateRow(4);
			row4.CreateCell(0, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("ФИО");
			row4.CreateCell(1, CellType.String).SetStyle(tableHeaderStyle).SetCellValue("Должность");
			row4.CreateCell(2, CellType.String).SetStyle(tableHeaderStyle);
			row4.CreateCell(3, CellType.String).SetStyle(tableHeaderStyle);
			row4.CreateCell(4, CellType.String).SetStyle(tableHeaderStyle);
			row4.CreateCell(5, CellType.String).SetStyle(tableHeaderStyle);
			row4.CreateCell(6, CellType.String).SetStyle(tableHeaderStyle);

			// Строка 5 - названия должностей
			var row5 = sheet.CreateRow(5);
			row5.CreateCell(0).SetStyle(tableHeaderStyle);
			row5.CreateCell(1, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[0]);
			row5.CreateCell(2, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[1]);
			row5.CreateCell(3, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[2]);
			row5.CreateCell(4, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[3]);
			row5.CreateCell(5, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[4]);
			row5.CreateCell(6, CellType.String).SetStyle(tableHeaderStyle).SetCellValue(academicTitles[5]);

			// Строка 6 - "1. Основной штат"
			var row6 = sheet.CreateRow(6);
			row6.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("1. Основной штат:");
			row6.CreateCell(1, CellType.String).SetStyle(tableColumnsBoldStyle);
			row6.CreateCell(2, CellType.String).SetStyle(tableColumnsBoldStyle);
			row6.CreateCell(3, CellType.String).SetStyle(tableColumnsBoldStyle);
			row6.CreateCell(4, CellType.String).SetStyle(tableColumnsBoldStyle);
			row6.CreateCell(5, CellType.String).SetStyle(tableColumnsBoldStyle);
			row6.CreateCell(6, CellType.String).SetStyle(tableColumnsBoldStyle);

			// Заполняем основной штат
			int rowNumber = 7;
			foreach (var mainStaffRow in inputData.MainStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, mainStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			// Заголовок "2. Внутренние совместители:"
			var internalStaffFirstRow = sheet.CreateRow(rowNumber);
			internalStaffFirstRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("2. Внутренние совместители:");
			internalStaffFirstRow.CreateCell(1, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(2, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(3, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(4, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(5, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			internalStaffFirstRow.CreateCell(6, CellType.Blank).SetStyle(tableColumnsBoldStyle);

			rowNumber++;
			foreach (var internalStaffRow in inputData.InternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, internalStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			// Заголовок "3. Внешние совместители:"
			var externalStaffFirstRow = sheet.CreateRow(rowNumber);
			externalStaffFirstRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("3. Внешние совместители");
			externalStaffFirstRow.CreateCell(1, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(2, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(3, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(4, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(5, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffFirstRow.CreateCell(6, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			
			// Строка с "(представители работодателей):"
			rowNumber++;
			var externalStaffSecondRow = sheet.CreateRow(rowNumber);
			externalStaffSecondRow.CreateCell(0, CellType.String).SetStyle(tableColumnsBoldStyle).SetCellValue("(представители работодателей):");
			externalStaffSecondRow.CreateCell(1, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(2, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(3, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(4, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(5, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			externalStaffSecondRow.CreateCell(6, CellType.Blank).SetStyle(tableColumnsBoldStyle);
			
			rowNumber++;
			foreach (var externalStaffRow in inputData.ExternalStaff)
			{
				var currentRow = sheet.CreateRow(rowNumber);
				FillStaffForStaffingTemplate(currentRow, externalStaffRow, tableColumnsNotBoldStyle);
				rowNumber++;
			}

			var resultsRow = sheet.CreateRow(rowNumber);
			resultsRow.Height = 402;
			resultsRow.CreateCell(0, CellType.String).SetCellValue("ИТОГО:").SetStyle(tableColumnsBoldStyle);
			resultsRow.CreateCell(1, CellType.Formula).SetCellFormula($"SUM(B8:B{rowNumber})").SetStyle(tableColumnsNotBoldStyle);
			resultsRow.CreateCell(2, CellType.Formula).SetCellFormula($"SUM(C8:C{rowNumber})").SetStyle(tableColumnsNotBoldStyle);
			resultsRow.CreateCell(3, CellType.Formula).SetCellFormula($"SUM(D8:D{rowNumber})").SetStyle(tableColumnsNotBoldStyle);
			resultsRow.CreateCell(4, CellType.Formula).SetCellFormula($"SUM(E8:E{rowNumber})").SetStyle(tableColumnsNotBoldStyle);
			resultsRow.CreateCell(5, CellType.Formula).SetCellFormula($"SUM(F8:F{rowNumber})").SetStyle(tableColumnsNotBoldStyle);
			resultsRow.CreateCell(6, CellType.Formula).SetCellFormula($"SUM(G8:G{rowNumber})").SetStyle(tableColumnsNotBoldStyle);

			rowNumber++;
			rowNumber++;
			sheet.CreateRow(rowNumber).CreateCell(3, CellType.String).SetCellValue("Принято на заседании кафедры").SetStyle(footerStyle);
			rowNumber++;
			sheet.CreateRow(rowNumber).CreateCell(3, CellType.String).SetCellValue($"Протокол №{inputData.ProtocolNumber}  от «{inputData.ProtocolDate.Day}» {inputData.ProtocolDate.GetDeclinationsOfMonth()} {inputData.ProtocolDate.Year} г.").SetStyle(footerStyle);

			rowNumber++;
			sheet.CreateRow(rowNumber).Height = 540;

			rowNumber++;
			var secondFooterRow = sheet.CreateRow(rowNumber);
			secondFooterRow.CreateCell(0, CellType.String).SetCellValue("и.о. зав.кафедрой").SetStyle(footerStyle);
			secondFooterRow.CreateCell(3, CellType.String).SetCellValue(inputData.HeadDepartment).SetStyle(footerStyle);
			
			rowNumber++;
			sheet.CreateRow(rowNumber).Height = 180;

			rowNumber++;
			var thirdRowFooterRow = sheet.CreateRow(rowNumber);
			thirdRowFooterRow.CreateCell(0, CellType.String).SetCellValue("СОГЛАСОВАНО:").SetStyle(footerStyle);

			rowNumber++;
			var fourthRowFooterRow = sheet.CreateRow(rowNumber);
			fourthRowFooterRow.CreateCell(0, CellType.String).SetCellValue("Директор:").SetStyle(footerStyle);
			fourthRowFooterRow.CreateCell(3, CellType.String).SetCellValue("В.А. Шульцев").SetStyle(footerStyle);

			rowNumber+=2;
			var fifthRowFooterRow = sheet.CreateRow(rowNumber);
			fifthRowFooterRow.CreateCell(0, CellType.String).SetCellValue("Зам.начальника УОП:").SetStyle(footerStyle);
			fifthRowFooterRow.CreateCell(3, CellType.String).SetCellValue("С.В. Фокеева").SetStyle(footerStyle);

			rowNumber++;
			var sixthRowFooterRow = sheet.CreateRow(rowNumber);
			sixthRowFooterRow.Height = 525;
			sixthRowFooterRow.CreateCell(0, CellType.String).SetCellValue("\"_____\"_____________ 2025 г.").SetStyle(footerStyle);

			return xssfWorkbook;
		}
		private void FillStaffForStaffingTemplate(IRow row, StaffingTemplateRow data, ICellStyle tableColumnsNotBoldStyle)
		{
			row.CreateCell(0, CellType.String).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.FullName);
			row.CreateCell(1, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(2, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(3, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(4, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(5, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(6, CellType.Numeric).SetStyle(tableColumnsNotBoldStyle);
			//Добавить ещё нолик в конце каждого числа епта
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
			/// <summary>
			/// Зав кафедры
			/// </summary>
			public string HeadDepartment { get; init; }
		}

		public record StaffingTemplateRow
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public double Bet { get; set; }
		}
		#endregion
		#region ServiceMemoTemplate
		public XSSFWorkbook GenerateServiceMemo(ServiceMemoInputData inputData)
		{
			XSSFWorkbook xssfWorkbook = new XSSFWorkbook();
			var headerStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, textWrap: false, horizontalAligment: HorizontalAlignment.Left);
			var headerBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Default, offBorder: true);
			var textStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Distributed, verticalAligment: VerticalAlignment.Justify);
			var textStyle2 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Justify, verticalAligment: VerticalAlignment.Center);
			var tableHeaderStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center, verticalAligment: VerticalAlignment.Center);
			var tableHeaderVerySmallStyle1 = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.VerySmall, horizontalAligment: HorizontalAlignment.Center, verticalAligment: VerticalAlignment.Center);
			var tableHeaderBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center, verticalAligment: VerticalAlignment.Justify);
			var tableHeaderSmallBoldStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.VerySmall, horizontalAligment: HorizontalAlignment.Center, verticalAligment: VerticalAlignment.Center);
			var tableHeaderDefaultStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, horizontalAligment: HorizontalAlignment.Center, verticalAligment: VerticalAlignment.Justify);
			var tableHeaderBoldFIOStyle = xssfWorkbook.GenerateDefaultStyle(true, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Left, verticalAligment: VerticalAlignment.Justify);
			var valuesStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, textWrap: false, horizontalAligment: HorizontalAlignment.Left);
			valuesStyle.DataFormat = xssfWorkbook.CreateDataFormat().GetFormat("0.00");

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
				FillStaffForServiceMemo(currentRow, staff, valuesStyle);
				rowNumber++;
			}

			var thirteenthRow = sheet.CreateRow(rowNumber);
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
				FillStaffForServiceMemo(currentRow, staff, valuesStyle);
				rowNumber++;
			}

			var fourteenthRow = sheet.CreateRow(rowNumber);
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
				FillStaffForServiceMemo(currentRow, staff, valuesStyle);
				rowNumber++;
			}

			var fifteenthRow = sheet.CreateRow(rowNumber);
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
				FillStaffForServiceMemo(currentRow, staff, valuesStyle);
				rowNumber++;
			}

			//Используется для формул
			int lastRowWithData = rowNumber;

			var sixteenthRow = sheet.CreateRow(rowNumber);
			sixteenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(1).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(5).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(6).SetStyle(tableHeaderBoldStyle);
			sixteenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);
			//fifteenthRow.CreateCell(7).SetStyle(tableHeaderBoldStyle);

			var resultFirstStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, horizontalAligment: HorizontalAlignment.Center);
			var resultSecondStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Center);
			var resultThirdStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, horizontalAligment: HorizontalAlignment.Left);

			var seventeenthRow = sheet.CreateRow(++rowNumber);
			seventeenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(1).SetStyle(tableHeaderBoldFIOStyle).SetCellValue("Резерв");
			seventeenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(3).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(4).SetStyle(tableHeaderBoldStyle);
			seventeenthRow.CreateCell(5).SetStyle(resultFirstStyle).SetCellFormula($"G{rowNumber}*750");
			seventeenthRow.CreateCell(6).SetStyle(resultFirstStyle).SetCellFormula($"{inputData.Reserve.ToString(CultureInfo.InvariantCulture).Replace(",", ".")}-SUM(G14:G{lastRowWithData})");
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

			resultSecondStyle.DataFormat = xssfWorkbook.CreateDataFormat().GetFormat("0.00");

			var ninteenthRow = sheet.CreateRow(++rowNumber);
			ninteenthRow.CreateCell(0).SetStyle(tableHeaderBoldStyle);
			ninteenthRow.CreateCell(1).SetStyle(resultThirdStyle).SetCellValue("ИТОГО:");
			ninteenthRow.CreateCell(2).SetStyle(tableHeaderBoldStyle);
			ninteenthRow.CreateCell(3).SetStyle(resultFirstStyle).SetCellFormula($"SUM(D14:D{lastRowWithData})");
			ninteenthRow.CreateCell(4).SetStyle(resultFirstStyle).SetCellFormula($"SUM(E14:E{lastRowWithData})");
			ninteenthRow.CreateCell(5).SetStyle(resultFirstStyle).SetCellFormula($"SUM(F14:F{lastRowWithData})");
			ninteenthRow.CreateCell(6).SetStyle(resultFirstStyle).SetCellFormula($"SUM(G14:G{lastRowWithData})");
			ninteenthRow.CreateCell(7).SetStyle(resultSecondStyle).SetCellFormula($"E{rowNumber+1}+G{rowNumber+1}");

			int dataEndRowNumber = rowNumber;

			//75
			//540
			sheet.CreateRow(++rowNumber).Height = 75;

			rowNumber++;
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowNumber, rowNumber, 0, 7));

			var twentiethRow = sheet.CreateRow(rowNumber);
			twentiethRow.Height = 540;
			var infoStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Small, offBorder: true, textWrap: true, horizontalAligment: HorizontalAlignment.Left, verticalAligment: VerticalAlignment.Center);
			twentiethRow.CreateCell(0).SetStyle(infoStyle).SetCellValue("* – столбец «Доплата» заполняется специалистом ФЭУ (в зависимости от указанной учебной сверхнормативной нагрузки и занимаемой должности)");

			rowNumber++;
			sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(rowNumber, rowNumber, 2, 7));

			var footerStyle = xssfWorkbook.GenerateDefaultStyle(false, Helper.FontHeight.Default, offBorder: true, textWrap: false, horizontalAligment: HorizontalAlignment.Left);

			var twentyFirst = sheet.CreateRow(rowNumber);
			twentyFirst.CreateCell(2).SetStyle(footerStyle).SetCellValue("Принято на заседании УП");

			var twentySecondRow = sheet.CreateRow(++rowNumber);
			twentySecondRow.Height = 465;
			twentySecondRow.CreateCell(2).SetStyle(footerStyle).SetCellValue($"Протокол №{inputData.ProtocolNumber} от «{inputData.ProtocolDateTime.Day}» {inputData.ProtocolDateTime.GetDeclinationsOfMonth()} {inputData.ProtocolDateTime.Year}г.");

			var twentyThirdRow = sheet.CreateRow(++rowNumber);
			twentyThirdRow.CreateCell(1).SetStyle(footerStyle).SetCellValue("Заведующий УП (ОП)");
			twentyThirdRow.CreateCell(6).SetStyle(footerStyle).SetCellValue("Л.Н. Цымбалюк");
			rowNumber++;
			sheet.CreateRow(++rowNumber).CreateCell(1).SetStyle(footerStyle).SetCellValue("Согласовано:");

			var twentyFourthRow = sheet.CreateRow(rowNumber += 2);
			twentyFourthRow.CreateCell(1).SetStyle(footerStyle).SetCellValue("Замначальника УОП");
			twentyFourthRow.CreateCell(6).SetStyle(footerStyle).SetCellValue("С.В. Фокеева");

			var twentyFifthRow = sheet.CreateRow(rowNumber += 2);
			twentyFifthRow.CreateCell(1).SetStyle(footerStyle).SetCellValue("Начальник ФЭУ");
			twentyFifthRow.CreateCell(6).SetStyle(footerStyle).SetCellValue("Е. Ю. Цветкова");

			//Заполняем всё белым

			int maxRows = sheet.PhysicalNumberOfRows + 30;
			int maxCols = 100;

			var whiteStyle = xssfWorkbook.CreateCellStyle();
			whiteStyle.FillForegroundColor = IndexedColors.White.Index;
			whiteStyle.FillPattern = FillPattern.SolidForeground;

			for (int rowIndex = 0; rowIndex < maxRows; rowIndex++)
			{
				IRow row = sheet.GetRow(rowIndex);
				if (row == null)
					row = sheet.CreateRow(rowIndex);

				if (rowIndex <= dataEndRowNumber)
					for (int colIndex = 8; colIndex < maxCols; colIndex++)
						row.CreateCell(colIndex).CellStyle = whiteStyle;
				else
					for (int colIndex = 0; colIndex < maxCols; colIndex++)
					{
						var cell = row.GetCell(colIndex);
						if (cell == null)
							row.CreateCell(colIndex).CellStyle = whiteStyle;
						else
						{
							cell.CellStyle.FillForegroundColor = IndexedColors.White.Index;
							cell.CellStyle.FillPattern = FillPattern.SolidForeground;
						}
					}
			}
			return xssfWorkbook;
		}

		private void FillStaffForServiceMemo(IRow row, ServiceMemoTemplateRow data, ICellStyle tableColumnsNotBoldStyle)
		{
			row.CreateCell(0).SetStyle(tableColumnsNotBoldStyle);
			row.CreateCell(1).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.FullName);
			row.CreateCell(2).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.AcademicTitle);

			if (data.MainBetInfo.HasValue)
			{
				row.CreateCell(3).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.MainBetInfo.Value.HoursAmount);
				row.CreateCell(4).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.MainBetInfo.Value.Bet);
			}
			else
			{
				row.CreateCell(3).SetStyle(tableColumnsNotBoldStyle);
				row.CreateCell(4).SetStyle(tableColumnsNotBoldStyle);
			}


			if (data.ExcessiveBetInfo.HasValue)
			{
				row.CreateCell(5).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.ExcessiveBetInfo.Value.HoursAmount);
				row.CreateCell(6).SetStyle(tableColumnsNotBoldStyle).SetCellValue(data.ExcessiveBetInfo.Value.Bet);
			}
			else
			{
				row.CreateCell(5).SetStyle(tableColumnsNotBoldStyle);
				row.CreateCell(6).SetStyle(tableColumnsNotBoldStyle);
			}
			row.CreateCell(7).SetStyle(tableColumnsNotBoldStyle);
		}

		public readonly record struct ServiceMemoInputData
		{
			public string DepartmentName { get; init; }
			public double Reserve { get; init; }
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

		public record ServiceMemoTemplateRow 
		{
			public string FullName { get; init; }
			public string AcademicTitle { get; init; }
			public ServiceMemoTemplateBetStruct? MainBetInfo { get; set; }
			public ServiceMemoTemplateBetStruct? ExcessiveBetInfo { get; set; }
		}

		public readonly record struct ServiceMemoTemplateBetStruct
		{
			public int HoursAmount { get; init; }
			public double Bet { get; init; }
		}

		#endregion
	}
}