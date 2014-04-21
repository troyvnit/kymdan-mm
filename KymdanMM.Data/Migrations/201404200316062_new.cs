namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _new : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Departments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DepartmentName = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MaterialProposals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        ManagementCode = c.String(),
                        ProposalCode = c.String(),
                        ProposerUserName = c.String(),
                        ProposerDepartmentId = c.Int(nullable: false),
                        ImplementerUserName = c.String(),
                        ImplementerDepartmentId = c.Int(nullable: false),
                        ProgressStatusId = c.Int(nullable: false),
                        Finished = c.Boolean(nullable: false),
                        ApproveStatus = c.Int(nullable: false),
                        Note = c.String(),
                        Deadline = c.DateTime(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProgressStatus", t => t.ProgressStatusId, cascadeDelete: true)
                .Index(t => t.ProgressStatusId);
            
            CreateTable(
                "dbo.Comments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Content = c.String(),
                        Approved = c.Boolean(nullable: false),
                        PosterUserName = c.String(),
                        MaterialProposalId = c.Int(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MaterialProposals", t => t.MaterialProposalId, cascadeDelete: true)
                .Index(t => t.MaterialProposalId);
            
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
            
            CreateTable(
                "dbo.ProgressStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MaterialProposals", "ProgressStatusId", "dbo.ProgressStatus");
            DropForeignKey("dbo.Materials", "MaterialProposalId", "dbo.MaterialProposals");
            DropForeignKey("dbo.Comments", "MaterialProposalId", "dbo.MaterialProposals");
            DropIndex("dbo.Materials", new[] { "MaterialProposalId" });
            DropIndex("dbo.Comments", new[] { "MaterialProposalId" });
            DropIndex("dbo.MaterialProposals", new[] { "ProgressStatusId" });
            DropTable("dbo.ProgressStatus");
            DropTable("dbo.Materials");
            DropTable("dbo.Comments");
            DropTable("dbo.MaterialProposals");
            DropTable("dbo.Departments");
        }
    }
}
