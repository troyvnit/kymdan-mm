namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uop : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MaterialProposals", "ProgressStatusId", "dbo.ProgressStatus");
            DropIndex("dbo.MaterialProposals", new[] { "ProgressStatusId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.MaterialProposals", "ProgressStatusId");
            AddForeignKey("dbo.MaterialProposals", "ProgressStatusId", "dbo.ProgressStatus", "Id", cascadeDelete: true);
        }
    }
}
