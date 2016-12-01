using dreamlet.DataAccessLayer.Entities.Base;

namespace dreamlet.DataAccessLayer.Entities.Models
{
    public class User : BaseMongoMetaEntity
    {
        public string Role { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
    }
}
