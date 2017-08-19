using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.DataAccessLayer.DbContext;
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
				typeof(DreamletDbContext).GetTypeInfo().Assembly,
				typeof(IDreamTermsService).GetTypeInfo().Assembly
			});

			return container;
		}
	}
}
