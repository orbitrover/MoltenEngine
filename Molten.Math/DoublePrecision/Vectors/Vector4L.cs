using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Molten.HalfPrecision;
using Molten.DoublePrecision;

namespace Molten.DoublePrecision
{
	///<summary>A <see cref = "long"/> vector comprised of four components.</summary>
	[StructLayout(LayoutKind.Sequential, Pack=8)]
    [Serializable]
	public partial struct Vector4L : IFormattable
	{
		///<summary>The X component.</summary>
        [DataMember]
		public long X;

		///<summary>The Y component.</summary>
        [DataMember]
		public long Y;

		///<summary>The Z component.</summary>
        [DataMember]
		public long Z;

		///<summary>The W component.</summary>
        [DataMember]
		public long W;

		///<summary>The size of <see cref="Vector4L"/>, in bytes.</summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(typeof(Vector4L));

		///<summary>A Vector4L with every component set to 1L.</summary>
		public static readonly Vector4L One = new Vector4L(1L, 1L, 1L, 1L);

		/// <summary>The X unit <see cref="Vector4L"/>.</summary>
		public static readonly Vector4L UnitX = new Vector4L(1L, 0, 0, 0);

		/// <summary>The Y unit <see cref="Vector4L"/>.</summary>
		public static readonly Vector4L UnitY = new Vector4L(0, 1L, 0, 0);

		/// <summary>The Z unit <see cref="Vector4L"/>.</summary>
		public static readonly Vector4L UnitZ = new Vector4L(0, 0, 1L, 0);

		/// <summary>The W unit <see cref="Vector4L"/>.</summary>
		public static readonly Vector4L UnitW = new Vector4L(0, 0, 0, 1L);

		/// <summary>Represents a zero'd Vector4L.</summary>
		public static readonly Vector4L Zero = new Vector4L(0, 0, 0, 0);

        /// <summary>
        /// Gets a value indicting whether this vector is zero
        /// </summary>
        public bool IsZero
        {
            get => X == 0 && Y == 0 && Z == 0 && W == 0;
        }

#region Constructors
        ///<summary>Creates a new instance of <see cref = "Vector4L"/>, using a <see cref="Vector2L"/> to populate the first two components.</summary>
		public Vector4L(Vector2L vector, long z, long w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = z;
			W = w;
		}
        ///<summary>Creates a new instance of <see cref = "Vector4L"/>, using a <see cref="Vector3L"/> to populate the first three components.</summary>
		public Vector4L(Vector3L vector, long w)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

