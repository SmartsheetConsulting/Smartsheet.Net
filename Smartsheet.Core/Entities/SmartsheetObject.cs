using ProfessionalServices.Core.Interfaces;
using Smartsheet.Core.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class SmartsheetObject : ISmartsheetObject
    {
        public SmartsheetHttpClient _Client { get; set; }

        public SmartsheetObject()
        {

        }

        public SmartsheetObject(SmartsheetHttpClient client)
        {
            this._Client = client;
        }
    }
}
