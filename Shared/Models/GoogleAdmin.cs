using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Dev1.Module.GoogleAdmin.Shared.Models
{
    [Table("Dev1GoogleAdmin")]
    public class GoogleAdmin : IAuditable
    {
        [Key]
        public int GoogleAdminId { get; set; }
        public int ModuleId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
