using dreamlet.DataAccessLayer.Entities.Models;

namespace dreamlet.DataAccessLayer.Entities.Base
{
    public abstract class BaseMongoMetaEntity : BaseMongoEntity
    {
        public Meta Meta { get; set; }
    }
}
