using dreamlet.BusinessLogicLayer.Ioc;
using dreamlet.DataAccessLayer.Entities.Models;
using dreamlet.Generic.Tests.Services;
using DryIoc;
using DryIoc.MefAttributedModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace dreamlet.Generic.Tests
{
	[TestClass]
	public class DbSeedTests
	{
		private static string DB_NAME, DB_USR, DB_PWD;
		private static IContainer _container;

		[TestInitialize]
		public void Setup()
		{
			DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
			DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
			DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();
		}

		private static void _AddUser(IMongoDatabase db)
		{

		}

		[TestCleanup]
		public void Cleanup()
		{

		}

		[TestMethod]
		public void Should_open_file_path_for_seed()
		{
			// PREPARE
			string json = System.IO.File.ReadAllText(@"Scrapes/dream-scrape1-take2-formatted.json");
			var obj = JsonConvert.DeserializeObject<IEnumerable<object>>(json);

			// ACT


			// ASSERT

		}
	}
}
