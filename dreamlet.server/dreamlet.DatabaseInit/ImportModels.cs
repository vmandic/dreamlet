using dreamlet.DataAccessLayer.Entities.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.DatabaseInit.ImportModels
{
	public class JsonDreamTerm : BaseMongoEntity
	{
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("explanations")]
		public IEnumerable<string> Explanations { get; set; }
	}
}
