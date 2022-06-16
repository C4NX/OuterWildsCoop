using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuterWildsServerLib.Utils
{
    /// <summary>
    /// A simple data structure that represents a 3d vector (X, Y, Z).
    /// </summary>
    public struct Vector3
    {
        private static readonly Vector3 zero = new Vector3(0f, 0f, 0f);
        private static readonly Vector3 one = new Vector3(1f, 1f, 1f);

        /// <summary>
        /// Returns a <see cref="Vector3"/> (0,0,0)
        /// </summary>
        public static Vector3 Zero => zero;

        /// <summary>
        /// Returns a <see cref="Vector3"/> (1,1,1)
        /// </summary>
        public static Vector3 One => zero;

        /// <summary>
        /// The x coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float X;

        /// <summary>
        /// The y coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float Y;

        /// <summary>
        /// The z coordinate of this <see cref="Vector3"/>.
        /// </summary>
        public float Z;

        /// <summary>
        /// Create a new 3d space vector from X, Y and Z.
        /// </summary>
        /// <param name="x">The X value.</param>
        /// <param name="y">The Y value.</param>
        /// <param name="z">The Z value.</param>
        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        /// <summary>
        /// Create a new 3d space vector from one value.
        /// </summary>
        /// <param name="value">The X, Y, Z value.</param>
        public Vector3(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }
    }
}
