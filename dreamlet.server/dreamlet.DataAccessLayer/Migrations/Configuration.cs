namespace dreamlet.DataAccessLayer.Migrations
{
	using System;
	using System.Collections.Generic;
	using System.Data.Entity.Migrations;
	using System.Data.Entity.Migrations.Design;
	using System.Data.Entity.Migrations.Model;

	internal sealed class Configuration : DbMigrationsConfiguration<dreamlet.DataAccessLayer.EfDbContext.DreamletEfContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
			CodeGenerator = new ExtendedMigrationCodeGenerator();
		}

        protected override void Seed(dreamlet.DataAccessLayer.EfDbContext.DreamletEfContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }

	public class ExtendedMigrationCodeGenerator : MigrationCodeGenerator
	{
		public override ScaffoldedMigration Generate(string migrationId, IEnumerable<MigrationOperation> operations, string sourceModel, string targetModel, string @namespace, string className)
		{
			foreach (MigrationOperation operation in operations)
			{
				if (operation is CreateTableOperation)
				{
					foreach (var column in ((CreateTableOperation)operation).Columns)
						if (column.ClrType == typeof(DateTime) && column.IsNullable.HasValue && !column.IsNullable.Value && string.IsNullOrEmpty(column.DefaultValueSql))
							column.DefaultValueSql = "GETUTCDATE()";
				}
				else if (operation is AddColumnOperation)
				{
					ColumnModel column = ((AddColumnOperation)operation).Column;

					if (column.ClrType == typeof(DateTime) && column.IsNullable.HasValue && !column.IsNullable.Value && string.IsNullOrEmpty(column.DefaultValueSql))
						column.DefaultValueSql = "GETUTCDATE()";
				}
			}

			CSharpMigrationCodeGenerator generator = new CSharpMigrationCodeGenerator();

			return generator.Generate(migrationId, operations, sourceModel, targetModel, @namespace, className);
		}
	}
}
