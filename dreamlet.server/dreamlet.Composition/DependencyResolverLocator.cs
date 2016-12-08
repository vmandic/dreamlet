using System.Web.Http.Dependencies;

namespace dreamlet.Composition
{
    public class DependencyResolverLocator
    {
        private static readonly object _locker = new object();
        private static IDependencyResolver _instance;

        public static void RegisterResolverOnce(IDependencyResolver httpWebApiDependencyResolver)
        {
            if (_instance == null)
                _instance = httpWebApiDependencyResolver;
        }

        public static IDependencyResolver Instance
        {
            get
            {
                lock (_locker)
                {
                    return _instance;
                }
            }
        }
    }
}
