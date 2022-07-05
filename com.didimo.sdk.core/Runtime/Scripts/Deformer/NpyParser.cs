using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Didimo.Utils
{
    /// <summary>
    /// Class to parse .npy files produced by Python's Numpy.
    /// https://numpy.org/doc/stable/reference/generated/numpy.lib.format.html
    /// This is a simplified implementation, so may not be able to parse
    /// some more complicated files/data types. Based on NumSharp (https://github.com/SciSharp/NumSharp).
    /// </summary>
    public class NpyParser
    {
        public struct ArrayInfo
        {
            public Type  DataType;
            public int   DataByteSize;
            public int[] Shape;

            public bool IsLittleEndian;
            public bool IsFortranOrder;

            public int Size => Shape.Aggregate((dim1, dim2) => dim1 * dim2);
        }

        public struct ArrayData
        {
            public ArrayInfo Info;
            public Array     Data;

            public T[] AsArray<T>() => (T[]) Data;
            public string AsString() => Encoding.UTF32.GetString(AsArray<byte>());
        }

        public const string MAGIC_STRING       = "\x93NUMPY";
        public const string HEADER_TYPE_KEY    = "descr";
        public const string HEADER_FORTRAN_KEY = "fortran_order";
        public const string HEADER_SHAPE_KEY   = "shape";

        protected readonly Stream stream;
        protected readonly bool   leaveStreamOpen;

        protected BinaryReader reader;


        public NpyParser(string filepath) : this(File.Open(filepath, FileMode.Open), false) { }

        public NpyParser(Stream dataStream, bool leaveOpen = true)
        {
            stream = dataStream;
            leaveStreamOpen = leaveOpen;
            reader = new BinaryReader(stream, Encoding.ASCII, true);

        }

        ~NpyParser()
        {
            if (!leaveStreamOpen) stream.Dispose();
            reader.Dispose();
        }

        public ArrayData Parse()
        {
            // The first 6 bytes are a magic string: exactly \x93NUMPY.
            if (MAGIC_STRING.Any(c => reader.ReadByte() != c))
                throw new Exception($"Could not find expected magic string '{MAGIC_STRING}' at the beginning of the file.");

            // The next 2 bytes are the major and minor versions.
            int majorVersion = reader.ReadByte();
            int minorVersion = reader.ReadByte();
            // Debug.Log($"Version: {majorVersion}.{minorVersion}");

            // Version 1: The next 2 bytes form a little-endian unsigned short int
            // Version 2+: The next 4 bytes form a little-endian unsigned int
            uint headerSize = majorVersion == 1 ? reader.ReadUInt16() : reader.ReadUInt32();

            // Version 3+: Read header as UTF-8 instead of ASCII.
            if (majorVersion > 2)
            {
                reader.Dispose();
                reader = new BinaryReader(stream, Encoding.UTF8, true);
            }

            string header = new (reader.ReadChars((int) headerSize));
            // Debug.Log($"Header: {header}");

            // Parse Array info in header (python string-like dict)
            ArrayInfo arrayInfo = ParseArrayInfo(header);
            /*
            Debug.Log($"Array Type {arrayInfo.DataType}\n" +
                      $"      LittleEndian: {arrayInfo.IsLittleEndian}\n" +
                      $"      RowMajor: {arrayInfo.IsRowMajor}\n" +
                      $"      Shape: {string.Join(", ", arrayInfo.Shape)}\n" +
                      $"      Byte Size: {arrayInfo.DataByteSize}");
            //*/


            // Read data into Array
            Array dataArray = ReadArrayData(arrayInfo);
            return new ArrayData {Info = arrayInfo, Data = dataArray};
        }

        protected Array ReadArrayData(ArrayInfo info)
        {
            int dataSize = info.Size;
            int dataByteSize = dataSize * info.DataByteSize;
            // Strings need special treatment. e.g.: <U10 = 10 letters of 4 bytes each
            Type dataType = info.DataType == typeof(string) ? typeof(byte) : info.DataType;

            // Read remaining data into a buffer
            byte[] dataBuffer = new byte[dataByteSize];
            int bytesRead = reader.Read(dataBuffer, 0, dataByteSize);
            if (dataByteSize != bytesRead)
                throw new Exception($"Read less bytes than expected. Expected {dataByteSize} but instead read {bytesRead} bytes.");

            // Create generic type array and copy data into it
            Array dataArray = Array.CreateInstance(dataType, info.DataType == typeof(string) ? dataByteSize : dataSize);
            Buffer.BlockCopy(dataBuffer, 0, dataArray, 0, dataByteSize);

            return dataArray;
        }


        protected static ArrayInfo ParseArrayInfo(string header)
        {
            string typeString = GetTypeString(header);
            bool isLittleEndian = IsDataLittleEndian(typeString);
            (Type dataType, int dataByteSize) = GetType(typeString);

            bool isFortranOrder = IsDataFortranOrder(header);
            int[] shape = GetDataShape(header);

            // Strings need special treatment. e.g.: <U10 = 10 letters of 4 bytes each
            if (dataType == typeof(string))
            {
                shape = new int[] {dataByteSize};
                dataByteSize = 4;
            }

            return new ArrayInfo
            {
                DataType = dataType,
                DataByteSize = dataByteSize,
                IsLittleEndian = isLittleEndian,
                IsFortranOrder = isFortranOrder,
                Shape = shape
            };
        }

        protected static string GetTypeString(string header)
        {
            int typeKeyIndex = header.IndexOf(HEADER_TYPE_KEY, StringComparison.InvariantCulture);
            int valueStartIndex = header.IndexOf('\'', typeKeyIndex + HEADER_TYPE_KEY.Length + 2) + 1;
            int valueEndIndex = header.IndexOf('\'', valueStartIndex);
            return header.Substring(valueStartIndex, valueEndIndex - valueStartIndex);
        }

        protected static bool IsDataLittleEndian(string typeString)
        {
            return typeString[0] switch
            {
                '>' => false,
                '!' => false,
                '<' => true,
                _   => throw new Exception("Unknown endianness")
            };
        }

        // https://numpy.org/doc/stable/reference/generated/numpy.dtype.kind.html
        protected static (Type type, int size) GetType(string typeString)
        {
            int byteSize = int.Parse(typeString[2..]);
            string dataTypeString = typeString[1..];

            // Strings need special treatment. e.g.: <U10 = 10 letters of 4 bytes each
            if (dataTypeString.StartsWith("U")) return (typeof(string), byteSize);

            // Native types
            switch (dataTypeString)
            {
                case "b1":
                    return (typeof(bool), byteSize);
                case "i1":
                    return (typeof(sbyte), byteSize);
                case "i2":
                    return (typeof(short), byteSize);
                case "i4":
                    return (typeof(int), byteSize);
                case "i8":
                    return (typeof(long), byteSize);
                case "u1":
                    return (typeof(byte), byteSize);
                case "u2":
                    return (typeof(ushort), byteSize);
                case "u4":
                    return (typeof(uint), byteSize);
                case "u8":
                    return (typeof(ulong), byteSize);
                case "f2":
                    throw new Exception("float16/Half type is not supported");
                case "f4":
                    return (typeof(float), byteSize);
                case "f8":
                    return (typeof(double), byteSize);
                default:
                    throw new Exception($"Unsupported/Unknown data type {typeString[1..]}");
            }
        }


        protected static bool IsDataFortranOrder(string header)
        {
            int fortanKeyIndex = header.IndexOf(HEADER_FORTRAN_KEY, StringComparison.InvariantCulture);
            int fortranValueStartIndex = header.IndexOfAny(new[] {'F', 'T'}, fortanKeyIndex + HEADER_FORTRAN_KEY.Length + 2);
            if (fortranValueStartIndex == -1) throw new Exception("Unexpected Fortran Order");
            return header[fortranValueStartIndex] == 'T';
        }

        protected static int[] GetDataShape(string header)
        {
            // Shape is a tuple such as (), (2,), (2,3), (2,3,4), ...
            int shapeKeyIndex = header.IndexOf(HEADER_SHAPE_KEY, StringComparison.InvariantCulture);
            int shapeValueStartIndex = header.IndexOf('(', shapeKeyIndex + HEADER_SHAPE_KEY.Length + 2);
            int shapeValueEndIndex = header.IndexOf(')', shapeValueStartIndex + 1);

            string shapeSubstring = header.Substring(shapeValueStartIndex + 1, shapeValueEndIndex - shapeValueStartIndex - 1);
            int[] shape =  shapeSubstring.Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToArray();

            // () can be cast as a 1 sized array instead of scalar. Makes things easier
            if (shape.Length == 0) return new[] {1};
            return shape;
        }
    }
}