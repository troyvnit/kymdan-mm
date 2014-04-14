namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
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
                "dbo.ProgressStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.MaterialProposals", "Department_Id", c => c.Int());
            AddColumn("dbo.MaterialProposals", "ProgressStatus_Id", c => c.Int());
            CreateIndex("dbo.MaterialProposals", "Department_Id");
            CreateIndex("dbo.MaterialProposals", "ProgressStatus_Id");
            AddForeignKey("dbo.MaterialProposals", "Department_Id", "dbo.Departments", "Id");
            AddForeignKey("dbo.MaterialProposals", "ProgressStatus_Id", "dbo.ProgressStatus", "Id");
            DropColumn("dbo.MaterialProposals", "Department");
            DropColumn("dbo.MaterialProposals", "ProgressStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MaterialProposals", "ProgressStatus", c => c.Int(nullable: false));
            AddColumn("dbo.MaterialProposals", "Department", c => c.Int(nullable: false));
            DropForeignKey("dbo.MaterialProposals", "ProgressStatus_Id", "dbo.ProgressStatus");
            DropForeignKey("dbo.MaterialProposals", "Department_Id", "dbo.Departments");
            DropIndex("dbo.MaterialProposals", new[] { "ProgressStatus_Id" });
            DropIndex("dbo.MaterialProposals", new[] { "Department_Id" });
            DropColumn("dbo.MaterialProposals", "ProgressStatus_Id");
            DropColumn("dbo.MaterialProposals", "Department_Id");
            DropTable("dbo.ProgressStatus");
            DropTable("dbo.Departments");
        }
    }
}
