using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoschApi.Entities.Data
{
    [Table("Gates")]
    public class Gate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GateId { get; set; }
        public string GateName { get; set; }

        [ForeignKey("SiteId")]
        public int SiteId { get; set; }
        public virtual Site Site { get; set; }

        public virtual ICollection<Camera> Cameras { get; set; }
    }
}
