using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Aptiv.DBCFiles.Tests
{
    [TestClass]
    public class AsyncParsingTests
    {
        DBCFile dbf;
        const byte HEAD = 0x50;

        [TestMethod()]
        [TestProperty("Category", "Load DBC")]
        public async Task DBCFileTest0()
        {
            foreach (var dbc in Directory.EnumerateFiles(TestToken.path, "*.dbc", SearchOption.AllDirectories))
            {
                dbf = new DBCFile(dbc, false);
                await dbf.CreateLibraryDBCAsync();
                Console.WriteLine(dbc);
            }
        }
    }
}
