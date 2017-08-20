using dreamlet.DbEntities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace dreamlet.DataAccessLayer.DbContext
{
  public class DbCompiledModelFactory
  {
    internal static DbProviderInfo DbProviderInfo => new DbProviderInfo("System.Data.SqlClient", "2012");

    [Export]
    public DbCompiledModel Model => _model.Value;

    private readonly Lazy<DbCompiledModel> _model;

    public DbCompiledModelFactory(IEnumerable<IModelMapping> mappings)
    {
      _model = new Lazy<DbCompiledModel>(() => {
        var b = new DbModelBuilder();

        foreach (var m in mappings) m.Define(b);

        b.Conventions.Remove<OneToManyCascadeDeleteConvention>();
        b.Conventions.Remove<PluralizingTableNameConvention>();

        return b.Build(DbProviderInfo).Compile();
      });
    }
  }
}
