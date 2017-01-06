using dreamlet.BusinessLogicLayer.Services.Base;
using dreamlet.BusinessLogicLayer.Services.Interfaces;
using DryIocAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dreamlet.BusinessLogicLayer.Services.Providers
{
	[Export(typeof(IDreamStoriesService)), WebRequestReuse]
	public class DreamStoriesService : BaseService, IDreamStoriesService
	{
		public DreamStoriesService()
		{

		}
	}
}
