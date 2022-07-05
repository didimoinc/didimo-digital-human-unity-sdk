using System;
using UnityEngine;


namespace Didimo.Core.Deformer
{
    public class ByteAsset : TextAsset
    {
        public ByteAsset(byte[] data) : base(Convert.ToBase64String(data)) { }
        public static byte[] ToByteArray(TextAsset asset) => Convert.FromBase64String(asset.text);
    }
}