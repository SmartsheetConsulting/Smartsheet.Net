using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Responses
{
    public class CopyOrMoveRowResult : ISmartsheetObject
    {
        public CopyOrMoveRowResult()
        {

        }

        public long? DestinationSheetId { get; set; }
        public IList<RowMapping> RowMappings { get; set; } 
    }
}
