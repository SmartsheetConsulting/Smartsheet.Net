using ProfessionalServices.Core.Interfaces;
using Smartsheet.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Responses
{
    public class IndexResultResponse : ISmartsheetResult
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<ISmartsheetObject> Data { get; set; }
    }
}
