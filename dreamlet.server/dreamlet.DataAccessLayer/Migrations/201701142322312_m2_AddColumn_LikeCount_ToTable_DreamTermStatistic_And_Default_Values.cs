namespace dreamlet.DataAccessLayer.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	
	public partial class m2_AddColumn_LikeCount_ToTable_DreamTermStatistic_And_Default_Values : DbMigration
	{
		public override void Up()
		{
			AddColumn("dbo.DreamTermStatistic", "LikeCount", c => c.Long(nullable: false));

			Sql(@"
				INSERT INTO DreamTermStatistic (DreamTermId, VisitCount, LikeCount)
					SELECT
						dt.Id,
						0,
						0
						FROM DreamTerm dt
			");
		}
		
		public override void Down()
		{
			Sql("DELETE FROM DreamTermStatistic");
			DropColumn("dbo.DreamTermStatistic", "LikeCount");
		}
	}
}
