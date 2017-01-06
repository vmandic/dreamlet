using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Web.Http;

namespace dreamlet.WebService
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Web API routes
			config.MapHttpAttributeRoutes();

			var appXmlType = config.Formatters.XmlFormatter.SupportedMediaTypes.FirstOrDefault(t => t.MediaType == "application/xml");
			config.Formatters.XmlFormatter.SupportedMediaTypes.Remove(appXmlType);

			var contractResolver = new DefaultContractResolver();
			contractResolver.NamingStrategy = new SnakeCaseNamingStrategy(true, false);

			config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = contractResolver;
			config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			config.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore;

			// Register DryIoc IoC manager
			DryIocConfig.Register(config);
		}
	}
}
