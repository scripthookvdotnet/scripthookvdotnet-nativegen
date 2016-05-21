using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace scripthookvdotnet_nativegen
{
    internal static class NativeGenerator
    {
        private static void Main(string[] args)
        {
            using (WebClient wc = new WebClient())
            {
                Console.WriteLine("Downloading natives.json");
                wc.Headers.Add("Accept-Encoding: gzip, deflate, sdch");

                string nativeFileRaw = Decompress(wc.DownloadData("http://www.dev-c.com/nativedb/natives.json"));
                string nativeTemplate = File.ReadAllText("../../nativeTemplate.txt");

                NativeFile nativeFile = JsonConvert.DeserializeObject<NativeFile>(nativeFileRaw);
                StringBuilder resultBuilder = new StringBuilder();

                foreach (string nativeNamespaceKey in nativeFile.Keys)
                {
                    Console.WriteLine("Processing " + nativeNamespaceKey);
                    NativeNamespace nativeNamespace = nativeFile[nativeNamespaceKey];

                    resultBuilder.AppendLine("			/*");
                    resultBuilder.AppendLine("				" + nativeNamespaceKey);
                    resultBuilder.AppendLine("			*/");

                    foreach (string nativeFuncKey in nativeNamespace.Keys)
                    {
                        NativeFunction nativeFunction = nativeNamespace[nativeFuncKey];
                        if (!string.IsNullOrEmpty(nativeFunction.Name))
                        {
                            resultBuilder.AppendLine("			" + nativeFunction.Name + " = " + nativeFuncKey + ", // " +
                                                     nativeFunction.JHash);
                        }
                    }
                }

                File.WriteAllText("NativeHashes.hpp", string.Format(nativeTemplate, resultBuilder));

                Console.WriteLine("NativeHashes.hpp created (Press any key to exit)");
                Console.ReadKey();
            }
        }

        /// <summary>
        ///     This method is used to decompress the received GZIP-String
        /// </summary>
        /// <param name="gzip">compressed input</param>
        /// <returns>UTF-8 encoded and decompressed string</returns>
        private static string Decompress(byte[] gzip)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                byte[] buffer = new byte[gzip.Length];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, gzip.Length);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return Encoding.UTF8.GetString(memory.ToArray());
                }
            }
        }
    }
}