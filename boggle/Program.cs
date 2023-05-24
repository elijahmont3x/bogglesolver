using DotNet.RestApi.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

// TODO: Expand on Dictionary setup? Maybe include order, removal of duplicates?
// TODO: Analyze time space efficiency and comment what I beleive the O is
// TODO: Add comments walking through each step of the code
// TOOD: Check to see running on latest Dot Net for completion

namespace Boggling
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Boggle myBoggle = new Boggle();
            IEnumerable<string> myResults;

            myBoggle.SetLegalWords();
            myResults = myBoggle.SolveBoard(4, 4, "ruateartwuinxoia");

            int count = 0;
            foreach (string word in myResults)
            {
                count++;
                Console.WriteLine($"{word}");
            }
            Console.WriteLine($"Boggles: {count}");
        }
    }

    internal class Boggle
    {
        private static string[] dictionary;
        private string[] letters;
        private int scopeX;
        private int scopeY;
        private List<string> results;

        enum eMatchType
        {
            PREFIX = 0,
            FULL = 1,
            INVALID = 2
        }

        public override string ToString()
        {
            string ret = string.Empty;

            for (int i = 0; i < scopeX; i++)
            {
                for (int j = 0; j < scopeY; j++)
                {
                    ret += letters[i * scopeX + j] + " ";
                }
                ret += "\n";
            }
            ret = ret.Trim();
            return ret;
        }

        public void SetLegalWords(IEnumerable<string> allWords = null)
        {
            List<string> list = new List<string>();
            using (StreamReader sr = File.OpenText("../../../wordbook.txt"))
            {
                string line = String.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line);
                }
            }

            dictionary = list.ToArray<string>();
        }

        public IEnumerable<string> SolveBoard(int scopeX, int scopeY, string letters)
        {
            this.letters = letters.Select(x => x.ToString().ToUpper()).ToArray();
            this.scopeX = scopeX;
            this.scopeY = scopeY;
            results = new List<string>();
            Console.WriteLine(this);

            for (int i = 0; i < scopeX; i++)
            {
                for (int j = 0; j < scopeY; j++)
                {
                    results.AddRange(Search(i, j));
                }
            }

            return results.Distinct();
        }

        private List<string> Search(int x, int y)
        {
            // Used to check if we already visited a node
            bool[] marked = new bool[scopeX * scopeY];

            return Search(x, y, marked, string.Empty);
        }

        private List<string> Search(int x, int y, bool[] marked, string line)
        {
            marked[x * scopeY + y] = true;
            line += letters[x * scopeY + y];

            if (!IsWordPrefix(line)) 
                return new List<string>();

            List<string> ret = new List<string>();
            if (IsFullWord(line))
                ret.Add(line);

            foreach ((int, int) coord in GetUnmarkedNeighbors(x, y, marked))
            {
                //Console.WriteLine(coord);
                bool[] markedCopy = new bool[scopeX * scopeY];
                Array.Copy(marked, markedCopy, scopeX * scopeY);
                string lineCopy = new string(line);

                ret.AddRange(Search(coord.Item1, coord.Item2, markedCopy, lineCopy));
            }

            return ret;
        }

        private List<(int, int)> GetUnmarkedNeighbors(int x, int y, bool[] marked)
        {
            List<(int, int)> ret = new List<(int, int)>();

            // Search for neighbors
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    // Check for boundries
                    if (x + i < 0 || x + i >= scopeX)
                        continue;
                    else if (y + j < 0 || y + j >= scopeY)
                        continue;
                    // and if the position has already been marked before adding the neighbor
                    else if (marked[(x + i) * scopeY + (y + j)])
                        continue;

                    ret.Add((x + i, y + j));
                }
            }

            return ret;
        }

        private eMatchType BinarySearch(int a, int b, string line)
        {
            int flag;

            if (a == b)
            {
                flag = string.Compare(line, dictionary[a]);

                if (flag == 0)
                    return eMatchType.FULL;
                if (flag < 0 && dictionary[a].Contains(line))
                    return eMatchType.PREFIX;

                return eMatchType.INVALID;
            }

            int index = a + (b - a) / 2;
            //Console.WriteLine($"{a} {b} {index} {dictionary[index]} {line}");

            flag = string.Compare(line, dictionary[index]);

            if(flag == -1)
            {
                if (b - a == 1) 
                    return BinarySearch(a, a, line);

                return BinarySearch(a, index, line);
            }
            else if (flag == 0)
                return eMatchType.FULL;
            else if (flag == 1)
            {
                if (b - a == 1) 
                    return BinarySearch(b, b, line);

                return BinarySearch(index, b, line);
            }

            return eMatchType.INVALID;
        }

        // Check to see if the we have a word prefix
        private bool IsWordPrefix(string line)
        {
            return BinarySearch(0, dictionary.Length - 1, line) != eMatchType.INVALID;
        }

        // Check to see if we have a full word
        private bool IsFullWord(string line)
        {
            return BinarySearch(0, dictionary.Length - 1, line) == eMatchType.FULL;
        }
    }
}
