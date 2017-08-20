using System.Data.Entity;

namespace dreamlet.DbEntities.Base
{
  public interface IModelMapping
  {
    void Define(DbModelBuilder builder);
  }
}
