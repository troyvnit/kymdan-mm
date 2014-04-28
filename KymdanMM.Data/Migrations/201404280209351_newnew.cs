namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newnew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MaterialProposals", "FromHardProposal", c => c.Boolean(nullable: false));
            AddColumn("dbo.Materials", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Materials", "EndDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Materials", "Finished", c => c.Boolean(nullable: false));
            AddColumn("dbo.Materials", "ApproveStatus", c => c.Int(nullable: false));
            DropColumn("dbo.MaterialProposals", "ImplementerUserName");
            DropColumn("dbo.MaterialProposals", "ImplementerDepartmentId");
            DropColumn("dbo.MaterialProposals", "ProgressStatusId");
            DropColumn("dbo.MaterialProposals", "Finished");
            DropColumn("dbo.MaterialProposals", "ApproveStatus");
            DropColumn("dbo.MaterialProposals", "Deadline");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MaterialProposals", "Deadline", c => c.DateTime(nullable: false));
            AddColumn("dbo.MaterialProposals", "ApproveStatus", c => c.Int(nullable: false));
            AddColumn("dbo.MaterialProposals", "Finished", c => c.Boolean(nullable: false));
            AddColumn("dbo.MaterialProposals", "ProgressStatusId", c => c.Int(nullable: false));
            AddColumn("dbo.MaterialProposals", "ImplementerDepartmentId", c => c.Int(nullable: false));
            AddColumn("dbo.MaterialProposals", "ImplementerUserName", c => c.String());
            DropColumn("dbo.Materials", "ApproveStatus");
            DropColumn("dbo.Materials", "Finished");
            DropColumn("dbo.Materials", "EndDate");
            DropColumn("dbo.Materials", "StartDate");
            DropColumn("dbo.MaterialProposals", "FromHardProposal");
        }
    }
}
