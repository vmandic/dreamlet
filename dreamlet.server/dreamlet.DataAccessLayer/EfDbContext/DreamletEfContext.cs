using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.DataAccessLayer.Entities.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.EfDbContext
{
	public class DreamletEfContext : DbContext
	{

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
		}
	}
}
