using System;
using System.Text;

namespace DigitalSalmon.DarkestFayte
{
    /*
        MIT License
        
        Copyright (c) 2017 Bolorunduro Winner-Timothy B
        
        Permission is hereby granted, free of charge, to any person obtaining a copy
        of this software and associated documentation files (the "Software"), to deal
        in the Software without restriction, including without limitation the rights
        to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        copies of the Software, and to permit persons to whom the Software is
        furnished to do so, subject to the following conditions:
        
        The above copyright notice and this permission notice shall be included in all
        copies or substantial portions of the Software.
        
        THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
        SOFTWARE.
    */

    public static class ShortId
    {
        private const string UPPERS  = "ABCDEFGHIJKLMNOPQRSTUVWXY";
        private const string LOWERS  = "abcdefghjlkmnopqrstuvwxyz";
        private const string NUMBERS = "0123456789";

        private const string SPECIALS = "-_";

        // app variables
        private static Random _random = new Random();
        private static string _pool   = $"{LOWERS}{UPPERS}";

        // thread management variables
        private static readonly object ThreadLock = new object();

        /// <summary>
        /// Resets the random number generator and character set
        /// </summary>
        public static void Reset()
        {
            lock (ThreadLock)
            {
                _random = new Random();
                _pool = $"{LOWERS}{UPPERS}";
            }
        }

        /// <summary>
        /// Generates a random string of varying length with special characters and without numbers
        /// </summary>
        /// <param name="useNumbers">Whether or not to include numbers</param>
        /// <param name="useSpecial">Whether or not special characters are included</param>
        /// <returns>A random string</returns>
        public static string Generate(bool useNumbers = false, bool useSpecial = true)
        {
            int length = _random.Next(7, 15);
            return Generate(useNumbers, useSpecial, length);
        }

        /// <summary>
        /// Generates a random string of a specified length with the option to add numbers and special characters
        /// </summary>
        /// <param name="useNumbers">Whether or not numbers are included in the string</param>
        /// <param name="useSpecial">Whether or not special characters are included</param>
        /// <param name="length">The length of the generated string</param>
        /// <returns>A random string</returns>
        public static string Generate(bool useNumbers, bool useSpecial, int length)
        {
            if (length < 7)
            {
                throw new ArgumentException($"The specified length of {length} is less than the lower limit of 7.");
            }

            string __pool;
            Random rand;

            lock (ThreadLock)
            {
                __pool = _pool;
                rand = _random;
            }

            StringBuilder poolBuilder = new StringBuilder(__pool);
            if (useNumbers)
            {
                poolBuilder.Append(NUMBERS);
            }

            if (useSpecial)
            {
                poolBuilder.Append(SPECIALS);
            }

            string pool = poolBuilder.ToString();

            char[] output = new char[length];
            for (int i = 0; i < length; i++)
            {
                int charIndex = rand.Next(0, pool.Length);
                output[i] = pool[charIndex];
            }

            return new string(output);
        }

        /// <summary>
        /// Generates a random string of a specified length with special characters and without numbers
        /// </summary>
        /// <param name="length">The length of the generated string</param>
        /// <returns>A random string</returns>
        public static string Generate(int length) => Generate(false, true, length);

        /// <summary>
        /// Changes the character set that id's are generated from
        /// </summary>
        /// <param name="characters">The new character set</param>
        /// <exception cref="InvalidOperationException">Thrown when the new character set is less than 20 characters</exception>
        public static void SetCharacters(string characters)
        {
            if (string.IsNullOrWhiteSpace(characters))
            {
                throw new ArgumentException("The replacement characters must not be null or empty.");
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (char character in characters)
            {
                if (!char.IsWhiteSpace(character))
                {
                    stringBuilder.Append(character);
                }
            }

            if (stringBuilder.Length < 20)
            {
                throw new InvalidOperationException("The replacement characters must be at least 20 letters in length and without whitespace.");
            }

            _pool = stringBuilder.ToString();
        }

        /// <summary>
        /// Sets the seed that the random generator works with.
        /// </summary>
        /// <param name="seed">The seed for the random number generator</param>
        public static void SetSeed(int seed)
        {
            lock (ThreadLock)
            {
                _random = new Random(seed);
            }
        }
    }
}