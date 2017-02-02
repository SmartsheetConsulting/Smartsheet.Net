using ProfessionalServices.Core.Interfaces;
using Smartsheet.Core.Interfaces;
using System.Collections.Generic;

namespace ProfessionalServices.Core.Responses
{
    public class ResultResponse<T> : ISmartsheetResult
    {
        public int ResultCode { get; set; }
        public string Message { get; set; }
        public T Result { get; set; }
        public int Version { get; set; }
        public ICollection<BulkItemFailureResponse> FailedItems { get; set; }
    }
}
