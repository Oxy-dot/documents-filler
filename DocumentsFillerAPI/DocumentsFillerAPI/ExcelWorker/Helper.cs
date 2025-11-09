using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;

namespace DocumentsFillerAPI.ExcelWorker
{
	public static class Helper
	{
		public static IEnumerable<IRow> GetNotEmptyRows(this ISheet sheet, int startRow)
		{
			var sheetEnumerator = sheet.GetRowEnumerator();

			while (sheetEnumerator.MoveNext())
			{
				var current = (IRow)sheetEnumerator.Current;
				if (current.RowNum < startRow)
					continue;

				if (current.Cells.All(a => a.CellType == CellType.Blank))
					continue;

				yield return current;
			}
		}

		public static string CustomTrim(this string text) => text.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "").ToLower();

		public static ICellStyle GenerateDefaultStyle(this IWorkbook workbook, bool isBold, FontHeight fontHeight, bool offBorder = false, bool textWrap = true, HorizontalAlignment horizontalAligment = HorizontalAlignment.Center, bool isCenterVerticalAlignment = true)
		{
			var newStyle = workbook.CreateCellStyle();
			newStyle.Alignment = horizontalAligment;
			newStyle.VerticalAlignment = isCenterVerticalAlignment ? VerticalAlignment.Center : VerticalAlignment.Justify;
			newStyle.WrapText = textWrap;
			if (!offBorder)
			{
				newStyle.BorderLeft = BorderStyle.Thin;
				newStyle.BorderRight = BorderStyle.Thin;
				newStyle.BorderTop = BorderStyle.Thin;
				newStyle.BorderBottom = BorderStyle.Thin;
			}

			IFont font = workbook.CreateFont();
			font.FontName = "Times New Roman";
			font.FontHeight = (fontHeight == FontHeight.Default ? 12 : fontHeight == FontHeight.Small ? 10 : 9) * 20;
			font.IsBold = isBold;
			newStyle.SetFont(font);

			return newStyle;
		}

		public static ICell SetStyle(this ICell cell, ICellStyle style)
		{
			cell.CellStyle = style;
			return cell;
		}

		public static ICell SetDefaultStyle(this ICell cell)
		{
			cell.CellStyle.Alignment = HorizontalAlignment.Center;
			cell.CellStyle.VerticalAlignment = VerticalAlignment.Center;
			cell.CellStyle.BorderLeft = BorderStyle.Medium;
			cell.CellStyle.BorderRight = BorderStyle.Medium;
			cell.CellStyle.BorderTop = BorderStyle.Medium;
			cell.CellStyle.BorderBottom = BorderStyle.Medium;
			return cell;
		}

		public enum FontHeight
		{
			/// <summary>
			/// 12
			/// </summary>
			Default, // 12
			/// <summary>
			/// 10
			/// </summary>
			Small, // 10
			/// <summary>
			/// 9
			/// </summary>
			VerySmall, //9
		}
	}
}
