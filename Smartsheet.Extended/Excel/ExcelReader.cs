using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.IO;

namespace Smartsheet.Extended.Excel
{
    public static class ExcelReader
    {
        static ExcelReader()
        {

        }

        public static IWorkbook ParseExcelFileIntoWorkbook(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);

            HSSFWorkbook workbook = new HSSFWorkbook(fileStream);

            return workbook;
        }

        public static ISheet ParseExcelFileIntoSheet(string filePath, int sheetIndex)
        {
            var workbook = ParseExcelFileIntoWorkbook(filePath);

            HSSFSheet sheet = (HSSFSheet)workbook.GetSheetAt(sheetIndex);

            return sheet;
        }

        public static IEnumerable<IRow> ParseExcelFileIntoRows(string filePath, int sheetIndex)
        {
            var sheet = ParseExcelFileIntoSheet(filePath, sheetIndex);

            var rows = new List<HSSFRow>();

            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                HSSFRow row = (HSSFRow)sheet.GetRow(rowIndex);

                if (row != null)
                {
                    for (int cellIndex = 0; cellIndex <= row.LastCellNum; cellIndex++)
                    {
                        HSSFCell cell = (HSSFCell)row.GetCell(cellIndex);

                        if (cell != null)
                        {
                            row.Cells.Add(cell);
                        }
                    }

                    rows.Add(row);
                }
            }

            return rows;
        }
    }
}
