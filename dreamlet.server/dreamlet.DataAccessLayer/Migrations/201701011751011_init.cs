namespace dreamlet.DataAccessLayer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DreamExplanation",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Explanation = c.String(),
                        DreamTermId = c.Guid(nullable: false),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DreamTerm", t => t.DreamTermId)
                .Index(t => t.DreamTermId);
            
            CreateTable(
                "dbo.DreamTerm",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        LanguageId = c.Guid(nullable: false),
                        Term = c.String(),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.LanguageId)
                .Index(t => t.LanguageId);
            
            CreateTable(
                "dbo.DreamTermTag",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        DreamTagId = c.Guid(nullable: false),
                        DreamTermId = c.Guid(nullable: false),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
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
                        Id = c.Guid(nullable: false, identity: true),
                        Tag = c.String(),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Language",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        InternationalCode = c.String(),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Role = c.Int(nullable: false),
                        Email = c.String(),
                        PasswordHash = c.String(),
                        ActiveState = c.Int(nullable: false),
                        CreatedAtUtc = c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"),
                        SequenceId = c.Int(nullable: false, identity: true),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DreamTerm", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.DreamTermTag", "DreamTermId", "dbo.DreamTerm");
            DropForeignKey("dbo.DreamTermTag", "DreamTagId", "dbo.DreamTag");
            DropForeignKey("dbo.DreamExplanation", "DreamTermId", "dbo.DreamTerm");
            DropIndex("dbo.DreamTermTag", new[] { "DreamTermId" });
            DropIndex("dbo.DreamTermTag", new[] { "DreamTagId" });
            DropIndex("dbo.DreamTerm", new[] { "LanguageId" });
            DropIndex("dbo.DreamExplanation", new[] { "DreamTermId" });
            DropTable("dbo.User");
            DropTable("dbo.Language");
            DropTable("dbo.DreamTag");
            DropTable("dbo.DreamTermTag");
            DropTable("dbo.DreamTerm");
            DropTable("dbo.DreamExplanation");
        }
    }
}
