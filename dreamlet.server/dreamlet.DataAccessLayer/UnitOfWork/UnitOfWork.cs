﻿using dreamlet.DataAccessLayer.DbContext;
using DryIocAttributes;
using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace dreamlet.DataAccessLayer.UnitOfWork
{
	[Export(typeof(IUnitOfWork)), WebRequestReuse]
	public class UnitOfWork : IUnitOfWork
	{

		private DreamletDbContext _context;

		public UnitOfWork(DreamletDbContext context = null)
		{
			if (context != null)
				_context = context;

		}

		[Import]
		public DreamletDbContext DreamletContext
		{
			get
			{
				return _context ?? (_context = new DreamletDbContext());
			}
			set
			{
				_context = value;
			}
		}

		public bool Commit()
		{
			try
			{
				DreamletContext.SaveChanges();
				return true;
			}
			catch (Exception ex)
			{
				// TODO: handle or log ex
				return false;
			}
		}

		public async Task<bool> CommitAsync()
		{
			try
			{
				await DreamletContext.SaveChangesAsync();
				return true;
			}
			catch (Exception ex)
			{
				// TODO: handle or log ex
				return false;
			}
		}
	}
}
