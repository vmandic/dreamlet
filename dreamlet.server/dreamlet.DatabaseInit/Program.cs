using dreamlet.BusinessLogicLayer.Ioc;
using DryIoc;
using DryIoc.MefAttributedModel;
using System;

namespace dreamlet.DatabaseInit
{
  class Program
  {
    static void Main(string[] args)
    {
      var c = new Container().WithMefAttributedModel();
      c.Register<Importer>(Reuse.InWebRequest);

      using (c = IocBootstrapper.RegisterDependencies(c).WithMefAttributedModel().OpenScope(Reuse.WebRequestScopeName))
      {
        var i = c.Resolve<Importer>();

        i.TryInsertUserAdmin();
        i.TryInsertLanguages();
        i.WriteInsertionScriptToFile();
      }

      Console.WriteLine("DONE!");
      Console.WriteLine("\"script.sql\" generated successfully in \\bin directory. Press any key to close...");
      Console.ReadKey();
    }
  }
}
