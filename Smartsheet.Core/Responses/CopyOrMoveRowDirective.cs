using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Responses
{
    class CopyOrMoveRowDirective : ISmartsheetObject
    {
        public CopyOrMoveRowDirective()
        {

        }

        public CopyOrMoveRowDestination To { get; set; }
        public IList<long?> RowIds { get; set; }
    }
}
