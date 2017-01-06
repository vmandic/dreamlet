using dreamlet.DataAccessLayer.Entities.Base;
using dreamlet.Utilities;

namespace dreamlet.DataAccessLayer.Entities.Models
{
	public class User : BaseEntity
	{
		public DreamletRole Role { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }
	}

	public class UserMapping : BaseEntityMapping<User>
	{
		public UserMapping() : base()
		{

		}
	}
}
