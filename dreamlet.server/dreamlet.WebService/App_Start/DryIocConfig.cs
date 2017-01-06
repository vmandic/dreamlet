using dreamlet.BusinessLogicLayer.Ioc;
using dreamlet.Composition;
using DryIoc;
using DryIoc.MefAttributedModel;
using DryIoc.WebApi;
using System.Web.Http;

namespace dreamlet.WebService
{
	public class DryIocConfig
	{
		public static void Register(HttpConfiguration config)
		{
			var container = new Container()
				.WithMefAttributedModel()
				.WithWebApi(config);

			IocBootstrapper.RegisterDependencies(container);

			// Holds a global static singleton of the dependencies resolver.
			// Grants the ability to resolve dependencies in which ever assemby that references dreamlet.Composition.
			DependencyResolverLocator.RegisterResolverOnce(config.DependencyResolver);
		}
	}
}