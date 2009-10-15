using System;
using iMDBLookup;

namespace iMDB_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            iMDBParser parser = new iMDBParser();

            parser.Parse("Matrix");
        }
    }
}
