
using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class Cell : ISmartsheetObject
    {
        public Cell()
        {
            this.Column = new Column();
        }

        public Cell Build()
        {
            this.Column = null;

            return this;
        }

        public Cell Build(bool? strict)
        {
            this.Column = null;
            this.Strict = strict;

            return this;
        }

        public long? ColumnId { get; set; }

        public dynamic Value { get; set; }

        public string Formula { get; set; }

        public string DisplayValue { get; set; }

        public bool? Strict { get; set; }

        public Hyperlink Hyperlink { get; set; }

        public Column Column { get; set; }
    }
}
