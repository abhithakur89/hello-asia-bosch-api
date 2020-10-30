using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoschApi.Entities.Data
{
    [Table("EntryRecords")]
    public class EntryRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryRecordId { get; set; }

        public DateTime Timestamp { get; set; }

        [ForeignKey("CameraId")]
        public int CameraId { get; set; }
        public virtual Camera Camera { get; set; }
    }
}
