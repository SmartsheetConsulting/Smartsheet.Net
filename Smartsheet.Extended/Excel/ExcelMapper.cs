using AutoMapper;
using NPOI.HSSF.UserModel;
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

        public static IEnumerable<Row> MapExcelRows(IList<IRow> excelRows, int headerRowNumber)
        {
            var rows = new List<Row>();

            var headerRow = excelRows[0];

            foreach (var excelRow in excelRows)
            {
                if (!excelRow.Cells.Any(c => string.IsNullOrEmpty(c.ToString())))
                {
                    if (excelRow != excelRows[headerRowNumber])
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
            }

            return rows;
        }
    }
}
