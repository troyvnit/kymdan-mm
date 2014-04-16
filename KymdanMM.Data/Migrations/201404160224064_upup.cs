namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class upup : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.MaterialProposals", "DepartmentId", "dbo.Departments");
            DropIndex("dbo.MaterialProposals", new[] { "DepartmentId" });
            AddColumn("dbo.MaterialProposals", "ProposerDepartmentId", c => c.Int(nullable: false));
            AddColumn("dbo.MaterialProposals", "ImplementerDepartmentId", c => c.Int(nullable: false));
            DropColumn("dbo.MaterialProposals", "DepartmentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MaterialProposals", "DepartmentId", c => c.Int(nullable: false));
            DropColumn("dbo.MaterialProposals", "ImplementerDepartmentId");
            DropColumn("dbo.MaterialProposals", "ProposerDepartmentId");
            CreateIndex("dbo.MaterialProposals", "DepartmentId");
            AddForeignKey("dbo.MaterialProposals", "DepartmentId", "dbo.Departments", "Id", cascadeDelete: true);
        }
    }
}