		///<summary>Creates a new instance of <see cref = "Vector4L"/>.</summary>
		public Vector4L(long x, long y, long z, long w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

        ///<summary>Creates a new instance of <see cref = "Vector4L"/>.</summary>
		public Vector4L(long value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector4L"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the X, Y, Z and W components of the vector. This must be an array with 4 elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than two elements.</exception>
        public Vector4L(long[] values)
        {
            if (values == null)
                throw new ArgumentNullException("values");
            if (values.Length != 4)
                throw new ArgumentOutOfRangeException("values", "There must be 4 and only 4 input values for Vector4L.");

			X = values[0];
			Y = values[1];
			Z = values[2];
			W = values[3];
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="Vector4L"/> struct from an unsafe pointer. The pointer should point to an array of four elements.
        /// </summary>
		public unsafe Vector4L(long* ptr)
		{
			X = ptr[0];
			Y = ptr[1];
			Z = ptr[2];
			W = ptr[3];
		}
#endregion

#region Instance Methods
        /// <summary>
        /// Determines whether the specified <see cref="Vector4L"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ref Vector4L other)
        {
            return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4L"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Vector4L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4L other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Vector4L"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Vector4L"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Vector4L"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value is not Vector4L)
                return false;

            var strongValue = (Vector4L)value;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Calculates the squared length of the vector.
        /// </summary>
        /// <returns>The squared length of the vector.</returns>
        /// <remarks>
        /// This method may be preferred to <see cref="Vector2F.Length"/> when only a relative length is needed
        /// and speed is of the essence.
        /// </remarks>
        public long LengthSquared()
        {
            return ((X * X) + (Y * Y) + (Z * Z) + (W * W));
        }

		/// <summary>
        /// Creates an array containing the elements of the current <see cref="Vector4L"/>.
        /// </summary>
        /// <returns>A four-element array containing the components of the vector.</returns>
        public long[] ToArray()
        {
            return new long[] { X, Y, Z, W};
        }
		/// <summary>
        /// Reverses the direction of the current <see cref="Vector4L"/>.
        /// </summary>
        /// <returns>A <see cref="Vector4L"/> facing the opposite direction.</returns>
		public Vector4L Negate()
		{
			return new Vector4L(-X, -Y, -Z, -W);
		}
		

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(long min, long max)
        {
			X = X < min ? min : X > max ? max : X;
			Y = Y < min ? min : Y > max ? max : Y;
			Z = Z < min ? min : Z > max ? max : Z;
			W = W < min ? min : W > max ? max : W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public void Clamp(Vector4L min, Vector4L max)
        {
			X = X < min.X ? min.X : X > max.X ? max.X : X;
			Y = Y < min.Y ? min.Y : Y > max.Y ? max.Y : Y;
			Z = Z < min.Z ? min.Z : Z > max.Z ? max.Z : Z;
			W = W < min.W ? min.W : W > max.W ? max.W : W;
        }
#endregion

#region To-String

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", 
			X.ToString(format, CultureInfo.CurrentCulture), Y.ToString(format, CultureInfo.CurrentCulture), Z.ToString(format, CultureInfo.CurrentCulture), W.ToString(format, CultureInfo.CurrentCulture));
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "X:{0} Y:{1} Z:{2} W:{3}", X, Y, Z, W);
        }

		/// <summary>
        /// Returns a <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this <see cref="Vector4L"/>.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(formatProvider, "X:{0} Y:{1} Z:{2} W:{3}", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
#endregion

#region Add operators
        public static void Add(ref Vector4L left, ref Vector4L right, out Vector4L result)
        {
			result.X = (left.X + right.X);
			result.Y = (left.Y + right.Y);
			result.Z = (left.Z + right.Z);
			result.W = (left.W + right.W);
        }

        public static void Add(ref Vector4L left, long right, out Vector4L result)
        {
			result.X = (left.X + right);
			result.Y = (left.Y + right);
			result.Z = (left.Z + right);
			result.W = (left.W + right);
        }

		public static Vector4L operator +(Vector4L left, Vector4L right)
		{
			Add(ref left, ref right, out Vector4L result);
            return result;
		}

		public static Vector4L operator +(Vector4L left, long right)
		{
            Add(ref left, right, out Vector4L result);
            return result;
		}

        public static Vector4L operator +(long left, Vector4L right)
		{
            Add(ref right, left, out Vector4L result);
            return result;
		}

		/// <summary>
        /// Assert a <see cref="Vector4L"/> (return it unchanged).
        /// </summary>
        /// <param name="value">The <see cref="Vector4L"/> to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) <see cref="Vector4L"/>.</returns>
        public static Vector4L operator +(Vector4L value)
        {
            return value;
        }
#endregion

#region Subtract operators
		public static void Subtract(ref Vector4L left, ref Vector4L right, out Vector4L result)
        {
			result.X = (left.X - right.X);
			result.Y = (left.Y - right.Y);
			result.Z = (left.Z - right.Z);
			result.W = (left.W - right.W);
        }

        public static void Subtract(ref Vector4L left, long right, out Vector4L result)
        {
			result.X = (left.X - right);
			result.Y = (left.Y - right);
			result.Z = (left.Z - right);
			result.W = (left.W - right);
        }

		public static Vector4L operator -(Vector4L left, Vector4L right)
		{
			Subtract(ref left, ref right, out Vector4L result);
            return result;
		}

		public static Vector4L operator -(Vector4L left, long right)
		{
            Subtract(ref left, right, out Vector4L result);
            return result;
		}

        public static Vector4L operator -(long left, Vector4L right)
		{
            Subtract(ref right, left, out Vector4L result);
            return result;
		}

        /// <summary>
        /// Negate/reverse the direction of a <see cref="Vector3D"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4L"/> to reverse.</param>
        /// <param name="result">The output for the reversed <see cref="Vector4L"/>.</param>
        public static void Negate(ref Vector4L value, out Vector4L result)
        {
			result.X = -value.X;
			result.Y = -value.Y;
			result.Z = -value.Z;
			result.W = -value.W;
            
        }

		/// <summary>
        /// Negate/reverse the direction of a <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="value">The <see cref="Vector4L"/> to reverse.</param>
        /// <returns>The reversed <see cref="Vector4L"/>.</returns>
        public static Vector4L operator -(Vector4L value)
        {
            Negate(ref value, out value);
            return value;
        }
#endregion

#region division operators
		public static void Divide(ref Vector4L left, ref Vector4L right, out Vector4L result)
        {
			result.X = (left.X / right.X);
			result.Y = (left.Y / right.Y);
			result.Z = (left.Z / right.Z);
			result.W = (left.W / right.W);
        }

        public static void Divide(ref Vector4L left, long right, out Vector4L result)
        {
			result.X = (left.X / right);
			result.Y = (left.Y / right);
			result.Z = (left.Z / right);
			result.W = (left.W / right);
        }

		public static Vector4L operator /(Vector4L left, Vector4L right)
		{
			Divide(ref left, ref right, out Vector4L result);
            return result;
		}

		public static Vector4L operator /(Vector4L left, long right)
		{
            Divide(ref left, right, out Vector4L result);
            return result;
		}

        public static Vector4L operator /(long left, Vector4L right)
		{
            Divide(ref right, left, out Vector4L result);
            return result;
		}
#endregion

#region Multiply operators
		public static void Multiply(ref Vector4L left, ref Vector4L right, out Vector4L result)
        {
			result.X = (left.X * right.X);
			result.Y = (left.Y * right.Y);
			result.Z = (left.Z * right.Z);
			result.W = (left.W * right.W);
        }

        public static void Multiply(ref Vector4L left, long right, out Vector4L result)
        {
			result.X = (left.X * right);
			result.Y = (left.Y * right);
			result.Z = (left.Z * right);
			result.W = (left.W * right);
        }

		public static Vector4L operator *(Vector4L left, Vector4L right)
		{
			Multiply(ref left, ref right, out Vector4L result);
            return result;
		}

		public static Vector4L operator *(Vector4L left, long right)
		{
            Multiply(ref left, right, out Vector4L result);
            return result;
		}

        public static Vector4L operator *(long left, Vector4L right)
		{
            Multiply(ref right, left, out Vector4L result);
            return result;
		}
#endregion

#region Operators - Equality
        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector4L left, Vector4L right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector4L left, Vector4L right)
        {
            return !left.Equals(ref right);
        }
#endregion

#region Operators - Cast
        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector2L"/>.</summary>
        public static explicit operator Vector2L(Vector4L value)
        {
            return new Vector2L(value.X, value.Y);
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector3L"/>.</summary>
        public static explicit operator Vector3L(Vector4L value)
        {
            return new Vector3L(value.X, value.Y, value.Z);
        }

#endregion

#region Static Methods
        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        public static Vector4L SmoothStep(ref Vector4L start, ref Vector4L end, double amount)
        {
            amount = MathHelper.SmoothStep(amount);
            return Lerp(ref start, ref end, amount);
        }

        /// <summary>
        /// Performs a cubic interpolation between two vectors.
        /// </summary>
        /// <param name="start">Start vector.</param>
        /// <param name="end">End vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two vectors.</returns>
        public static Vector4L SmoothStep(Vector4L start, Vector4L end, long amount)
        {
            return SmoothStep(ref start, ref end, amount);
        }    

        /// <summary>
        /// Orthogonalizes a list of <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="destination">The list of orthogonalized <see cref="Vector4L"/>.</param>
        /// <param name="source">The list of vectors to orthogonalize.</param>
        /// <remarks>
        /// <para>Orthogonalization is the process of making all vectors orthogonal to each other. This
        /// means that any given vector in the list will be orthogonal to any other given vector in the
        /// list.</para>
        /// <para>Because this method uses the modified Gram-Schmidt process, the resulting vectors
        /// tend to be numerically unstable. The numeric stability decreases according to the vectors
        /// position in the list so that the first vector is the most stable and the last vector is the
        /// least stable.</para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="source"/> or <paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="destination"/> is shorter in length than <paramref name="source"/>.</exception>
        public static void Orthogonalize(Vector4L[] destination, params Vector4L[] source)
        {
            //Uses the modified Gram-Schmidt process.
            //q1 = m1
            //q2 = m2 - ((q1 ⋅ m2) / (q1 ⋅ q1)) * q1
            //q3 = m3 - ((q1 ⋅ m3) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m3) / (q2 ⋅ q2)) * q2
            //q4 = m4 - ((q1 ⋅ m4) / (q1 ⋅ q1)) * q1 - ((q2 ⋅ m4) / (q2 ⋅ q2)) * q2 - ((q3 ⋅ m4) / (q3 ⋅ q3)) * q3
            //q5 = ...

            if (source == null)
                throw new ArgumentNullException("source");
            if (destination == null)
                throw new ArgumentNullException("destination");
            if (destination.Length < source.Length)
                throw new ArgumentOutOfRangeException("destination", "The destination array must be of same length or larger length than the source array.");

            for (int i = 0; i < source.Length; ++i)
            {
                Vector4L newvector = source[i];

                for (int r = 0; r < i; ++r)
                    newvector -= (Dot(destination[r], newvector) / Dot(destination[r], destination[r])) * destination[r];

                destination[i] = newvector;
            }
        }

        

        /// <summary>
        /// Takes the value of an indexed component and assigns it to the axis of a new <see cref="Vector4L"/>. <para />
        /// For example, a swizzle input of (1,1) on a <see cref="Vector4L"/> with the values, 20 and 10, will return a vector with values 10,10, because it took the value of component index 1, for both axis."
        /// </summary>
        /// <param name="val">The current vector.</param>
		/// <param name="xIndex">The axis index to use for the new X value.</param>
		/// <param name="yIndex">The axis index to use for the new Y value.</param>
		/// <param name="zIndex">The axis index to use for the new Z value.</param>
		/// <param name="wIndex">The axis index to use for the new W value.</param>
        /// <returns></returns>
        public static unsafe Vector4L Swizzle(Vector4L val, int xIndex, int yIndex, int zIndex, int wIndex)
        {
            return new Vector4L()
            {
			   X = (&val.X)[xIndex],
			   Y = (&val.X)[yIndex],
			   Z = (&val.X)[zIndex],
			   W = (&val.X)[wIndex],
            };
        }

        /// <returns></returns>
        public static unsafe Vector4L Swizzle(Vector4L val, uint xIndex, uint yIndex, uint zIndex, uint wIndex)
        {
            return new Vector4L()
            {
			    X = (&val.X)[xIndex],
			    Y = (&val.X)[yIndex],
			    Z = (&val.X)[zIndex],
			    W = (&val.X)[wIndex],
            };
        }

        /// <summary>
        /// Calculates the dot product of two <see cref="Vector4L"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4L"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4L"/> source vector.</param>
        public static long Dot(ref Vector4L left, ref Vector4L right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Calculates the dot product of two <see cref="Vector4L"/> vectors.
        /// </summary>
        /// <param name="left">First <see cref="Vector4L"/> source vector</param>
        /// <param name="right">Second <see cref="Vector4L"/> source vector.</param>
        public static long Dot(Vector4L left, Vector4L right)
        {
			return ((left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z) + (left.W * right.W));
        }

		/// <summary>
        /// Returns a <see cref="Vector4L"/> containing the 2D Cartesian coordinates of a point specified in Barycentric coordinates relative to a 2D triangle.
        /// </summary>
        /// <param name="value1">A <see cref="Vector4L"/> containing the 4D Cartesian coordinates of vertex 1 of the triangle.</param>
        /// <param name="value2">A <see cref="Vector4L"/> containing the 4D Cartesian coordinates of vertex 2 of the triangle.</param>
        /// <param name="value3">A <see cref="Vector4L"/> containing the 4D Cartesian coordinates of vertex 3 of the triangle.</param>
        /// <param name="amount1">Barycentric coordinate b2, which expresses the weighting factor toward vertex 2 (specified in <paramref name="value2"/>).</param>
        /// <param name="amount2">Barycentric coordinate b3, which expresses the weighting factor toward vertex 3 (specified in <paramref name="value3"/>).</param>
        public static Vector4L Barycentric(ref Vector4L value1, ref Vector4L value2, ref Vector4L value3, long amount1, long amount2)
        {
			return new Vector4L(
				((value1.X + (amount1 * (value2.X - value1.X))) + (amount2 * (value3.X - value1.X))), 
				((value1.Y + (amount1 * (value2.Y - value1.Y))) + (amount2 * (value3.Y - value1.Y))), 
				((value1.Z + (amount1 * (value2.Z - value1.Z))) + (amount2 * (value3.Z - value1.Z))), 
				((value1.W + (amount1 * (value2.W - value1.W))) + (amount2 * (value3.W - value1.W)))
			);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4L"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(ref Vector4L start, ref Vector4L end, double amount, out Vector4L result)
        {
			result.X = (long)((1D - amount) * start.X + amount * end.X);
			result.Y = (long)((1D - amount) * start.Y + amount * end.Y);
			result.Z = (long)((1D - amount) * start.Z + amount * end.Z);
			result.W = (long)((1D - amount) * start.W + amount * end.W);
        }

        /// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector4L Lerp(Vector4L start, Vector4L end, double amount)
        {
			return new Vector4L()
			{
				X = (long)((1D - amount) * start.X + amount * end.X),
				Y = (long)((1D - amount) * start.Y + amount * end.Y),
				Z = (long)((1D - amount) * start.Z + amount * end.Z),
				W = (long)((1D - amount) * start.W + amount * end.W),
			};
        }

		/// <summary>
        /// Performs a linear interpolation between two <see cref="Vector4L"/>.
        /// </summary>
        /// <param name="start">The start vector.</param>
        /// <param name="end">The end vector.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Vector4L Lerp(ref Vector4L start, ref Vector4L end, double amount)
        {
			return new Vector4L()
			{
				X = (long)((1D - amount) * start.X + amount * end.X),
				Y = (long)((1D - amount) * start.Y + amount * end.Y),
				Z = (long)((1D - amount) * start.Z + amount * end.Z),
				W = (long)((1D - amount) * start.W + amount * end.W),
			};
        }

        /// <summary>
        /// Returns a <see cref="Vector4L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the smallest components of the source vectors.</returns>
		public static void Min(ref Vector4L left, ref Vector4L right, out Vector4L result)
		{
				result.X = (left.X < right.X) ? left.X : right.X;
				result.Y = (left.Y < right.Y) ? left.Y : right.Y;
				result.Z = (left.Z < right.Z) ? left.Z : right.Z;
				result.W = (left.W < right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the smallest components of the source vectors.</returns>
		public static Vector4L Min(ref Vector4L left, ref Vector4L right)
		{
			Min(ref left, ref right, out Vector4L result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4L"/> containing the smallest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the smallest components of the source vectors.</returns>
		public static Vector4L Min(Vector4L left, Vector4L right)
		{
			return new Vector4L()
			{
				X = (left.X < right.X) ? left.X : right.X,
				Y = (left.Y < right.Y) ? left.Y : right.Y,
				Z = (left.Z < right.Z) ? left.Z : right.Z,
				W = (left.W < right.W) ? left.W : right.W,
			};
		}

        /// <summary>
        /// Returns a <see cref="Vector4L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the largest components of the source vectors.</returns>
		public static void Max(ref Vector4L left, ref Vector4L right, out Vector4L result)
		{
				result.X = (left.X > right.X) ? left.X : right.X;
				result.Y = (left.Y > right.Y) ? left.Y : right.Y;
				result.Z = (left.Z > right.Z) ? left.Z : right.Z;
				result.W = (left.W > right.W) ? left.W : right.W;
		}

        /// <summary>
        /// Returns a <see cref="Vector4L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the largest components of the source vectors.</returns>
		public static Vector4L Max(ref Vector4L left, ref Vector4L right)
		{
			Max(ref left, ref right, out Vector4L result);
            return result;
		}

		/// <summary>
        /// Returns a <see cref="Vector4L"/> containing the largest components of the specified vectors.
        /// </summary>
        /// <param name="left">The first source <see cref="Vector4L"/>.</param>
        /// <param name="right">The second source <see cref="Vector4L"/>.</param>
        /// <returns>A <see cref="Vector4L"/> containing the largest components of the source vectors.</returns>
		public static Vector4L Max(Vector4L left, Vector4L right)
		{
			return new Vector4L()
			{
				X = (left.X > right.X) ? left.X : right.X,
				Y = (left.Y > right.Y) ? left.Y : right.Y,
				Z = (left.Z > right.Z) ? left.Z : right.Z,
				W = (left.W > right.W) ? left.W : right.W,
			};
		}

		/// <summary>
        /// Calculates the squared distance between two <see cref="Vector4L"/> vectors.
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
		public static long DistanceSquared(ref Vector4L value1, ref Vector4L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;
            long w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

        /// <summary>
        /// Calculates the squared distance between two <see cref="Vector4L"/> vectors.
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
		public static long DistanceSquared(Vector4L value1, Vector4L value2)
        {
            long x = value1.X - value2.X;
            long y = value1.Y - value2.Y;
            long z = value1.Z - value2.Z;
            long w = value1.W - value2.W;

            return ((x * x) + (y * y) + (z * z) + (w * w));
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector4L Clamp(Vector4L value, long min, long max)
        {
			return new Vector4L()
			{
				X = value.X < min ? min : value.X > max ? max : value.X,
				Y = value.Y < min ? min : value.Y > max ? max : value.Y,
				Z = value.Z < min ? min : value.Z > max ? max : value.Z,
				W = value.W < min ? min : value.W > max ? max : value.W,
			};
        }

        /// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        /// <param name="result">The output for the resultant <see cref="Vector4L"/>.</param>
        public static void Clamp(ref Vector4L value, ref Vector4L min, ref Vector4L max, out Vector4L result)
        {
				result.X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X;
				result.Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y;
				result.Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z;
				result.W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W;
        }

		/// <summary>Clamps the component values to within the given range.</summary>
        /// <param name="value">The <see cref="Vector4L"/> value to be clamped.</param>
        /// <param name="min">The minimum value of each component.</param>
        /// <param name="max">The maximum value of each component.</param>
        public static Vector4L Clamp(Vector4L value, Vector4L min, Vector4L max)
        {
			return new Vector4L()
			{
				X = value.X < min.X ? min.X : value.X > max.X ? max.X : value.X,
				Y = value.Y < min.Y ? min.Y : value.Y > max.Y ? max.Y : value.Y,
				Z = value.Z < min.Z ? min.Z : value.Z > max.Z ? max.Z : value.Z,
				W = value.W < min.W ? min.W : value.W > max.W ? max.W : value.W,
			};
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal. 
        /// </summary>
        /// <param name="vector">The source vector.</param>
        /// <param name="normal">Normal of the surface.</param>
        /// <remarks>Reflect only gives the direction of a reflection off a surface, it does not determine 
        /// whether the original vector was close enough to the surface to hit it.</remarks>
        public static Vector4L Reflect(ref Vector4L vector, ref Vector4L normal)
        {
            long dot = (vector.X * normal.X) + (vector.Y * normal.Y) + (vector.Z * normal.Z) + (vector.W * normal.W);

            return new Vector4L()
            {
				X = (long)(vector.X - ((2 * dot) * normal.X)),
				Y = (long)(vector.Y - ((2 * dot) * normal.Y)),
				Z = (long)(vector.Z - ((2 * dot) * normal.Z)),
				W = (long)(vector.W - ((2 * dot) * normal.W)),
            };
        }
#endregion

#region Tuples
        public static implicit operator (long x, long y, long z, long w)(Vector4L val)
        {
            return (val.X, val.Y, val.Z, val.W);
        }

        public static implicit operator Vector4L((long x, long y, long z, long w) val)
        {
            return new Vector4L(val.x, val.y, val.z, val.w);
        }
#endregion

#region Indexers
		/// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the X, Y, Z or W component, depending on the index.</value>
        /// <param name="index">The index of the component to access. Use 0 for the X component, 1 for the Y component and so on.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 3].</exception>  
		public long this[int index]
		{
			get
			{
				switch(index)
				{
					case 0: return X;
					case 1: return Y;
					case 2: return Z;
					case 3: return W;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4L run from 0 to 3, inclusive.");
			}

			set
			{
				switch(index)
				{
					case 0: X = value; break;
					case 1: Y = value; break;
					case 2: Z = value; break;
					case 3: W = value; break;
				}
				throw new ArgumentOutOfRangeException("index", "Indices for Vector4L run from 0 to 3, inclusive.");
			}
		}
#endregion

#region Casts - vectors
        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="SByte4"/>.</summary>
        public static explicit operator SByte4(Vector4L val)
        {
            return new SByte4()
            {
                X = (sbyte)val.X,
                Y = (sbyte)val.Y,
                Z = (sbyte)val.Z,
                W = (sbyte)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Byte4"/>.</summary>
        public static explicit operator Byte4(Vector4L val)
        {
            return new Byte4()
            {
                X = (byte)val.X,
                Y = (byte)val.Y,
                Z = (byte)val.Z,
                W = (byte)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4I"/>.</summary>
        public static explicit operator Vector4I(Vector4L val)
        {
            return new Vector4I()
            {
                X = (int)val.X,
                Y = (int)val.Y,
                Z = (int)val.Z,
                W = (int)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4UI"/>.</summary>
        public static explicit operator Vector4UI(Vector4L val)
        {
            return new Vector4UI()
            {
                X = (uint)val.X,
                Y = (uint)val.Y,
                Z = (uint)val.Z,
                W = (uint)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4S"/>.</summary>
        public static explicit operator Vector4S(Vector4L val)
        {
            return new Vector4S()
            {
                X = (short)val.X,
                Y = (short)val.Y,
                Z = (short)val.Z,
                W = (short)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4US"/>.</summary>
        public static explicit operator Vector4US(Vector4L val)
        {
            return new Vector4US()
            {
                X = (ushort)val.X,
                Y = (ushort)val.Y,
                Z = (ushort)val.Z,
                W = (ushort)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4UL"/>.</summary>
        public static explicit operator Vector4UL(Vector4L val)
        {
            return new Vector4UL()
            {
                X = (ulong)val.X,
                Y = (ulong)val.Y,
                Z = (ulong)val.Z,
                W = (ulong)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4F"/>.</summary>
        public static explicit operator Vector4F(Vector4L val)
        {
            return new Vector4F()
            {
                X = (float)val.X,
                Y = (float)val.Y,
                Z = (float)val.Z,
                W = (float)val.W,
            };
        }

        ///<summary>Casts a <see cref="Vector4L"/> to a <see cref="Vector4D"/>.</summary>
        public static explicit operator Vector4D(Vector4L val)
        {
            return new Vector4D()
            {
                X = (double)val.X,
                Y = (double)val.Y,
                Z = (double)val.Z,
                W = (double)val.W,
            };
        }

#endregion
	}
}
