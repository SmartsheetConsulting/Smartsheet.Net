using ProfessionalServices.Core.Interfaces;
using Smartsheet.Core.Interfaces;

namespace ProfessionalServices.Core.Responses
{
    public class BulkItemFailureResponse : ISmartsheetResult
    {
        public long Index { get; set; }
        public long RowId { get; set; }
        public ErrorResponse Error { get; set; }
    }
}
