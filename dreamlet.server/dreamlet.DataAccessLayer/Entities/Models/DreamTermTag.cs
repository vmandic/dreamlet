﻿using dreamlet.DataAccessLayer.Entities.Base;
using System;

namespace dreamlet.DataAccessLayer.Entities.Models
{
	public class DreamTermTag : BaseEntity
	{
		public Guid DreamTagId { get; set; }
		public Guid DreamTermId { get; set; }

		public virtual DreamTag DreamTag { get; set; }
		public virtual DreamTerm DreamTerm { get; set; }
	}

	internal class DreamTermTagMapping : BaseEntityMapping<DreamTermTag>
	{
		public DreamTermTagMapping()
		{

		}
	}
}
