using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

            XSSFWorkbook workbook = new XSSFWorkbook(fileStream);

            return workbook;
        }

        public static ISheet ParseExcelFileIntoSheet(string filePath, int sheetIndex)
        {
            var workbook = ParseExcelFileIntoWorkbook(filePath);

            XSSFSheet sheet = (XSSFSheet)workbook.GetSheetAt(sheetIndex);

            return sheet;
        }

        public static IEnumerable<IRow> ParseExcelFileIntoRows(string filePath, int sheetIndex)
        {
            var sheet = ParseExcelFileIntoSheet(filePath, sheetIndex);

            var rows = new List<XSSFRow>();

            for (int rowIndex = 0; rowIndex < sheet.LastRowNum; rowIndex++)
            {
                XSSFRow row = (XSSFRow)sheet.GetRow(rowIndex);

                if (row != null)
                {
                    for (int cellIndex = 0; cellIndex <= row.LastCellNum; cellIndex++)
                    {
                        XSSFCell cell = (XSSFCell)row.GetCell(cellIndex);

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
