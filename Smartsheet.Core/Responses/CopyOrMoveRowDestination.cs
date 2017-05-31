using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Responses
{
    class CopyOrMoveRowDestination : ISmartsheetObject
    {
        public CopyOrMoveRowDestination()
        {

        }

        public long? SheetId { get; set; }
    }
}
