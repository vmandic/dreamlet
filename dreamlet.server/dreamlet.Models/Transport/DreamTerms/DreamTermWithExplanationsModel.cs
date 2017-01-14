using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.Models.Transport.DreamTerms
{
	public class DreamTermWithExplanationsModel : DreamTermModel
	{
		public DreamTermWithExplanationsModel()
		{
			this.Explanations = new List<string>();
		}

		public IEnumerable<string> Explanations { get; set; }
	}
}
