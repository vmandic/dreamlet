using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.Models.Transport.DreamTerms
{
	public class DreamTermLetterGroup
	{
		public DreamTermLetterGroup()
		{
			this.DreamTerms = new List<DreamTermModel>();
		}

		public string LetterGroup { get; set; }
		public IEnumerable<DreamTermModel> DreamTerms { get; set; }
	}
}
