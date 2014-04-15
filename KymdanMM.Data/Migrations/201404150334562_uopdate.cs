namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uopdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MaterialProposals", "Department_Id", "dbo.Departments");
            DropForeignKey("dbo.MaterialProposals", "ProgressStatus_Id", "dbo.ProgressStatus");
            DropIndex("dbo.MaterialProposals", new[] { "Department_Id" });
            DropIndex("dbo.MaterialProposals", new[] { "ProgressStatus_Id" });
            RenameColumn(table: "dbo.MaterialProposals", name: "Department_Id", newName: "DepartmentId");
            RenameColumn(table: "dbo.MaterialProposals", name: "ProgressStatus_Id", newName: "ProgressStatusId");
            AlterColumn("dbo.MaterialProposals", "DepartmentId", c => c.Int(nullable: false));
            AlterColumn("dbo.MaterialProposals", "ProgressStatusId", c => c.Int(nullable: false));
            CreateIndex("dbo.MaterialProposals", "DepartmentId");
            CreateIndex("dbo.MaterialProposals", "ProgressStatusId");
            AddForeignKey("dbo.MaterialProposals", "DepartmentId", "dbo.Departments", "Id", cascadeDelete: true);
            AddForeignKey("dbo.MaterialProposals", "ProgressStatusId", "dbo.ProgressStatus", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MaterialProposals", "ProgressStatusId", "dbo.ProgressStatus");
            DropForeignKey("dbo.MaterialProposals", "DepartmentId", "dbo.Departments");
            DropIndex("dbo.MaterialProposals", new[] { "ProgressStatusId" });
            DropIndex("dbo.MaterialProposals", new[] { "DepartmentId" });
            AlterColumn("dbo.MaterialProposals", "ProgressStatusId", c => c.Int());
            AlterColumn("dbo.MaterialProposals", "DepartmentId", c => c.Int());
            RenameColumn(table: "dbo.MaterialProposals", name: "ProgressStatusId", newName: "ProgressStatus_Id");
            RenameColumn(table: "dbo.MaterialProposals", name: "DepartmentId", newName: "Department_Id");
            CreateIndex("dbo.MaterialProposals", "ProgressStatus_Id");
            CreateIndex("dbo.MaterialProposals", "Department_Id");
            AddForeignKey("dbo.MaterialProposals", "ProgressStatus_Id", "dbo.ProgressStatus", "Id");
            AddForeignKey("dbo.MaterialProposals", "Department_Id", "dbo.Departments", "Id");
        }
    }
}
