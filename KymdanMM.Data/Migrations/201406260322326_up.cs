namespace KymdanMM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class up : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Materials", "Quantity", c => c.Decimal(nullable: false));
            AlterColumn("dbo.Materials", "InventoryQuantity", c => c.Decimal(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Materials", "InventoryQuantity", c => c.Int(nullable: false));
            AlterColumn("dbo.Materials", "Quantity", c => c.Int(nullable: false));
        }
    }
}
