using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
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

        public static IEnumerable<IRow> ParseCSVIntoRowsFromPath(string filePath, char delimiter = ',', bool trimQuotes = true)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8).Select(a => a.Split(delimiter)).ToList();

            var rows = ConvertCsvIntoRows(lines, trimQuotes);

            return rows;
        }

        public static IEnumerable<IRow> ParseCSVIntoRowsFromContents(string fileContents, char delimiter = ',', bool trimQuotes = true)
        {
            var lines = fileContents.Split(Environment.NewLine.ToCharArray()).Select(x => x.Split(delimiter)).ToList();

            var headerRow = lines[0];

            var returnLines = lines.Skip(1).Select(l => l.Take(l.Count() - 1).ToArray()).ToList();

            returnLines.Insert(0, headerRow);

            var rows = ConvertCsvIntoRows(returnLines, trimQuotes);

            return rows;
        }

        private static IList<IRow> ConvertCsvIntoRows(List<string[]> lines, bool trimQuotes)
        {
            IWorkbook workbook = new XSSFWorkbook();
            ISheet worksheet = workbook.CreateSheet();
            var rows = new List<IRow>();

            for (var i = 0; i < lines.Count(); i++)
            {
                var line = lines[i];

                var row = worksheet.CreateRow(i);

                if (line.Count() == lines[0].Count())
                {
                    for (var l = 0; l < line.Count(); l++)
                    {
                        var col = line[l];

                        if (trimQuotes)
                        {
                            row.CreateCell(l).SetCellValue(TypeConverter.TryConvert(col.Replace("\"", "")));
                        }
                        else
                        {
                            row.CreateCell(l).SetCellValue(TypeConverter.TryConvert(col));
                        }
                    }

                    rows.Add(row);
                }
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
