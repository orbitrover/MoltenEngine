using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "float"/> vector comprised of 3 components.</summary>
	public partial struct Vector3F
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0F ? 0F : X > 1F ? 1F : X;
			Y = Y < 0F ? 0F : Y > 1F ? 1F : Y;
			Z = Z < 0F ? 0F : Z > 1F ? 1F : Z;
        }
	}
}
