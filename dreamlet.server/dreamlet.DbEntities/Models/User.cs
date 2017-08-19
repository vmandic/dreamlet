using dreamlet.DbEntities.Base;
using dreamlet.Utilities;

namespace dreamlet.DbEntities.Models
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
