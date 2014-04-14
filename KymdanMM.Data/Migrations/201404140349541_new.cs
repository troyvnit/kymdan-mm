namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _new : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MaterialProposals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        ManagementCode = c.String(),
                        ProposalCode = c.String(),
                        ProposerUserName = c.String(),
                        ImplementerUserName = c.String(),
                        Department = c.Int(nullable: false),
                        ProgressStatus = c.Int(nullable: false),
                        Finished = c.Boolean(nullable: false),
                        Approved = c.Boolean(nullable: false),
                        Note = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Materials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MaterialName = c.String(),
                        Description = c.String(),
                        Quantity = c.Int(nullable: false),
                        Unit = c.String(),
                        Used = c.Boolean(nullable: false),
                        UsingPurpose = c.String(),
                        Deadline = c.DateTime(nullable: false),
                        Note = c.String(),
                        MaterialProposalId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MaterialProposals", t => t.MaterialProposalId, cascadeDelete: true)
                .Index(t => t.MaterialProposalId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Materials", "MaterialProposalId", "dbo.MaterialProposals");
            DropIndex("dbo.Materials", new[] { "MaterialProposalId" });
            DropTable("dbo.Materials");
            DropTable("dbo.MaterialProposals");
        }
    }
}
