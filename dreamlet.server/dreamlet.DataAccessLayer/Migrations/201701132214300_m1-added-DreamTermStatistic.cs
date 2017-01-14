namespace dreamlet.DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m1addedDreamTermStatistic : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DreamTermStatistic",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        SequenceId = c.Int(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        DreamTermId = c.Guid(nullable: false),
                        VisitCount = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.DreamTermId)
                .ForeignKey("dbo.DreamTerm", t => t.DreamTermId)
                .Index(t => t.DreamTermId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DreamTermStatistic", "DreamTermId", "dbo.DreamTerm");
            DropIndex("dbo.DreamTermStatistic", new[] { "DreamTermId" });
            DropTable("dbo.DreamTermStatistic");
        }
    }
}
