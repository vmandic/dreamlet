namespace dreamlet.DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class m1_initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DreamExplanation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Explanation = c.String(),
                        DreamTermId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DreamTerm", t => t.DreamTermId)
                .Index(t => t.DreamTermId);
            
            CreateTable(
                "dbo.DreamTerm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        LanguageId = c.Int(nullable: false),
                        Term = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId)
                .Index(t => t.LanguageId);
            
            CreateTable(
                "dbo.DreamTermStatistic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        DreamTermId = c.Int(nullable: false),
                        VisitCount = c.Long(nullable: false),
                        LikeCount = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.DreamTermId)
                .ForeignKey("dbo.DreamTerm", t => t.DreamTermId)
                .Index(t => t.DreamTermId);
            
            CreateTable(
                "dbo.DreamTermTag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        DreamTagId = c.Int(nullable: false),
                        DreamTermId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DreamTag", t => t.DreamTagId)
                .ForeignKey("dbo.DreamTerm", t => t.DreamTermId)
                .Index(t => t.DreamTagId)
                .Index(t => t.DreamTermId);
            
            CreateTable(
                "dbo.DreamTag",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Tag = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Language",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        InternationalCode = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Uid = c.Guid(nullable: false, identity: true),
                        ActiveState = c.Int(nullable: false, defaultValue: 1),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        Role = c.Int(nullable: false),
                        Email = c.String(),
                        PasswordHash = c.String(),
                    })
                .PrimaryKey(t => t.Id);
		}
        
        public override void Down()
        {
            DropForeignKey("dbo.DreamTerm", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.DreamTermTag", "DreamTermId", "dbo.DreamTerm");
            DropForeignKey("dbo.DreamTermTag", "DreamTagId", "dbo.DreamTag");
            DropForeignKey("dbo.DreamTermStatistic", "DreamTermId", "dbo.DreamTerm");
            DropForeignKey("dbo.DreamExplanation", "DreamTermId", "dbo.DreamTerm");
            DropIndex("dbo.DreamTermTag", new[] { "DreamTermId" });
            DropIndex("dbo.DreamTermTag", new[] { "DreamTagId" });
            DropIndex("dbo.DreamTermStatistic", new[] { "DreamTermId" });
            DropIndex("dbo.DreamTerm", new[] { "LanguageId" });
            DropIndex("dbo.DreamExplanation", new[] { "DreamTermId" });
            DropTable("dbo.User");
            DropTable("dbo.Language");
            DropTable("dbo.DreamTag");
            DropTable("dbo.DreamTermTag");
            DropTable("dbo.DreamTermStatistic");
            DropTable("dbo.DreamTerm");
            DropTable("dbo.DreamExplanation");
        }
    }
}
