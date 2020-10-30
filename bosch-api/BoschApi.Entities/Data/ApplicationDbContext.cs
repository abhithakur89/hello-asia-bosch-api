using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoschApi.Entities.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Site> Sites { get; set; }
        public DbSet<Gate> Gates { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<EntryRecord> EntryRecords { get; set; }
        public DbSet<ExitRecord> ExitRecords { get; set; }
        public DbSet<EntryCount> EntryCounts { get; set; }
        public DbSet<ExitCount> ExitCounts { get; set; }
        public DbSet<CrowdDensityLevel> CrowdDensityLevels { get; set; }
    }
}
