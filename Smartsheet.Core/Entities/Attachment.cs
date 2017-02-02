using ProfessionalServices.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smartsheet.Core.Entities
{
    public class Attachment : ISmartsheetObject
    {
        public Attachment()
        {

        }

        public long Id { get; set; }
        public long UrlExpiresInMillis { get; set; }
        public long ParentId { get; set; }
        public long SizeInKb { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string AttachmentType { get; set; }
        public string AttachmentSubType { get; set; }
        public string MimeType { get; set; }
        public string ParentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public User CreatedBy { get; set; }
    }
}
