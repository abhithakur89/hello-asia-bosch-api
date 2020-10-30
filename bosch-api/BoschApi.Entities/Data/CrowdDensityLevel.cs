using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace BoschApi.Entities.Data
{
    [Table("CrowdDensityLevels")]
    public class CrowdDensityLevel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CrowdDensityLevelId { get; set; }

        public DateTime Timestamp { get; set; }

        [ForeignKey("CameraId")]
        public int CameraId { get; set; }
        public virtual Camera Camera { get; set; }

        public int Level { get; set; }
    }
}
