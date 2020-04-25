using Customer.API.Data.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Customer.API.Data
{
    public class CampContext : DbContext
    {
        public CampContext() : base(ConfigurationManager.ConnectionStrings["CustomerConnectionString"].ConnectionString)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<CampContext, Migrations.Configuration>());
        }

        public DbSet<Camp> Camps { get; set; }
        public DbSet<Speaker> Speakers { get; set; }
        public DbSet<Talk> Talks { get; set; }

    }
}