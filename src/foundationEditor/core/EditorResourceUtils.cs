using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace foundationEditor
{
    public class EditorResourceUtils
    {
        public static Texture2D LoadTextureFromDll(string v, int width, int height)
        {
            Texture2D textured;
            textured = new Texture2D(width, height);
            Assembly executingAssembly = Assembly.GetExecutingAssembly();

            String projectName = Assembly.GetExecutingAssembly().GetName().Name.ToString();
            Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(projectName+".Resources." + v + ".png");
            if (manifestResourceStream == null)
            {
                manifestResourceStream = executingAssembly.GetManifestResourceStream(projectName+".Resources." + v + ".jpg");
            }
            if (manifestResourceStream != null)
            {
                textured.LoadImage(ReadToEnd(manifestResourceStream));
            }

            textured.hideFlags = HideFlags.HideAndDontSave;

            return textured;
        }

        public static Texture2D LoadTexture2D(string v)
        {
            return LoadTextureFromDll(v, 1, 1);
        }

        private static byte[] ReadToEnd(Stream stream)
        {
            byte[] buffer4;
            long position = stream.Position;
            stream.Position = 0L;
            try
            {
                int num3;
                byte[] buffer = new byte[0x1000];
                int offset = 0;
                while ((num3 = stream.Read(buffer, offset, buffer.Length - offset)) > 0)
                {
                    offset += num3;
                    if (offset == buffer.Length)
                    {
                        int num4 = stream.ReadByte();
                        if (num4 != -1)
                        {
                            byte[] buffer2 = new byte[buffer.Length * 2];
                            Buffer.BlockCopy(buffer, 0, buffer2, 0, buffer.Length);
                            Buffer.SetByte(buffer2, offset, (byte)num4);
                            buffer = buffer2;
                            offset++;
                        }
                    }
                }
                byte[] dst = buffer;
                if (buffer.Length != offset)
                {
                    dst = new byte[offset];
                    Buffer.BlockCopy(buffer, 0, dst, 0, offset);
                }
                buffer4 = dst;
            }
            finally
            {
                stream.Position = position;
            }
            return buffer4;
        }
    }
}