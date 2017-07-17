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

        public Sheet(string sheetName, IList<Column> columns)
        {
            this.Name = sheetName;
            this.Columns = columns;
        }

        public Sheet(SmartsheetHttpClient client, string sheetName = "", IList<Column> columns = null) : base(client)
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

        public IList<string> EffectiveAttachmentOptions { get; set; }
        public IList<Column> Columns { get; set; }
        public IList<Row> Rows { get { return this.MapCellsToColumns(); } set { this.UnformattedRows = value; } }
        private IList<Row> UnformattedRows { get; set; }

        //
        //  Extension Methods
        #region Extensions
        private IList<Row> MapCellsToColumns()
        {
            if (this.UnformattedRows != null)
            {
                foreach (var row in this.UnformattedRows)
                {
                    var parsedColumns = this.Columns.ToList();
                    var parsedCells = row.Cells.ToList();

                    for (var i = 0; i < parsedColumns.Count; i++)
                    {
                        var cell = parsedCells.Find(pc => pc.ColumnId == parsedColumns[i].Id);

                        if (cell != null)
                        {
                            cell.ColumnId = parsedColumns[i].Id;
                            cell.Column = parsedColumns[i];
                        }
                    }
                }
            }

            return this.UnformattedRows;
        }

        public Sheet RemoveSystemColumnsAndCells()
        {
            this.MapCellsToColumns();

            var systemColumns = this.Columns
                .Where(c => c.SystemColumnType != null)
                .Select(c => c.Id)
                .ToList();

            for(var i = 0; i < this.Columns.Count; i++)
            {
                if (systemColumns.Contains(this.Columns[i].Id))
                {
                    this.Columns.Remove(this.Columns[i]);
                }
            }

            for(var x = 0; x < this.Rows.Count; x++)
            {
                for(var y = 0; y < this.Rows[x].Cells.Count; y++)
                {
                    if (systemColumns.Contains(this.Rows[x].Cells[y].ColumnId))
                    {
                        this.Rows[x].Cells.Remove(this.Rows[x].Cells[y]);
                    }
                }
            }

            return this;
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
        public async Task<IEnumerable<Row>> CreateRows(IList<Row> rows, bool? toTop = null, bool? toBottom = null, bool? enforceStrict = null, long? parentId = null, long? siblingId = null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                rows[i].Build();

                foreach (var cell in rows[i].Cells)
                {
                    cell.Build(enforceStrict);
                }

                rows[i].Id = null;
                rows[i].CreatedAt = null;
                rows[i].ModifiedAt = null;
                rows[i].RowNumber = null;
                rows[i].ToTop = toTop;
                rows[i].ToBottom = toBottom;
                rows[i].ParentId = parentId;
                rows[i].SiblingId = siblingId;
            }

            var response = await this._Client.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.POST, string.Format("sheets/{0}/rows", this.Id), rows);

            return response.Result;
        }

        public async Task<IEnumerable<Row>> UpdateRows(IList<Row> rows, bool? toTop = null, bool? toBottom = null, bool? above = null, long? parentId = null, long? siblingId = null)
        {
            if (rows.Count() > 0)
            {
                for (var i = 0; i < rows.Count(); i++)
                {
                    for (var x = 0; x < rows[i].Cells.Count(); x++)
                    {
                        rows[i].Cells.ElementAt(x).Build(false);

                        if (rows[i].Cells.ElementAt(x).Value == null)
                        {
                            rows[i].Cells.Remove(rows[i].Cells.ElementAt(x));
                        }

                        rows[i].Cells.ElementAt(x).Column = null;
                    }

                    rows[i].Build(false, false, true);

                    rows[i].Above = above;
                    rows[i].ToTop = toTop;
                    rows[i].ToBottom = toBottom;
                    rows[i].ParentId = parentId;
                    rows[i].SiblingId = siblingId;
                }
            }

            var response = await this._Client.ExecuteRequest<ResultResponse<IEnumerable<Row>>, IEnumerable<Row>>(HttpVerb.PUT, string.Format("sheets/{0}/rows", this.Id), rows);

            return response.Result;
        }

        public async Task<IEnumerable<long>> RemoveRows(IList<Row> rows)
        {
            var rowList = rows.ToList();

            var response = new ResultResponse<IEnumerable<long>>();

            while(rowList.Count > 0)
            {
                var rowIdList = string.Join(",", rows.Take(300).Select(r => Convert.ToString(r.Id)));

                var url = string.Format("sheets/{0}/rows?ids={1}&ignoreRowsNotFound=true", this.Id, rowIdList);

                response = await this._Client.ExecuteRequest<ResultResponse<IEnumerable<long>>, IEnumerable<Row>>(HttpVerb.DELETE, string.Format("sheets/{0}/rows?ids={1}&ignoreRowsNotFound=true", this.Id, rowIdList), null);

                if (response.Message.Equals("SUCCESS"))
                {
                    rowList.RemoveAll(r => rowIdList.Contains(Convert.ToString(r.Id)));
                }
            }

            return response.Result;
        }
        #endregion
    }
}
