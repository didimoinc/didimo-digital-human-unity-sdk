using UnityEngine;

namespace DigitalSalmon
{
    public static class VectorUtilities
    {
        /// <summary>
        /// A <c>Vector2</c> comprised of <c>float.NaN</c> components.
        /// </summary>
        public static readonly Vector2 NaNVector2 = new Vector2(float.NaN, float.NaN);

        /// <summary>
        /// A <c>Vector3</c> comprised of <c>float.NaN</c> components.
        /// </summary>
        public static readonly Vector3 NaNVector3 = new Vector3(float.NaN, float.NaN, float.NaN);

        /// <summary>
        /// Return the vector where a.x|y -> b.x|y by t.
        /// </summary>
        public static Vector2 SmoothStep(Vector2 a, Vector2 b, float t) => new Vector2(Mathf.SmoothStep(a.x, b.x, t), Mathf.SmoothStep(a.y, b.y, t));

        /// <summary>
        /// Return square magnitude of the distance between a and b.
        /// </summary>
        public static float SqrDistance(Vector2 a, Vector2 b)
        {
            double num1 = a.x - b.x;
            double num2 = a.y - b.y;
            return (float) (num1 * num1 + num2 * num2);
        }

        /// <summary>
        /// Returns a Vector2 where x|y are min(a.x|y, b.x|y).
        /// </summary>
        public static Vector2 Min(Vector2 a, Vector2 b) => new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));

        /// <summary>
        /// Returns a Vector2 where x|y are max(a.x|y, b.x|y).
        /// </summary>
        public static Vector2 Max(Vector2 a, Vector2 b) => new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));

        /// <summary>
        /// Returns a Vector2Int where x|y are min(a.x|y, b.x|y).
        /// </summary>
        public static Vector2Int Min(Vector2Int a, Vector2Int b) => new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));

        /// <summary>
        /// Returns a Vector2Int where x|y are max(a.x|y, b.x|y).
        /// </summary>
        public static Vector2Int Max(Vector2Int a, Vector2Int b) => new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
    }
}