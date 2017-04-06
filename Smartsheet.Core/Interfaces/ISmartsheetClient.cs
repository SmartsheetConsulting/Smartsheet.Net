using Smartsheet.Core.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Interfaces
{
    public interface ISmartsheetClient
    {
        Task<TResult> ExecuteRequest<TResult, T>(HttpVerb verb, string url, T data, IList<Tuple<string, string>> headers = null, bool deserializeAsJson = true);
    }
}
