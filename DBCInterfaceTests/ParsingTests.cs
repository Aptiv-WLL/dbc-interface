using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aptiv.DBCFiles.Tests
{
    static class TestToken
    {
        static public string path = "..\\..\\..\\DBC files\\";
        static public IEnumerator<string> e = Directory.EnumerateFiles(path, "*.dbc", SearchOption.AllDirectories).GetEnumerator();
    }

    [TestClass]
    public class ParsingTests
    {
        DBCFile dbf;
        const byte HEAD = 0x50;

        [TestMethod()]
        [TestProperty("Category", "Load DBC")]
        public void DBCFileTest0()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }

        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest1()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }

        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest2()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest3()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest4()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }

        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest5()
        {
            Console.WriteLine(TestToken.e.Current);
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest6()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest7()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest8()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest9()
        {
            Console.WriteLine(TestToken.e.Current);
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest10()
        {
            Console.WriteLine(TestToken.e.Current);
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest11()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest12()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest13()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest14()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest15()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest16()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest17()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest18()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest19()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest20()
        {
            Console.WriteLine(TestToken.e.Current);
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest21()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        [TestMethod()]
        [TestCategory("Load DBC")]
        public void DBCFileTest22()
        {
            if (TestToken.e.MoveNext())
                dbf = new DBCFile(TestToken.e.Current);
            Console.WriteLine(TestToken.e.Current);
        }
        
        [TestMethod()]
        public void TryParseDatabaseTest()
        {
            DBCFile dbf = DBCFile.TryParseDatabase(TestToken.path + "ISC1_I_NIS_R_19__vAC.dbc", out bool partial);
            DBCFile dbf_ = new DBCFile(TestToken.path + "ISC1_I_NIS_R_19__vAC.dbc");
            //check if these are the same
        }

        [TestMethod]
        public void BrettEddieBug()
        {
            var path = ".\\Resources\\ISM_A_NCR17Q4_HMICAN_171121.dbc";
            DBCFile file = new DBCFile(path);
        }
    }
}
