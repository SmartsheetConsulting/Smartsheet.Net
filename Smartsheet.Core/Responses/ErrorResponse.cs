using ProfessionalServices.Core.Interfaces;
using Smartsheet.Core.Interfaces;

namespace ProfessionalServices.Core.Responses
{
    public class ErrorResponse : ISmartsheetResult
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
