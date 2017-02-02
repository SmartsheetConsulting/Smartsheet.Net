using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class Hyperlink : ISmartsheetObject
    {
        public long? SheetId { get; set; }
        public string Url { get; set; }
    }
}
