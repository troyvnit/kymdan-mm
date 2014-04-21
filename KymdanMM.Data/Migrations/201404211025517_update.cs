namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Comments", "PosterDisplayName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Comments", "PosterDisplayName");
        }
    }
}
