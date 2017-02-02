﻿using System;
using System.Collections.Generic;
using ProfessionalServices.Core.Interfaces;
using System.Linq;

namespace Smartsheet.Core.Entities
{
    public class Row : SmartsheetObject
    {
        public Row()
        {
            this.Cells = new List<Cell>();
            this.Columns = new List<Column>();
        }

        public Row Build(
            bool toTop,
            ICollection<Cell> cells = null)
        {
            this.Cells = cells;
            this.Columns = null;
            this.Discussions = null;
            this.Attatchments = null;

            return this;
        }

        public Row Build(bool toTop)
        {
            this.Columns = null;
            this.Discussions = null;
            this.Attatchments = null;

            var buildCells = new List<Cell>();

            foreach (var cell in this.Cells)
            {
                buildCells.Add(cell.Build());
            }

            this.Cells = buildCells;

            return this;
        }

        public long? Id { get; set; }
        public long? SheetId { get; set; }
        public long? ParentId { get; set; }
        public long? SiblingId { get; set; }

        public int? RowNumber { get; set; }
        public int? Version { get; set; }

        public bool? FilteredOut { get; set; }
        public bool? InCriticalPath { get; set; }
        public bool? Locked { get; set; }
        public bool? LockedForUser { get; set; }
        public bool? Expanded { get; set; }
        public bool? ToTop { get; set; }
        public bool? ToBottom { get; set; }
        public bool? Above { get; set; }

        public string AccessLevel { get; set; }
        public string Format { get; set; }
        public string ConditionalFormat { get; set; }
        public string Permalink { get; set; }

        public DateTime? CreatedAt { get; set; }
        public User CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public User ModifiedBy { get; set; }

        public ICollection<Cell> Cells { get; set; }
        public ICollection<Column> Columns { get; set; }

        public ICollection<Discussion> Discussions { get; set; }
        public ICollection<Attachment> Attatchments { get; set; }

        //
        //  Extension Methods
        #region Extensions
        public Cell GetCellForColumn(long columnId)
        {
            var cell = this.Cells.Where(c => c.Column.Id == columnId).FirstOrDefault();

            return cell;
        }

        public Cell GetCellForColumn(string columnTitle)
        {
            var cell = this.Cells.Where(c => c.Column.Title.Trim() == columnTitle).FirstOrDefault();

            return cell;
        }

        public void UpdateCellForColumn(string columnTitle, dynamic value)
        {
            var cell = this.Cells.Where(c => c.Column.Title.Trim() == columnTitle).FirstOrDefault();
            cell.Value = value;
        }
        #endregion
    }
}