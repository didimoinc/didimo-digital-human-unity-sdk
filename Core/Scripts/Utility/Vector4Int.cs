using System;
using UnityEngine;

namespace DigitalSalmon
{
    [Serializable]
    public struct Vector4Int
    {
        //-----------------------------------------------------------------------------------------
        // Constants:
        //-----------------------------------------------------------------------------------------

        public static readonly Vector4Int Zero   = new Vector4Int(0, 0, 0, 0);
        public static readonly Vector4Int One    = new Vector4Int(1, 1, 1, 1);
        public static readonly Vector4Int NegOne = new Vector4Int(-1, -1, -1, -1);

        public static readonly Vector4Int MinValue = new Vector4Int(int.MinValue, int.MinValue, int.MinValue, int.MinValue);
        public static readonly Vector4Int MaxValue = new Vector4Int(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

        //-----------------------------------------------------------------------------------------
        // Public Fields:
        //-----------------------------------------------------------------------------------------

        // ReSharper disable InconsistentNaming
        public int x;

        public int y;

        public int z;

        public int w;
        // ReSharper restore InconsistentNaming

        //-----------------------------------------------------------------------------------------
        // Public Properties:
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// The North, South, East, and West cardinal directions. (Horizontal & Vertical/ Axials).
        /// </summary>

        public int SqrMagnitude => x * x + y * y + z * z + w * w;

        public float Magnitude => Mathf.Sqrt(SqrMagnitude);

        //-----------------------------------------------------------------------------------------
        // Constructors:
        //-----------------------------------------------------------------------------------------

        public Vector4Int(float x, float y, float z, float w)
        {
            this.x = (int) x;
            this.y = (int) y;
            this.z = (int) z;
            this.w = (int) w;
        }

        public Vector4Int(int x, int y, int z, int w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        //-----------------------------------------------------------------------------------------
        // Public Operators:
        //-----------------------------------------------------------------------------------------

        // Explicit cast to Vector2
        public static explicit operator Vector4(Vector4Int v) => new Vector4(v.x, v.y, v.z, v.w);

        // Addition
        public static Vector4Int operator +(Vector4Int a, Vector4Int b) => new Vector4Int(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

        // Subtraction
        public static Vector4Int operator -(Vector4Int a, Vector4Int b) => new Vector4Int(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);

        // Multiplication
        public static Vector4Int operator *(Vector4Int a, int b) => new Vector4Int(a.x * b, a.y * b, a.z * b, a.w * b);

        // Int Multiplication
        public static Vector4Int operator *(int a, Vector4Int b) => new Vector4Int(a * b.x, a * b.y, a * b.z, a * b.w);

        // Division
        public static Vector4Int operator /(Vector4Int a, int b) => new Vector4Int(a.x / b, a.y / b, a.z / b, a.w / b);

        // Inversion
        public static Vector4Int operator -(Vector4Int a) => new Vector4Int(-a.x, -a.y, -a.z, -a.w);

        // Equality
        public static bool operator ==(Vector4Int a, Vector4Int b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;

        // InEquality
        public static bool operator !=(Vector4Int a, Vector4Int b) => a.x != b.x && a.y != b.y && a.z != b.z && a.w != b.w;

        //-----------------------------------------------------------------------------------------
        // Public Methods:
        //-----------------------------------------------------------------------------------------

        public Vector4 ToVector4() => new Vector4(x, y, z, w);

        public bool IsWithin(Vector4Int min, Vector4Int max) => x >= min.x && x <= max.x && y >= min.y && y <= max.y && z >= min.z && z <= max.z && w >= min.w && w <= max.w;

        public override string ToString() => $"{x},{y},{z},{w}";

        public bool Equals(Vector4Int other) => this == other;

        public override bool Equals(object obj)
        {
            if (obj is Vector4Int) return this == (Vector4Int) obj;
            return base.Equals(obj);
        }

        public override int GetHashCode() =>
            // ReSharper disable NonReadonlyMemberInGetHashCode
            10267 * x + 20033 * y + 400992 * z + 812921 * w;
        // ReSharper restore NonReadonlyMemberInGetHashCode

        public Vector4Int WithX(int newX) => new Vector4Int(newX, y, z, w);

        public Vector4Int WithY(int newY) => new Vector4Int(x, newY, z, w);

        public Vector4Int WithZ(int newZ) => new Vector4Int(x, y, newZ, w);

        public Vector4Int WithW(int newW) => new Vector4Int(x, y, z, newW);

        public void Clamp(Vector4Int min, Vector4Int max)
        {
            x = Math.Max(x, min.x);
            y = Math.Max(y, min.y);
            z = Math.Max(z, max.z);
            w = Math.Max(w, max.w);

            x = Math.Min(x, max.x);
            y = Math.Min(y, max.y);
            z = Math.Min(z, max.z);
            w = Math.Min(w, max.w);
        }
    }
}