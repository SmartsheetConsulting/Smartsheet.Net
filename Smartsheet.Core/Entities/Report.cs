using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class Report : ISmartsheetObject
    {
        public Report()
        {
              
        }

        public long Id { get; set; }
    }
}
