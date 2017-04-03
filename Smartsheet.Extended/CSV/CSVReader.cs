using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using Smartsheet.Extended.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Extended.CSV
{
    public static class CSVReader
    {
        static CSVReader()
        {

        }

        public static IEnumerable<IRow> ParseCSVIntoRows(string filePath, char delimiter = ',')
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Select(a => a.Split(delimiter)).ToList();

            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet();
            var rows = new List<IRow>();

            for (var i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];

                var row = worksheet.CreateRow(i);

                for (var l = 0; l < line.Count(); l++)
                {
                    var col = line[l];

                    row.CreateCell(l).SetCellValue(TypeConverter.TryConvert(col));
                }

                rows.Add(row);
            }

            return rows;
        }

        public static IEnumerable<string[]> ReadCsv(string fileName, char delimiter = ',')
        {
            var lines = System.IO.File.ReadAllLines(fileName, Encoding.UTF8).Select(a => a.Split(delimiter));
            return (lines);
        }
    }
}
