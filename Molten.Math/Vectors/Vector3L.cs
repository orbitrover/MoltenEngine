using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "long"/> vector comprised of three components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
	public partial struct Vector3L
	{
		///<summary>The X component.</summary>
		public long X;

		///<summary>The Y component.</summary>
		public long Y;

		///<summary>The Z component.</summary>
		public long Z;


		///<summary>The size of <see cref="Vector3L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3L));

		public static Vector3L One = new Vector3L(1L, 1L, 1L);

		/// <summary>
        /// The X unit <see cref="Vector3L"/>.
        /// </summary>
		public static Vector3L UnitX = new Vector3L(1L, 0L, 0L);

		/// <summary>
        /// The Y unit <see cref="Vector3L"/>.
        /// </summary>
		public static Vector3L UnitY = new Vector3L(0L, 1L, 0L);

		/// <summary>
        /// The Z unit <see cref="Vector3L"/>.
        /// </summary>
		public static Vector3L UnitZ = new Vector3L(0L, 0L, 1L);

		public static Vector3L Zero = new Vector3L(0L, 0L, 0L);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector3L"/>.</summary>
		public Vector3L(long x, long y, long z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3L"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3L(long[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3L.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3L"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector</param>
        /// <param name="result">When the method completes, contains the squared distance between the two vectors.</param>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static void DistanceSquared(ref Vector3L value1, ref Vector3L value2, out long result)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3L"/> vectors.
        /// </summary>
        /// <param name="value1">The first vector.</param>
        /// <param name="value2">The second vector.</param>
        /// <returns>The squared distance between the two vectors.</returns>
        /// <remarks>Distance squared is the value before taking the square root. 
        /// Distance squared can often be used in place of distance if relative comparisons are being made. 
        /// For example, consider three points A, B, and C. To determine whether B or C is further from A, 
        /// compare the distance between A and B to the distance between A and C. Calculating the two distances 
        /// involves two square roots, which are computationally expensive. However, using distance squared 
        /// provides the same information and avoids calculating two square roots.
        /// </remarks>
		public static long DistanceSquared(ref Vector3L value1, ref Vector3L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector3L"/>.
        /// </summary>
        /// <returns>A three-element array containing the components of the vector.</returns>
        public long[] ToArray()
        {
            return new long[] { X, Y, Z};
        }

		/// <summary>
        /// Reverses the direction of the current <see cref="Vector3L"/>.
        /// </summary>
        /// <returns>A <see cref="Vector3L"/> facing the opposite direction.</returns>
		public Vector3L Negate()
		{
			return new Vector3L(-X, -Y, -Z);
		}

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two vectors.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector3L Lerp(ref Vector3L start, ref Vector3L end, float amount)
        {
			return new Vector3L()
			{
				X = (long)((1f - amount) * start.X + amount * end.X),
				Y = (long)((1f - amount) * start.Y + amount * end.Y),
				Z = (long)((1f - amount) * start.Z + amount * end.Z),
			};
        }
#endregion

#region Add operators
		public static Vector3L operator +(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3L operator +(Vector3L left, long right)
		{
			return new Vector3L(left.X + right, left.Y + right, left.Z + right);
		}

		/// <summary>
        /// Assert a <see cref="Vector3L"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector3L"/>.</returns>
        public static Vector3L operator +(Vector3L value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static Vector3L operator -(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3L operator -(Vector3L left, long right)
		{
			return new Vector3L(left.X - right, left.Y - right, left.Z - right);
		}

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3L"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector3L"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector3L"/>.</returns>
        public static Vector3L operator -(Vector3L value)
        {
            return new Vector3L(-value.X, -value.Y, -value.Z);
        }
#endregion

#region division operators
		public static Vector3L operator /(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3L operator /(Vector3L left, long right)
		{
			return new Vector3L(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3L operator *(Vector3L left, Vector3L right)
		{
			return new Vector3L(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3L operator *(Vector3L left, long right)
		{
			return new Vector3L(left.X * right, left.Y * right, left.Z * right);
		}
#endregion

#region Properties

#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y or Z component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 2].</exception>
        
		public long this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3L run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3L run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

