namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class uptodate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Materials", "ImplementerUserName", c => c.String());
            AddColumn("dbo.Materials", "ImplementerDepartmentId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Materials", "ImplementerDepartmentId");
            DropColumn("dbo.Materials", "ImplementerUserName");
        }
    }
}
