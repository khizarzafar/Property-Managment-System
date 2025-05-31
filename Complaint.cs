using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocietyManagmentSystem
{
    public class Complaint
    {
        public int complaint_id { get; set; }
        public int resident_id { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public string status { get; set; } // "Open", "In Progress", "Resolved"
        public string admin_notes { get; set; }

        public Complaint()
        {
            // Default status could be set here if not handled by DB default
            // status = "Open";
            admin_notes = string.Empty;
        }
    }
}
