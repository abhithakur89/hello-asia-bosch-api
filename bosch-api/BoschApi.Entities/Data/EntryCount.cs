using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoschApi.Entities.Data
{
    [Table("EntryCounts")]
    public class EntryCount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntryCountId { get; set; }

        public DateTime Date { get; set; }
        
        [ForeignKey("CameraId")]
        public int CameraId { get; set; }
        public virtual Camera Camera { get; set; }

        public int Count { get; set; }
    }
}
