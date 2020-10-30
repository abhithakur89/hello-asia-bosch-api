using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoschApi.Entities.Data
{
    [Table("ExitCounts")]
    public class ExitCount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExitCountId { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("CameraId")]
        public int CameraId { get; set; }
        public virtual Camera Camera { get; set; }

        public int Count { get; set; }
    }
}
