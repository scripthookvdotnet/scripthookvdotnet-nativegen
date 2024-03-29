﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using NetJSON;

namespace NativeGen
{
	public static class Application
	{
		public struct Options
		{
			public bool OutputGuessedNames;
			public bool AppendJoaatHash;
			public bool Verbose;
		}

		public static Options ConfigureOptions(string[] optionArgs)
		{
			Options result = new Options();
			foreach (string arg in optionArgs)
			{
				if (arg == "-g" || arg == "--output-guessed-names")
				{
					result.OutputGuessedNames = true;
				}
				if (arg == "-j" || arg == "--append-joaat-hash")
				{
					result.AppendJoaatHash = true;
				}
				if (arg == "-v" || arg == "--verbose")
				{
					result.Verbose = true;
				}
			}

			return result;
		}

		public static void Main(string[] args)
		{
			string inputJsonUrl = "https://github.com/alloc8or/gta5-nativedb-data/blob/master/natives.json?raw=true";
			string outputFile = "NativeHashes.txt";

			int argsLength = args.Length;
			if (argsLength == 1)
			{
				outputFile = args[0];
			}
			if (argsLength >= 2)
			{
				inputJsonUrl = args[0];
				outputFile = args[1];
			}

			Options options = (argsLength >= 3)
				? ConfigureOptions(args.SubArray(2, argsLength - 2))
				: new Options();

			using (var wc = new WebClient())
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

				LogToConsoleIfVerbose("Downloading natives.json", options.Verbose);
				wc.Headers.Add("Accept-Encoding: gzip, deflate, sdch");

				string nativeFileRaw = Decompress(wc.DownloadData(inputJsonUrl));
				string nativeTemplate = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NativeTemplate.txt"));

				NativeFile nativeFile = NetJSON.NetJSON.Deserialize<NativeFile>(nativeFileRaw);
				StringBuilder resultBuilder = new StringBuilder();

				foreach (string nativeNamespaceKey in nativeFile.Keys)
				{
					LogToConsoleIfVerbose($"Processing {nativeNamespaceKey}", options.Verbose);
					NativeNamespace nativeNamespace = nativeFile[nativeNamespaceKey];

					resultBuilder.AppendLine("\t\t\t/*");
					resultBuilder.AppendLine("\t\t\t\t" + nativeNamespaceKey);
					resultBuilder.AppendLine("\t\t\t*/");

					foreach (string nativeFuncKey in nativeNamespace.Keys)
					{
						NativeFunction nativeFunction = nativeNamespace[nativeFuncKey];

						string nativeFunctionName = nativeFunction.Name;
						if (string.IsNullOrEmpty(nativeFunctionName))
						{
							continue;
						}
						if (!options.OutputGuessedNames && nativeFunctionName.StartsWith("_", StringComparison.Ordinal))
						{
							continue;
						}

						if (options.AppendJoaatHash)
						{
							string substringForJoaatHashComment = !string.IsNullOrEmpty(nativeFunction.JHash) ? $" // {nativeFunction.JHash}" : string.Empty;
							resultBuilder.AppendLine($"\t\t\t{nativeFunctionName} = {nativeFuncKey},{substringForJoaatHashComment}");
						}
						else
						{
							resultBuilder.AppendLine($"\t\t\t{nativeFunctionName} = {nativeFuncKey},");
						}
					}
				}

				File.WriteAllText(outputFile, string.Format(nativeTemplate, resultBuilder));

				LogToConsoleIfVerbose("Finished generating native hash enum", options.Verbose);
			}
		}

		public static void LogToConsoleIfVerbose(string input, bool log = false)
		{
			if (log)
			{
				Console.WriteLine(input);
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
					}
					while (count > 0);

					return Encoding.UTF8.GetString(memory.ToArray());
				}
			}
		}
	}

	public static class ArrayExtensions
	{
		public static T[] SubArray<T>(this T[] data, int index, int length)
		{
			T[] result = new T[length];
			Array.Copy(data, index, result, 0, length);
			return result;
		}
	}
}
