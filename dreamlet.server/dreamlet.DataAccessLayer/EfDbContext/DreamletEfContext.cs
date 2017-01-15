using dreamlet.DataAccessLayer.Entities.Models;
using DryIocAttributes;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace dreamlet.DataAccessLayer.EfDbContext
{
	[Export, WebRequestReuse]
	public class DreamletEfContext : DbContext
	{
		private static object _locker = new object();
		private static int _identifier = 0;

		static DreamletEfContext()
		{
			// NOTE: do not create database on "first contact", i.e. no database init strategy.
			Database.SetInitializer<DreamletEfContext>(null);
		}

		public DreamletEfContext()
		{
			this.Configuration.LazyLoadingEnabled = false;
			this.Configuration.AutoDetectChangesEnabled = false;
			this.Configuration.ValidateOnSaveEnabled = false;

			lock (_locker)
				_identifier++;
		}

		protected override void OnModelCreating(DbModelBuilder mb)
		{
			mb.Conventions.Remove<OneToManyCascadeDeleteConvention>();
			mb.Conventions.Remove<PluralizingTableNameConvention>();
			var configs = mb.Configurations;

			// NOTE: add table mapping configs
			configs.Add(new DreamExplanationMapping());
			configs.Add(new DreamTagMapping());
			configs.Add(new DreamTermMapping());
			configs.Add(new DreamTermTagMapping());
			configs.Add(new LanguageMapping());
			configs.Add(new UserMapping());
			configs.Add(new DreamTermStatisticMapping());
		}
	}
}
