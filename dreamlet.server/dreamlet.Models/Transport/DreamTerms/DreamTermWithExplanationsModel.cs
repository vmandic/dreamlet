using System.Collections.Generic;

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
