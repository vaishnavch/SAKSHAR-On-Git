using System;
using System.Collections.Generic;

namespace Sakshar
{
    class Slide
    {
        string[] items = new string[SlidesGenerator.itemsPerSlide];
        string ans;

        void Shuffle()
        {
            int n = SlidesGenerator.itemsPerAlphabet;
            while (n > 1)
            {
                n--;
                int k = RandomNoGenerator.getNumber(n + 1);
                string value = items[k];
                items[k] = items[n];
                items[n] = value;
            }
        }

        void ShuffleNew()
        {
            for (int i = 0; i < SlidesGenerator.itemsPerSlide; i++)
            {
                int id = RandomNoGenerator.getNumberInRange(i, SlidesGenerator.itemsPerSlide);

                //swap elements
                string temp = items[i];
                items[i] = items[id];
                items[id] = temp;
            }
        }

        public Slide(List<string> chars)
        {
            for (int i = 0; i < SlidesGenerator.itemsPerSlide; i++)
                items[i] = chars[i] + Convert.ToString(1 + RandomNoGenerator.getNumber(SlidesGenerator.itemsPerAlphabet));

            SetAnswer(items[0]);
            ShuffleNew();
        }

        public string[] getAllItems()
        {
            return items;
        }

        void SetAnswer(string ans)
        {
            this.ans = ans;
        }

        public string GetAnswer()
        {
            return ans;
        }
    }

    internal class RandomNoGenerator
    {
        static Random generator = new Random();

        /// <summary>
        /// This returns a random integer number from 0 to miax (max excluded) 
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int getNumber(int max)
        {
            return generator.Next(max);
        }

        /// <summary>
        /// This returns a random integer number from min to max (max excluded) 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int getNumberInRange(int min, int max)
        {
            return generator.Next(min, max);
        }
    }

    class SlidesGenerator
    {
        bool sequential;
        string[] alphabets;
        internal static int itemsPerSlide;
        internal static int itemsPerAlphabet;

        public SlidesGenerator(string[] alphabets, int itemsPerAlphabet, bool sequential = false, int itemsPerSlide = 4)
        {
            this.alphabets = alphabets;
            this.sequential = sequential;
            SlidesGenerator.itemsPerSlide = itemsPerSlide;
            SlidesGenerator.itemsPerAlphabet = itemsPerAlphabet;
        }

        public Slide[] getAllSlides()
        {
            Slide[] allSlides = new Slide[alphabets.Length];
            List<string> ansChars = new List<string>(alphabets.Length);

            for (int index = 0; index < alphabets.Length; index++)
            {
                string firstChar = sequential ? alphabets[index] : getUniqueChar(ansChars);
                List<string> visitedChars = new List<string>(itemsPerSlide);
                visitedChars.Add(firstChar);

                while (visitedChars.Count < itemsPerSlide)
                    visitedChars.Add(getUniqueChar(visitedChars));

                allSlides[index] = new Slide(visitedChars);
                ansChars.Add(firstChar);
            }

            return allSlides;
        }

        // Get unique char from alphabets which is not present in the provided list
        string getUniqueChar(List<string> visitedChars)
        {
            string nextChar;
            while (isCharVisited(nextChar = alphabets[RandomNoGenerator.getNumber(alphabets.Length)], visitedChars))
                return getUniqueChar(visitedChars);

            return nextChar;
        }

        bool isCharVisited(string newChar, List<string> visitedChars)
        {
            return visitedChars.Contains(newChar);
        }
    }
}
