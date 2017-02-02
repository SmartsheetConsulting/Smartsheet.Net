using System.Collections.Generic;
using ProfessionalServices.Core.Interfaces;

namespace Smartsheet.Core.Entities
{
    public class Folder : ISmartsheetObject
    {
        public Folder()
        {
            this.Sheets = new List<Sheet>();
            this.Folders = new List<Folder>();
            this.Reports = new List<Report>();
            this.Templates = new List<Template>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public bool Favorite { get; set; }
        public string Permalink { get; set; }

        public ICollection<Sheet> Sheets { get; set; }
        public ICollection<Folder> Folders { get; set; }
        public ICollection<Report> Reports { get; set; }
        public ICollection<Template> Templates { get; set; }
    }
}
