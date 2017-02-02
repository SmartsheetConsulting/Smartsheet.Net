using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Smartsheet.Core.Http;
using System.Threading.Tasks;
using ProfessionalServices.Core.Responses;

namespace Smartsheet.Core.Entities
{
    public class Sheet : SmartsheetObject
    {
        public Sheet()
        {

        }

        public Sheet(string sheetName)
        {
            this.Name = sheetName;
        }

        public Sheet(string sheetName, ICollection<Column> columns)
        {
            this.Name = sheetName;
            this.Columns = columns;
        }

        public Sheet(SmartsheetHttpClient client, string sheetName = "", ICollection<Column> columns = null) : base(client)
        {
            this.EffectiveAttachmentOptions = new List<string>();
            this.Columns = new List<Column>();
            this.Rows = new List<Row>(); 
        }

        public long? Id { get; set; }
        public long? Version { get; set; }
        public long? OwnerId { get; set; }
        public long? FromId { get; set; }

        public int? TotalRowCount { get; set; }

        public string Name { get; set; }
        public string AccessLevel { get; set; }
        public string Permalink { get; set; }
        public string Owner { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public bool? ReadOnly { get; set; }
        public bool? GantEnabled { get; set; }
        public bool? DependenciesEnabled { get; set; }
        public bool? ResourceManagementEnabled { get; set; }
        public bool? CellImageUploadEnabled { get; set; }
        public bool? Favorite { get; set; }
        public bool? ShowParentRowsForFilters { get; set; }

        public UserSettings UserSettings { get; set; }
        public Workspace Workspace { get; set; }

        public ICollection<string> EffectiveAttachmentOptions { get; set; }
        public ICollection<Column> Columns { get; set; }
        public ICollection<Row> Rows { get { return this.MapCellsToColumns(); } set { this.UnformattedRows = value; } }
        private ICollection<Row> UnformattedRows { get; set; }

        //
        //  Extension Methods
        #region Extensions
        private ICollection<Row> MapCellsToColumns()
        {
            if (this.UnformattedRows != null)
            {
                foreach (var row in this.UnformattedRows)
                {
                    var parsedColumns = this.Columns.ToList();
                    var parsedCells = row.Cells.ToList();

                    for (var i = 0; i < parsedColumns.Count; i++)
                    {
                        var cell = parsedCells[i];

                        cell.ColumnId = parsedColumns[i].Id;
                        cell.Column = parsedColumns[i];
                    }
                }
            }

            return this.UnformattedRows;
        }

        public Column GetColumnById(long columnId)
        {
            var column = this.Columns.Where(c => c.Id == columnId).FirstOrDefault();

            return column;
        }

        public Column GetColumnByTitle(string columnTitle)
        {
            var column = this.Columns.Where(c => c.Title.Equals(columnTitle)).FirstOrDefault();

            return column;
        }

        #endregion

        //
        //  Client Methods
        #region SmartsheetHttpClient
        public async Task<IEnumerable<Row>> CreateRows(IEnumerable<Row> rows, bool? toTop = null, bool? toBottom = null, long? parentId = null, long? siblingId = null)
        {
            if (rows.Count() > 1)
            {
                foreach (var row in rows)
                {
                    foreach (var cell in row.Cells)
                    {
                        cell.Build();
                    }

                    row.ToTop = toTop;
                    row.ToBottom = toBottom;
                    row.ParentId = parentId;
                    row.SiblingId = siblingId;
                }
            }

            var response = await this._Client.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.POST, string.Format("sheets/{0}/rows", this.Id), rows);

            return response.Result;
        }

        public async Task<IEnumerable<Row>> UpdateRows(IEnumerable<Row> rows)
        {
            foreach (var row in rows)
            {
                foreach (var cell in row.Cells)
                {
                    cell.Build();
                }

                row.Build(false, row.Cells);
            }

            var response = await this._Client.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.PUT, string.Format("sheets/{0}/rows", this.Id), rows);

            return response.Result;
        }
        #endregion
    }
}
