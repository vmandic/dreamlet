using dreamlet.BusinessLogicLayer.Services.Interfaces;
using dreamlet.BusinessLogicLayer.Services.Providers;
using dreamlet.DataAccessLayer.EfDbContext;
using DryIoc;
using DryIoc.MefAttributedModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace dreamlet.BusinessLogicLayer.Ioc
{
	public class IocBootstrapper
	{
		public static IContainer RegisterDependencies(IContainer container)
		{
			container.RegisterExports(new List<Assembly> {
				typeof(DreamletEfContext).GetTypeInfo().Assembly,
				typeof(IDreamTermsService).GetTypeInfo().Assembly
			});

			return container;
		}
	}
}
