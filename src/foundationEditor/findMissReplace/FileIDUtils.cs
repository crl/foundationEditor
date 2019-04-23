using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace foundationEditor
{
    public class MD4 : HashAlgorithm
    {
        private uint _a;
        private uint _b;
        private uint _c;
        private uint _d;
        private uint[] _x;
        private int _bytesProcessed;

        public MD4()
        {
            _x = new uint[16];

            Initialize();
        }

        public override void Initialize()
        {
            _a = 0x67452301;
            _b = 0xefcdab89;
            _c = 0x98badcfe;
            _d = 0x10325476;

            _bytesProcessed = 0;
        }

        protected override void HashCore(byte[] array, int offset, int length)
        {
            ProcessMessage(Bytes(array, offset, length));
        }

        protected override byte[] HashFinal()
        {
            try
            {
                ProcessMessage(Padding());

                return new[] {_a, _b, _c, _d}.SelectMany(word => Bytes(word)).ToArray();
            }
            finally
            {
                Initialize();
            }
        }

        private void ProcessMessage(IEnumerable<byte> bytes)
        {
            foreach (byte b in bytes)
            {
                int c = _bytesProcessed & 63;
                int i = c >> 2;
                int s = (c & 3) << 3;

                _x[i] = (_x[i] & ~((uint) 255 << s)) | ((uint) b << s);

                if (c == 63)
                {
                    Process16WordBlock();
                }

                _bytesProcessed++;
            }
        }

        private static IEnumerable<byte> Bytes(byte[] bytes, int offset, int length)
        {
            for (int i = offset; i < length; i++)
            {
                yield return bytes[i];
            }
        }

        private IEnumerable<byte> Bytes(uint word)
        {
            yield return (byte) (word & 255);
            yield return (byte) ((word >> 8) & 255);
            yield return (byte) ((word >> 16) & 255);
            yield return (byte) ((word >> 24) & 255);
        }

        private IEnumerable<byte> Repeat(byte value, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return value;
            }
        }

        private IEnumerable<byte> Padding()
        {
            return Repeat(128, 1)
                .Concat(Repeat(0, ((_bytesProcessed + 8) & 0x7fffffc0) + 55 - _bytesProcessed))
                .Concat(Bytes((uint) _bytesProcessed << 3))
                .Concat(Repeat(0, 4));
        }

        private void Process16WordBlock()
        {
            uint aa = _a;
            uint bb = _b;
            uint cc = _c;
            uint dd = _d;

            foreach (int k in new[] {0, 4, 8, 12})
            {
                aa = Round1Operation(aa, bb, cc, dd, _x[k], 3);
                dd = Round1Operation(dd, aa, bb, cc, _x[k + 1], 7);
                cc = Round1Operation(cc, dd, aa, bb, _x[k + 2], 11);
                bb = Round1Operation(bb, cc, dd, aa, _x[k + 3], 19);
            }

            foreach (int k in new[] {0, 1, 2, 3})
            {
                aa = Round2Operation(aa, bb, cc, dd, _x[k], 3);
                dd = Round2Operation(dd, aa, bb, cc, _x[k + 4], 5);
                cc = Round2Operation(cc, dd, aa, bb, _x[k + 8], 9);
                bb = Round2Operation(bb, cc, dd, aa, _x[k + 12], 13);
            }

            foreach (int k in new[] {0, 2, 1, 3})
            {
                aa = Round3Operation(aa, bb, cc, dd, _x[k], 3);
                dd = Round3Operation(dd, aa, bb, cc, _x[k + 8], 9);
                cc = Round3Operation(cc, dd, aa, bb, _x[k + 4], 11);
                bb = Round3Operation(bb, cc, dd, aa, _x[k + 12], 15);
            }

            unchecked
            {
                _a += aa;
                _b += bb;
                _c += cc;
                _d += dd;
            }
        }

        private static uint ROL(uint value, int numberOfBits)
        {
            return (value << numberOfBits) | (value >> (32 - numberOfBits));
        }

        private static uint Round1Operation(uint a, uint b, uint c, uint d, uint xk, int s)
        {
            unchecked
            {
                return ROL(a + ((b & c) | (~b & d)) + xk, s);
            }
        }

        private static uint Round2Operation(uint a, uint b, uint c, uint d, uint xk, int s)
        {
            unchecked
            {
                return ROL(a + ((b & c) | (b & d) | (c & d)) + xk + 0x5a827999, s);
            }
        }

        private static uint Round3Operation(uint a, uint b, uint c, uint d, uint xk, int s)
        {
            unchecked
            {
                return ROL(a + (b ^ c ^ d) + xk + 0x6ed9eba1, s);
            }
        }
    }

    public static class FileIDUtil
    {
        public static int Compute(Type t)
        {
            //在dll文件里面的hash fileID;
            string toBeHashed = "s\0\0\0" + t.Namespace + t.Name;

            using (HashAlgorithm hash = new MD4())
            {
                byte[] hashed = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(toBeHashed));

                int result = 0;

                for (int i = 3; i >= 0; --i)
                {
                    result <<= 8;
                    result |= hashed[i];
                }

                return result;
            }
        }

        public static Dictionary<int, string> hashTypeDictionary = new Dictionary<int, string>();
        private static Dictionary<string, Assembly> assemblyList = new Dictionary<string, Assembly>();
        private static Dictionary<string, string> routerMapping = new Dictionary<string, string>();

        public static Dictionary<int, string> getAllFileIDByDll(string assemblyPath)
        {
            if (assemblyList.ContainsKey(assemblyPath))
            {
                return null;
            }
            UnityEngine.Object[] typsList = AssetDatabase.LoadAllAssetsAtPath(assemblyPath);
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(assemblyPath);
            }
            catch (Exception)
            {
                //Debug.Log("getAllFileIDByDll:" + assemblyPath + " error:" + ex.Message);
                return hashTypeDictionary;
            }

            foreach (UnityEngine.Object item in typsList)
            {
                Type type = assembly.GetType(item.name);
                if (type == null)
                {
                    //Debug.LogWarning(item.name + ":不在定义中");
                    continue;
                }
                int hash = Compute(type);

                if (hashTypeDictionary.ContainsKey(hash) == false)
                {
                    hashTypeDictionary.Add(hash, item.name);
                }
            }

            assemblyList.Add(assemblyPath, assembly);

            return hashTypeDictionary;
        }

        public static Type getTypeByName(string name, out string dllpath)
        {
            if (string.IsNullOrEmpty(name))
            {
                dllpath = null;
                return null;
            }

            Assembly assembly;
            foreach (string assemblyPath in assemblyList.Keys)
            {
                assembly = assemblyList[assemblyPath];
                Type type = assembly.GetType(name);
                if (type != null)
                {
                    dllpath = assemblyPath;
                    return type;
                }
            }
            dllpath = null;
            return null;
        }

        public static string getFileNameByFileID(int fileID)
        {
            string value;
            hashTypeDictionary.TryGetValue(fileID, out value);
            return value;
        }

        public static void clearRouter()
        {
            routerMapping.Clear();
        }
       
        public static void registerRouter(string oldPath, string newPath)
        {
            if (routerMapping.ContainsKey(oldPath))
            {
                routerMapping[oldPath] = newPath;
            }
            else
            {
                routerMapping.Add(oldPath, newPath);
            }
        }

        public static string getRouter(string path)
        {
            string newPath;
            if (routerMapping.TryGetValue(path, out newPath))
            {
                return newPath;
            }

            return path;
        }
    }
}