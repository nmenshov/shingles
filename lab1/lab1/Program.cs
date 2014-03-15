using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            var file1 = new StreamReader("../../Data/1.txt",Encoding.Default);
            var file2 = new StreamReader("../../Data/2.txt", Encoding.Default);

            string text1 = file1.ReadToEnd();
            string text2 = file2.ReadToEnd();

            uint lenght = 5;

            var shin1 = Shingles.GetShingle(Shingles.Canonize(text1), lenght);
            var shin2 = Shingles.GetShingle(Shingles.Canonize(text2), lenght);

            var hash1 = Shingles.GetHash(shin1);
            var hash2 = Shingles.GetHash(shin2);

            var result = Shingles.Compare(hash1, hash2);

            Console.WriteLine("common = {0}%\n", result * 100);

            var map = Shingles.SuperMap();

            var shash1 = Shingles.GetSuperHash(map, hash1);
            var shash2 = Shingles.GetSuperHash(map, hash2);

            var result2 = Shingles.CompareSuper(shash1, shash2);
            Console.WriteLine("super = {0}%\n", result2 * 100);

            var map2 = Shingles.GetMegaHash();

            var mhash1 = Shingles.GetMegaHash(map2, shash1);
            var mhash2 = Shingles.GetMegaHash(map2, shash2);

            var result3 = Shingles.CompareMega(mhash1, mhash2);
            Console.WriteLine("mega = {0}%\n", result2 * 100);

            Console.ReadLine();
        }
    }
}
