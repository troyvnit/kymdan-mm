using System.Data.Entity;
using KymdanMM.Data.Migrations;
using KymdanMM.Model.Models;

namespace KymdanMM.Data
{
    public class KymdanMMEntities : DbContext
    {
        public KymdanMMEntities()
            : base("KymdanMMEntities")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<KymdanMMEntities, Configuration>());
        }

        DbSet<MaterialProposal> MaterialProposals { get; set; }

        public virtual void Commit()
        {
            base.SaveChanges();
        }
    }
}
