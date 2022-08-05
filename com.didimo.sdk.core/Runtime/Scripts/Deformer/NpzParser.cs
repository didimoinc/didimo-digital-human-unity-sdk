using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Didimo.Core.Deformer;
using UnityEngine;
using ArrayData = Didimo.Utils.NpyParser.ArrayData;


namespace Didimo.Utils
{
    /// <summary>
    /// Class to parse .npz files produced by Python's Numpy.
    /// https://numpy.org/doc/stable/reference/generated/numpy.lib.format.html
    /// This is a zip file that contains multiple .npy files
    /// </summary>
    public class NpzParser
    {
        private readonly byte[] fileBytes;

        public static byte[] RemoveAPIHeader(byte[] fileBytes)
        {
            byte[] signatureZip = { 0x50, 0x4b, 0x03, 0x04 };
            const int signatureSize = 4;

            if (fileBytes.Length < signatureSize)
                return fileBytes;

            byte[] signature = fileBytes.Take(4).ToArray();

            if (signature.SequenceEqual(signatureZip))
            {
                // Its a zip file (npz), so just return it.
                return fileBytes;
            }
            // Its not a zip file. Check if the file is a Didimo API DMX file.

            byte[] signatureDmx = { 0x44, 0x4d, 0x58 };
            byte[] apiHeaderFirstValye = fileBytes.Take(Array.IndexOf<byte>(fileBytes, 0x00, 0)).ToArray();

            if (!apiHeaderFirstValye.SequenceEqual(signatureDmx))
            {
                Debug.LogError("Unsupported deformation file.");
                return null;
            }

            // If it is, remove the header.
            // The header is comprised of 3 strings, separated by the byte 0x00
            int startIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                int idx = Array.IndexOf<byte>(fileBytes, 0x00, startIndex);

                if (idx == -1)
                {
                    Debug.LogError("Deformation file was not a zip, but we failed to remove API header from it.");
                    return null;
                }

                startIndex = idx + 1;
            }

            return fileBytes.TakeLast(fileBytes.Length - startIndex).ToArray();
        }
        
        public NpzParser(string filepath) : this(File.ReadAllBytes(filepath)) { }

        public NpzParser(byte[] filebytes)
        {
            fileBytes = RemoveAPIHeader(filebytes);
        }

        public NpzParser(TextAsset byteAsset) : this(ByteAsset.ToByteArray(byteAsset)) { }

        public Dictionary<string, ArrayData> Parse()
        {
            Dictionary<string, ArrayData> result = new();
            using MemoryStream stream = new(fileBytes, false);
            ZipArchive zip = new(stream, ZipArchiveMode.Read);
            foreach (ZipArchiveEntry zipEntry in zip.Entries)
            {
                try
                {
                    using Stream npyStream = zipEntry.Open();
                    NpyParser npyParser = new(npyStream);
                    ArrayData arrayData = npyParser.Parse();
                    result[zipEntry.Name] = arrayData;
                }
                catch (Exception e)
                {
                    // Some NPY files can fail to parse due to reasons like unsupported format (e.g.: strings)
                    Debug.LogWarning($"Unable to parse file {zipEntry.Name}. Exception: {e}");
                }
            }
            return result;
        }
    }
}