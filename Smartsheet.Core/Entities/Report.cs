using ProfessionalServices.Core.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace Smartsheet.Core.Entities
{
    public class Report : Sheet
    {
        public Report()
        {

        }

        public ICollection<Sheet> SourceSheets { get; set; }
    }
}
