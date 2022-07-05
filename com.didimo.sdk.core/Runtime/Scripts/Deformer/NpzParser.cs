using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        protected readonly Stream stream;
        protected readonly bool leaveStreamOpen;

        public NpzParser(string filepath) : this(File.Open(filepath, FileMode.Open), false) { }

        public NpzParser(byte[] filebytes) : this(new MemoryStream(filebytes, false), false) {  }

        public NpzParser(TextAsset byteAsset) : this(ByteAsset.ToByteArray(byteAsset)) { }

        public NpzParser(Stream dataStream, bool leaveOpen = true)
        {
            stream = dataStream;
            leaveStreamOpen = leaveOpen;
        }

        ~NpzParser()
        {
            if (!leaveStreamOpen) stream.Dispose();
        }

        public Dictionary<string, ArrayData> Parse()
        {
            Dictionary<string, ArrayData> result = new Dictionary<string, ArrayData>();
            ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read);
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