using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Extended.Excel
{
    public class ColumnMap
    {
        public ColumnMap()
        {

        }

        public ColumnMap Init(IEnumerable<Tuple<string, string>> pairs)
        {
            this.Pairs = pairs;

            return this;
        }

        public string GetDestinationColumn(string sourceColumnName)
        {
            sourceColumnName = sourceColumnName.Trim();

            return this.Pairs.Where(p => p.Item1.Equals(sourceColumnName)).Select(p => p.Item2).FirstOrDefault();
        }

        public string GetSourceColumn(string destinationColumnName)
        {
            destinationColumnName = destinationColumnName.Trim();

            return this.Pairs.Where(p => p.Item2.Equals(destinationColumnName)).Select(p => p.Item1).FirstOrDefault();
        }

        public IEnumerable<Tuple<string, string>> Pairs { get; private set; }
    }
}
