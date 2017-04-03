using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
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

            var fileType = GetFileExtension(fileStream);

            switch (fileType)
            {
                case ".xls":
                    return new HSSFWorkbook(fileStream);
                case ".xlsx":
                    return new XSSFWorkbook(fileStream);
                default:
                    throw new NotImplementedException("File Type Not Supported");
            }
        }

        public static ISheet ParseExcelFileIntoSheet(string filePath, int sheetIndex)
        {
            var workbook = ParseExcelFileIntoWorkbook(filePath);

            if (workbook.GetType() == typeof(HSSFWorkbook))
            {
                var sheet = (HSSFSheet)workbook.GetSheetAt(sheetIndex);

                return sheet;
            }
            else if(workbook.GetType() == typeof(XSSFWorkbook))
            {
                var sheet = (XSSFSheet)workbook.GetSheetAt(sheetIndex);

                return sheet;
            }
            else
            {
                throw new NotImplementedException("Workbook Not In Known Format");
            }
        }

        public static IEnumerable<IRow> ParseExcelFileIntoRows(string filePath, int sheetIndex)
        {
            var sheet = ParseExcelFileIntoSheet(filePath, sheetIndex);

            if (sheet.GetType() == typeof(HSSFSheet))
            {
                var rows = new List<HSSFRow>();

                for (int rowIndex = 0; rowIndex < sheet.PhysicalNumberOfRows; rowIndex++)
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
            else if (sheet.GetType() == typeof(XSSFSheet))
            {
                var rows = new List<XSSFRow>();

                for (int rowIndex = 0; rowIndex < sheet.PhysicalNumberOfRows; rowIndex++)
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
            else
            {
                throw new NotImplementedException("Workbook Not In Known Format");
            }
        }

        private static string GetFileExtension(FileStream fileStream)
        {
            return Path.GetExtension(fileStream.Name);
        }
    }
}
