using NPOI;
using NPOI.XSSF.Streaming;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.OpenXmlFormats;
using DocumentsFillerAPI.ExcelWorker;

namespace DocumentsFillerAPI.ExcelHelper
{
	public class ExcelFilesParser
	{
		private readonly (string Name, bool IsAddress)[] COLUMNS_NAMES = new (string Name, bool IsAddress)[]
		{
			("A", true), //MainBet
			("доп", false), //AddBet
			("сверхнагрузкичасы", false), //ExcessiveHours
			("D", true), //FIO
		};

		public (string Message, List<PPSParsedRow>) ParsePPSExcelFile(FileStream fs)
		{
			try
			{
				using (fs)
				{
					XSSFWorkbook xssfWorkbook = new XSSFWorkbook(fs, true);
					var sheet = xssfWorkbook.GetSheet("ППС");

					//Получаем первую колонку, по ней получаем адреса нужных колонок
					var firstRowCells = sheet.GetRow(sheet.FirstRowNum).Cells;
					var allRows = sheet.GetNotEmptyRows(sheet.FirstRowNum + 1).ToList();

					var secondRowCells = allRows.First();

					int mainBetColumnIndex = secondRowCells.First().ColumnIndex; /*.FirstOrDefault(a => a.Address.ToString() == (COLUMNS_NAMES[0].Name + sheet.FirstRowNum+1))?.ColumnIndex;*/
					int? additionalBetColumnIndex = firstRowCells.FirstOrDefault(a => a.StringCellValue.CustomTrim() == COLUMNS_NAMES[1].Name)?.ColumnIndex;
					int? excessiveHoursColumnIndex = firstRowCells.FirstOrDefault(a => a.CellType == CellType.String && a.StringCellValue.CustomTrim() == COLUMNS_NAMES[2].Name)?.ColumnIndex; /*additionalBetColumnIndex.HasValue ? additionalBetColumnIndex.Value + 1 : mainBetColumnIndex + 2; *//*firstRowCells.FirstOrDefault(a => a.StringCellValue == COLUMNS_NAMES[2].Name)?.ColumnIndex;*/
					int fullNameColumnIndex = additionalBetColumnIndex.HasValue ? additionalBetColumnIndex.Value + 2 : mainBetColumnIndex + 3;

					if (additionalBetColumnIndex == null || excessiveHoursColumnIndex == null)
						return ($"Cant find all needed column in file: MainBetColumnIndex={mainBetColumnIndex}, AdditionalBetColumnIndex={additionalBetColumnIndex}, ExcessiveHoursColumnIndex={excessiveHoursColumnIndex}, FullNameColumnIndex={fullNameColumnIndex}", new List<PPSParsedRow>());

					List<string> allErrors = new List<string>();
					List<PPSParsedRow> rows = new List<PPSParsedRow>();

					allRows.ForEach(row =>
					{
						if (row.Cells.Count == 0)
							return;

						var mainBetCell = row.Cells.FirstOrDefault(a => a.ColumnIndex == mainBetColumnIndex); /*.NumericCellValue;*/
						var additionalBetCell = row.Cells.FirstOrDefault(a => a.ColumnIndex == additionalBetColumnIndex);
						var excessiveHoursCell = row.Cells.FirstOrDefault(a => a.ColumnIndex == excessiveHoursColumnIndex); /*currentRow.GetCell().CellType == NPOI.SS.UserModel.CellType.Blank ? null : currentRow.GetCell(2).NumericCellValue;*/
						var fullNameCell = row.Cells.FirstOrDefault(a => a.ColumnIndex == fullNameColumnIndex);

						if (excessiveHoursCell == null || excessiveHoursCell.CellType == CellType.Blank || fullNameCell == null || fullNameCell.CellType == CellType.Blank)
						{
							allErrors.Add($"Cant find main bet or add bet or fullName on row = {row.RowNum+1}");
							return;
						}

						rows.Add(new PPSParsedRow
						{
							MainBet = (mainBetCell?.CellType ?? CellType.Blank) == CellType.Blank ? null : mainBetCell!.NumericCellValue,
							AdditionalBet = (additionalBetCell?.CellType ?? CellType.Blank) == CellType.Blank ? null : additionalBetCell!.NumericCellValue,
							ExcessiveHours = excessiveHoursCell.CellType == CellType.Blank ? null : excessiveHoursCell!.NumericCellValue,
							ShortFullName = fullNameCell.StringCellValue,
						});
					});

					return (string.Join("\r\n", allErrors), rows);
				}
			}
			catch (Exception ex)
			{
				return (ex.Message, new List<PPSParsedRow>());
			}
		}

		public record struct PPSParsedRow
		{
			public double? MainBet { get; init; }
			public double? AdditionalBet { get; init; }
			public double? ExcessiveHours { get; init; }
			public string ShortFullName { get; init; }
		}
	}
}
