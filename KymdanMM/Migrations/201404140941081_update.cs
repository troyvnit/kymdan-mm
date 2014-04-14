namespace KymdanMM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            
            AddColumn("dbo.UserProfile", "Department_Id", c => c.Int());
            CreateIndex("dbo.UserProfile", "Department_Id");
            AddForeignKey("dbo.UserProfile", "Department_Id", "dbo.Departments", "Id");
            DropColumn("dbo.UserProfile", "Department");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserProfile", "Department", c => c.Int(nullable: false));
            DropForeignKey("dbo.UserProfile", "Department_Id", "dbo.Departments");
            DropIndex("dbo.UserProfile", new[] { "Department_Id" });
            DropColumn("dbo.UserProfile", "Department_Id");
            DropTable("dbo.Departments");
        }
    }
}
