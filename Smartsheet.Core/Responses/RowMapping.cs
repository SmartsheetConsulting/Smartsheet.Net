using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Responses
{
    public class RowMapping : ISmartsheetObject
    {
        public RowMapping()
        {

        }

        public long? From { get; set; }
        public long? To { get; set; }
    }
}
