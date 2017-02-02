using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class UpdateRequest : MultiRowEmail
    {
        public UpdateRequest()
        {
            this.RowIds = new List<long>();
            this.ColumnIds = new List<long>();
        }

        public long? Id { get; set; }
        public DateTime? CreatedAt { get; set;}
        public DateTime? ModifiedAt { get; set; }

        public User SentBy { get; set; }
    }
}
