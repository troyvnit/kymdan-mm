namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newnewnew : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Materials", "ApproveDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Materials", "ProgressStatusId", c => c.Int(nullable: false));
            DropColumn("dbo.Materials", "EndDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Materials", "EndDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Materials", "ProgressStatusId");
            DropColumn("dbo.Materials", "ApproveDate");
        }
    }
}
