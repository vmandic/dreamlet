using DryIocAttributes;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace dreamlet.DataAccessLayer.DbContext
{
  [Export, WebRequestReuse]
	public class DreamletDbContext : System.Data.Entity.DbContext
	{
		private static object _locker = new object();
		private static int _identifier = 0;

		static DreamletDbContext()
		{
			// NOTE: do not create database on "first contact", i.e. no database init strategy.
			Database.SetInitializer<DreamletDbContext>(null);
		}

		public DreamletDbContext(DbCompiledModel model) : base(model)
		{
			this.Configuration.LazyLoadingEnabled = false;
			this.Configuration.AutoDetectChangesEnabled = false;
			this.Configuration.ValidateOnSaveEnabled = false;

			lock (_locker)
				_identifier++;
		}
	}
}
