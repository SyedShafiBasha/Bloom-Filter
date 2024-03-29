﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    class Program
    {
        const int MAX = 150;
        private const int capacity = 100;//1000000; // Capacity per filter - 10 Million
        static Filter<string>[] filters = new Filter<string>[MAX]; // List of all filters
        public const string fileName = @"C:\Syed\POC\emails.csv";
        public const string serializedMails = @"C:\Syed\POC\emails2.csv";
        static void Main(string[] args)
        {
            for (int i = 0; i < MAX; i++) filters[i] = new Filter<string>(capacity);

            // Setup --
            Prepare();
            SerializeObject(filters, @"C:\Syed\Files\emails.bin");
            DeSerializeObject(@"C:\Syed\Files\emails.bin");
            //Serialize and store filters
            //  Serialize();
            // Runtime --
            // Deserialize & load filters
            Console.Write(HasKey("abcd@def.com"));
        }

        //public static void Serialize()
        //{
        //    //Hashtable addresses = new Hashtable();
        //    //addresses.Add("Jeff", "123 Main Street, Redmond, WA 98052");
        //    //addresses.Add("Fred", "987 Pine Road, Phila., PA 19116");
        //    //addresses.Add("Mary", "PO Box 112233, Palo Alto, CA 94301");
        //    IFormatter formatter = new BinaryFormatter();
        //    Stream stream = new FileStream(serializedMails, FileMode.Create, FileAccess.Write);

        //    using (StreamReader sr = File.OpenText(fileName))
        //    {
        //        string s = "";
        //        while ((s = sr.ReadLine()) != null)
        //        {
        //            Console.WriteLine(s);
        //            formatter.Serialize(stream, s);
        //        }
        //    }

        //    //FileStream fs = new FileStream(@"C:\Syed\POC\emails_Serialize.csv", FileMode.Create);

        //    //// Construct a BinaryFormatter and use it to serialize the data to the stream.
        //    //BinaryFormatter formatter = new BinaryFormatter();
        //    //try
        //    //{
        //    //    formatter.Serialize(fs, addresses);
        //    //}
        //    //catch (SerializationException e)
        //    //{
        //    //    Console.WriteLine("Failed to serialize. Reason: " + e.Message);
        //    //    throw;
        //    //}
        //    //finally
        //    //{
        //    //    fs.Close();
        //    //}
        //}

        //public static void Deserialize()
        //{
        //    // Declare the hashtable reference.
        //    Hashtable addresses = null;

        //    // Open the file containing the data that you want to deserialize.
        //    FileStream fs = new FileStream(@"C:\Syed\POC\DataFile.dat", FileMode.Open);
        //    try
        //    {
        //        BinaryFormatter formatter = new BinaryFormatter();

        //        // Deserialize the hashtable from the file and 
        //        // assign the reference to the local variable.
        //        addresses = (Hashtable)formatter.Deserialize(fs);
        //    }
        //    catch (SerializationException e)
        //    {
        //        Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
        //        throw;
        //    }
        //    finally
        //    {
        //        fs.Close();
        //    }

        //    // To prove that the table deserialized correctly, 
        //    // display the key/value pairs.
        //    foreach (DictionaryEntry de in addresses)
        //    {
        //        Console.WriteLine("{0} lives at {1}.", de.Key, de.Value);
        //    }
        //}



        public static void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }
            BinaryFormatter serializer = new BinaryFormatter();
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, serializableObject);
            }
        }
        public static void DeSerializeObject(string fileName)
        {
           // if (string.IsNullOrEmpty(fileName)) { return default(); }
            BinaryFormatter serializer = new BinaryFormatter();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                serializer.Deserialize(stream);
            }
        }

        public static void Prepare()
        {
            var mails = File.ReadAllLines(@"C:\Syed\emails.csv");
            foreach (var mail in mails)
                filters[GetHash(mail)].Add(mail);
        }

        private static bool HasKey(string mail)
        {
            return filters[GetHash(mail)].Contains(mail);
        }

        private static int GetHash(string toHash)
        {
            var hash = BitConverter.ToInt32(Encoding.UTF8.GetBytes(toHash), 0) % MAX;
            return hash;
        }

        [Serializable]
        public class Filter<T>
        {
            /// <summary>
            /// A function that can be used to hash input.
            /// </summary>
            /// <param name="input">The values to be hashed.</param>
            /// <returns>The resulting hash code.</returns>
            public delegate int HashFunction(T input);

            /// <summary>
            /// Creates a new Bloom filter, specifying an error rate of 1/capacity, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
            /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
            /// </summary>
            /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
            public Filter(int capacity) : this(capacity, null) { }

            /// <summary>
            /// Creates a new Bloom filter, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
            /// A secondary hash function will be provided for you if your type T is either string or int. Otherwise an exception will be thrown. If you are not using these types please use the overload that supports custom hash functions.
            /// </summary>
            /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
            /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
            public Filter(int capacity, int errorRate) : this(capacity, errorRate, null) { }

            /// <summary>
            /// Creates a new Bloom filter, specifying an error rate of 1/capacity, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
            /// </summary>
            /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
            /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
            public Filter(int capacity, HashFunction hashFunction) : this(capacity, bestErrorRate(capacity), hashFunction) { }

            /// <summary>
            /// Creates a new Bloom filter, using the optimal size for the underlying data structure based on the desired capacity and error rate, as well as the optimal number of hash functions.
            /// </summary>
            /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
            /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
            /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
            public Filter(int capacity, float errorRate, HashFunction hashFunction) : this(capacity, errorRate, hashFunction, bestM(capacity, errorRate), bestK(capacity, errorRate)) { }

            /// <summary>
            /// Creates a new Bloom filter.
            /// </summary>
            /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items can be added, but the error rate will exceed what is expected.</param>
            /// <param name="errorRate">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
            /// <param name="hashFunction">The function to hash the input values. Do not use GetHashCode(). If it is null, and T is string or int a hash function will be provided for you.</param>
            /// <param name="m">The number of elements in the BitArray.</param>
            /// <param name="k">The number of hash functions to use.</param>
            public Filter(int capacity, float errorRate, HashFunction hashFunction, int m, int k)
            {
                // validate the params are in range
                if (capacity < 1)
                    throw new ArgumentOutOfRangeException("capacity", capacity, "capacity must be > 0");
                if (errorRate >= 1 || errorRate <= 0)
                    throw new ArgumentOutOfRangeException("errorRate", errorRate, String.Format("errorRate must be between 0 and 1, exclusive. Was {0}", errorRate));
                if (m < 1) // from overflow in bestM calculation
                    throw new ArgumentOutOfRangeException(String.Format("The provided capacity and errorRate values would result in an array of length > int.MaxValue. Please reduce either of these values. Capacity: {0}, Error rate: {1}", capacity, errorRate));

                // set the secondary hash function
                if (hashFunction == null)
                {
                    if (typeof(T) == typeof(String))
                    {
                        getHashSecondary = hashString;
                    }
                    else if (typeof(T) == typeof(int))
                    {
                        getHashSecondary = hashInt32;
                    }
                    else
                    {
                        throw new ArgumentNullException("hashFunction", "Please provide a hash function for your type T, when T is not a string or int.");
                    }
                }
                else
                    getHashSecondary = hashFunction;

                hashFunctionCount = k;
                hashBits = new BitArray(m);
            }

            /// <summary>
            /// Adds a new item to the filter. It cannot be removed.
            /// </summary>
            /// <param name="item"></param>
            public void Add(T item)
            {
                // start flipping bits for each hash of item
                int primaryHash = item.GetHashCode();
                int secondaryHash = getHashSecondary(item);
                for (int i = 0; i < hashFunctionCount; i++)
                {
                    int hash = computeHash(primaryHash, secondaryHash, i);
                    hashBits[hash] = true;
                }
            }

            /// <summary>
            /// Checks for the existance of the item in the filter for a given probability.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool Contains(T item)
            {
                int primaryHash = item.GetHashCode();
                int secondaryHash = getHashSecondary(item);
                for (int i = 0; i < hashFunctionCount; i++)
                {
                    int hash = computeHash(primaryHash, secondaryHash, i);
                    if (hashBits[hash] == false)
                        return false;
                }
                return true;
            }

            /// <summary>
            /// The ratio of false to true bits in the filter. E.g., 1 true bit in a 10 bit filter means a truthiness of 0.1.
            /// </summary>
            public double Truthiness
            {
                get
                {
                    return (double)trueBits() / hashBits.Count;
                }
            }

            private int trueBits()
            {
                int output = 0;
                foreach (bool bit in hashBits)
                {
                    if (bit == true)
                        output++;
                }
                return output;
            }

            /// <summary>
            /// Performs Dillinger and Manolios double hashing. 
            /// </summary>
            private int computeHash(int primaryHash, int secondaryHash, int i)
            {
                int resultingHash = (primaryHash + (i * secondaryHash)) % hashBits.Count;
                return Math.Abs((int)resultingHash);
            }

            private int hashFunctionCount;
            private BitArray hashBits;
            private HashFunction getHashSecondary;

            private static int bestK(int capacity, float errorRate)
            {
                return (int)Math.Round(Math.Log(2.0) * bestM(capacity, errorRate) / capacity);
            }

            private static int bestM(int capacity, float errorRate)
            {
                return (int)Math.Ceiling(capacity * Math.Log(errorRate, (1.0 / Math.Pow(2, Math.Log(2.0)))));
            }

            private static float bestErrorRate(int capacity)
            {
                float c = (float)(1.0 / capacity);
                if (c != 0)
                    return c;
                else
                    return (float)Math.Pow(0.6185, int.MaxValue / capacity); // http://www.cs.princeton.edu/courses/archive/spring02/cs493/lec7.pdf
            }

            /// <summary>
            /// Hashes a 32-bit signed int using Thomas Wang's method v3.1 (http://www.concentric.net/~Ttwang/tech/inthash.htm).
            /// Runtime is suggested to be 11 cycles. 
            /// </summary>
            /// <param name="input">The integer to hash.</param>
            /// <returns>The hashed result.</returns>
            private static int hashInt32(T input)
            {
                uint? x = input as uint?;
                unchecked
                {
                    x = ~x + (x << 15); // x = (x << 15) - x- 1, as (~x) + y is equivalent to y - x - 1 in two's complement representation
                    x = x ^ (x >> 12);
                    x = x + (x << 2);
                    x = x ^ (x >> 4);
                    x = x * 2057; // x = (x + (x << 3)) + (x<< 11);
                    x = x ^ (x >> 16);
                    return (int)x;
                }
            }

            /// <summary>
            /// Hashes a string using Bob Jenkin's "One At A Time" method from Dr. Dobbs (http://burtleburtle.net/bob/hash/doobs.html).
            /// Runtime is suggested to be 9x+9, where x = input.Length. 
            /// </summary>
            /// <param name="input">The string to hash.</param>
            /// <returns>The hashed result.</returns>
            private static int hashString(T input)
            {
                string s = input as string;
                int hash = 0;

                for (int i = 0; i < s.Length; i++)
                {
                    hash += s[i];
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }
                hash += (hash << 3);
                hash ^= (hash >> 11);
                hash += (hash << 15);
                return hash;
            }
        }
    }
}
