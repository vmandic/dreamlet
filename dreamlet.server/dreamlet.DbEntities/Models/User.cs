using dreamlet.DbEntities.Base;
using dreamlet.Utilities;
using System.ComponentModel.Composition;

namespace dreamlet.DbEntities.Models
{
	public class User : BaseEntity
	{
		public DreamletRole Role { get; set; }
		public string Email { get; set; }
		public string PasswordHash { get; set; }

    [Export(typeof(IModelMapping))]
    class Map : BaseEntityMapping<User> { }
  }
}
