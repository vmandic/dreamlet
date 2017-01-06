using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Configuration;

namespace dreamlet.Generic.Tests
{
    [TestClass]
    public class RepositoryTests
    {
        private static string DB_NAME, DB_USR, DB_PWD;

        [TestInitialize]
        public void Setup()
        {
            DB_NAME = ConfigurationManager.AppSettings["DB_NAME"].ToString();
            DB_USR = ConfigurationManager.AppSettings["DB_USR"].ToString();
            DB_PWD = ConfigurationManager.AppSettings["DB_PWD"].ToString();
        }

        private static void _AddUser()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void TestMongoRepo()
        {

        }
    }
}
