using AutoMapper;
using NPOI.SS.UserModel;
using Smartsheet.Core.Entities;
using Smartsheet.Extended.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Extended.Excel
{
    public static class ExcelMapper
    {
        static ExcelMapper()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMissingTypeMaps = true;
                cfg.AddProfile<MappingProfile>();
            });
        }

        public static IEnumerable<Row> MapExcelRows(IList<IRow> excelRows, int headerRowIndex, bool excludeRowsWithNullCells = false, int numberOfBeginningRowsToSkip = 0, int numberOfEndRowsToSkip = 0)
        {
            var rows = new List<Row>();

            var headerRow = excelRows[headerRowIndex];

            foreach (var excelRow in excelRows)
            {
                if (excelRows.IndexOf(excelRow) > headerRowIndex)
                {
                    var row = new Row();

                    foreach (var excelCell in excelRow.Cells)
                    {
                        var cell = new Cell();

                        cell.Column.Title = (String)CellExtension.GetCellValue(headerRow.Cells[excelCell.ColumnIndex]);
                        cell.Value = CellExtension.GetCellValue(excelCell);

                        row.Cells.Add(cell);
                    }

                    rows.Add(row);
                }
            }

            if (excludeRowsWithNullCells)
            {
                foreach (var row in rows)
                {
                    if (!row.Cells.Any(c => string.IsNullOrEmpty(c.ToString())))
                    {
                        rows.Remove(row);
                    }
                }
            }

            if (numberOfBeginningRowsToSkip > 0)
            {
                for (int i = 0; i < numberOfBeginningRowsToSkip; i++)
                {
                    rows.Remove(rows[i]);
                }
            }

            if (numberOfEndRowsToSkip > 0)
            {
                for (int i = 0; i < numberOfEndRowsToSkip; i++)
                {
                    rows.Remove(rows.LastOrDefault());
                }
            }

            return rows;
        }
    }
}
