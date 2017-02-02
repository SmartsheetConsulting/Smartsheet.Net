using System.Collections.Generic;
using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class Template : ISmartsheetObject
    {
        public Template()
        {
            this.Tags = new List<string>();
            this.Categories = new List<string>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AccessLevel { get; set; }
        public string Image { get; set; }
        public string LargeImage { get; set; }
        public string Locale { get; set; }
        public string Type { get; set; }
        public bool Blank { get; set; }
        public string GlobalTemplate { get; set; }

        public ICollection<string> Tags { get; set; }
        public ICollection<string> Categories { get; set; }
    }
}
