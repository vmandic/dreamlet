using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.DataAccessLayer.DbContext;
using dreamlet.DbEntities.Base;
using DryIoc;
using DryIoc.MefAttributedModel;
using System.Collections.Generic;
using System.Reflection;

namespace dreamlet.BusinessLogicLayer.Ioc
{
	public class IocBootstrapper
	{
		public static IContainer RegisterDependencies(IContainer container)
		{
			container.RegisterExports(new List<Assembly> {
				typeof(DreamletDbContext).GetTypeInfo().Assembly,     // dreamlet.DataAccessLayer
				typeof(IDreamTermsService).GetTypeInfo().Assembly,    // dreamlet.BusinessLogicLayer
        typeof(IBaseEntity).GetTypeInfo().Assembly,           // dreamlet.DbEntites
      });

      return container;
		}
	}
}
