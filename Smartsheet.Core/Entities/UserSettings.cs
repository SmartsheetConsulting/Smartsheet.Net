using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class UserSettings : ISmartsheetObject
    {
        public bool CriticalPathEnabled { get; set; }
        public bool DisplaySummaryTasks { get; set; }
    }
}
