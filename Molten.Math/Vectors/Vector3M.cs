using System;
using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "decimal"/> vector comprised of 3 components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=16)]
	public partial struct Vector3M
	{
		///<summary>The X component.</summary>
		public decimal X;

		///<summary>The Y component.</summary>
		public decimal Y;

		///<summary>The Z component.</summary>
		public decimal Z;


		///<summary>The size of <see cref="Vector3M"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector3M));

		public static Vector3M One = new Vector3M(1M, 1M, 1M);

		/// <summary>
        /// The X unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitX = new Vector3M(1M, 0M, 0M);

		/// <summary>
        /// The Y unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitY = new Vector3M(0M, 1M, 0M);

		/// <summary>
        /// The Z unit <see cref="Vector3M"/>.
        /// </summary>
		public static Vector3M UnitZ = new Vector3M(0M, 0M, 1M);

		public static Vector3M Zero = new Vector3M(0M, 0M, 0M);

#region Constructors
		///<summary>Creates a new instance of <see cref = "Vector3M"/>.</summary>
		public Vector3M(decimal x, decimal y, decimal z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector3M"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y and Z components of the vector. This must be an array with 3 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector3M(decimal[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 3)
                throw new ArgumentOutOfRangeException("values", "There must be 3 and only 3 input values for Vector3M.");

			X = values[0];
			Y = values[1];
			Z = values[2];
        }
#endregion

#region Common Functions
		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3M"/> vectors.
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
		public static void DistanceSquared(ref Vector3M value1, ref Vector3M value2, out decimal result)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;

            result = (x * x) + (y * y) + (z * z);
        }

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector3M"/> vectors.
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
		public static decimal DistanceSquared(ref Vector3M value1, ref Vector3M value2)
        {
            decimal x = value1.X - value2.X;
            decimal y = value1.Y - value2.Y;
            decimal z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }


#endregion

#region Add operators
		public static Vector3M operator +(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
		}

		public static Vector3M operator +(Vector3M left, decimal right)
		{
			return new Vector3M(left.X + right, left.Y + right, left.Z + right);
		}
#endregion

#region Subtract operators
		public static Vector3M operator -(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
		}

		public static Vector3M operator -(Vector3M left, decimal right)
		{
			return new Vector3M(left.X - right, left.Y - right, left.Z - right);
		}
#endregion

#region division operators
		public static Vector3M operator /(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X / right.X, left.Y / right.Y, left.Z / right.Z);
		}

		public static Vector3M operator /(Vector3M left, decimal right)
		{
			return new Vector3M(left.X / right, left.Y / right, left.Z / right);
		}
#endregion

#region Multiply operators
		public static Vector3M operator *(Vector3M left, Vector3M right)
		{
			return new Vector3M(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
		}

		public static Vector3M operator *(Vector3M left, decimal right)
		{
			return new Vector3M(left.X * right, left.Y * right, left.Z * right);
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
        
		public decimal this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3M run from 0 to 2, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector3M run from 0 to 2, inclusive.");
			}
		}
#endregion
	}
}

