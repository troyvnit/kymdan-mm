namespace KymdanMM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class upup : DbMigration
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
        }
        
        public override void Down()
        {
        }
    }
}
