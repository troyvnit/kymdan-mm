namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MaterialProposals", "ApproveStatus", c => c.Int(nullable: false));
            DropColumn("dbo.MaterialProposals", "Approved");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MaterialProposals", "Approved", c => c.Boolean(nullable: false));
            DropColumn("dbo.MaterialProposals", "ApproveStatus");
        }
    }
}
