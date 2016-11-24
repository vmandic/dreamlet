using dreamlet.DataAccessLayer.Entities.Base;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class Language : BaseMongoMetaEntity
    {
        public string Code { get; set; }
    }
}
