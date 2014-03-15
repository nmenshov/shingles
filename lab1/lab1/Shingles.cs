using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace lab1
{    
    public static class Shingles
    {
        private const int HashCount = 84;
        private const int HashGroupCount = 6;

 
        private static List<char> _stop_symbols = new List<char>() 
        { '.', ',', '!', '?', ':', ';', '-', '\n', '\r', '(', ')', '\'' };
        
        private static List<string> _stop_words = new List<string>(){"это", "как", "так",
                                                                        "и", "в", "над",
                                                                        "к", "до", "не",
                                                                        "на", "но", "за",
                                                                        "то", "с", "ли",
                                                                        "а", "во", "от",
                                                                        "со", "для", "о",
                                                                        "же", "ну", "вы",
                                                                        "бы", "что", "кто",
                                                                        "он", "она"};

        private static List<Func<string, uint>> _84AvasomeFunctions;
 
        static Shingles()
        {            
            GenerateFunctions();
        }

        private static void GenerateFunctions()
        {
            _84AvasomeFunctions = new List<Func<string, uint>>();

            var r = new Random();
            for (int i = 0; i < 84; i++)
            {
                int b = r.Next(5, 20);
                int simple = r.Next(b, b*2);
                var function = new Func<string, uint>(s => (uint) s.Select((t, j) =>
                {
                    return (t*((uint) Math.Pow(b, j)%simple)%simple);
                }).Sum());
                _84AvasomeFunctions.Add(function);
            }            
        }
        
        public static List<string> Canonize(string text)
        {
            List<string> list = text.ToLower().Split(' ').ToList();
            list.ForEach(x => x.Trim(_stop_symbols.ToArray()));
            list.RemoveAll(x => _stop_words.Contains(x));

            return list;
        }

        public static List<string> GetShingle(List<string> text, uint length)
        {            
            var ret = new List<string>();

            for (int i = 0; i < text.Count-length; i++)
            {
                string mid = "";
                for (int j = i; j < i+length; j++)
                    mid += text[j];
                ret.Add(mid);
            }

            return ret;
        }

        public static List<uint> GetHash(List<string> text)
        {
            int index = 0;
            var result = new List<uint>();
            for (var i = 0; i < HashCount; i++)
            {
                result.Add(uint.MaxValue);
            }

            foreach (var shingle in text)
            {
                var hash = _84AvasomeFunctions[index](shingle);
                if (result[index] >= hash)
                    result[index] = hash;
                index = (int) ((index + 1)%HashCount);
            }
            result.Sort((x, y) =>
            {
                if (x > y) return 1;
                if (x < y) return -1;
                return 0;
            });
            return result;
        }

        public static float Compare(List<uint> shingles1, List<uint> shingles2)
        {
            uint count = 0;
            var realcount =  Math.Max(shingles1.FindIndex(x => x == uint.MaxValue),
                shingles2.FindIndex(x => x == uint.MaxValue));

            for (int i = 0; i < HashCount; i++)
            {
                if (shingles1[i] == shingles2[i] && shingles1[i] != uint.MaxValue)
                    count++;
            }


            if (realcount == -1)
                realcount = (int) HashCount;

            return (float)count/realcount;
        }

        public static List<KeyValuePair<int, int>> SuperMap()
        {
            var map = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < HashCount; i++)
            {
                map.Add(new KeyValuePair<int, int>());
            }

            var list = new List<int>();
            var r = new Random();
            for (int i = 0; i < HashGroupCount; i++)
            {
                for (int j = 0; j < HashCount/HashGroupCount; j++)
                {
                    int k = r.Next(0, (HashCount - 1));
                    while (list.Contains(k))
                        k = r.Next(0, HashCount);
                    list.Add(k);
                    map[k] = new KeyValuePair<int, int>(i,j);
                }   
            }

            return map;
        }

        public static List<List<uint>> GetSuperHash(List<KeyValuePair<int, int>> map, List<uint> hash )
        {
            var ret = new List<List<uint>>();
            for (int i = 0; i < HashGroupCount; i++)
            {
                ret.Add(new List<uint>());
                for (int j = 0; j < HashCount/HashGroupCount; j++)
                {
                    ret[i].Add(0);
                }
            }

            for (int i = 0; i < HashCount; i++)
            {
                ret[map[i].Key][map[i].Value] = hash[i];
            }
            return ret;
        }

        public static float CompareSuper(List<List<uint>> shingles1, List<List<uint>> shingles2)
        {
            uint count = 0;

            for (int i = 0; i < HashGroupCount; i++)
            {
                bool res = true;
                for (int j = 0; j < HashCount/HashGroupCount; j++)
                {
                    if (shingles1[i].Contains(shingles2[i][j]))
                        count++;
                }
            }

            return (float) count/HashCount;
        }

        public static List<KeyValuePair<int, int>> GetMegaHash()
        {
            var map = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < HashGroupCount; i++)
            {
                for (int j = i+1; j < HashGroupCount; j++)
                {
                    map.Add(new KeyValuePair<int, int>(i,j));
                }
            }
            return map;
        }

        public static List<List<uint>> GetMegaHash(List<KeyValuePair<int, int>> map, List<List<uint>> super)
        {
            var ret = new List<List<uint>>();

            foreach (var pair in map)
            {
                var list = new List<uint>(super[pair.Key]);    
                list.AddRange(super[pair.Value]);
                ret.Add(list);
            }

            return ret;
        }

        public static float CompareMega(List<List<uint>> shingles1, List<List<uint>> shingles2)
        {
            uint count = 0;

            for (int i = 0; i < HashGroupCount; i++)
            {
                bool res = true;
                for (int j = 0; j < HashCount / HashGroupCount; j++)
                {
                    if (shingles1[i].Contains(shingles2[i][j]))
                        count++;
                }
            }

            return (float)count / HashCount/HashGroupCount*shingles1.Count;
        }
    }
}
