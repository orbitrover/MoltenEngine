using System.Runtime.InteropServices;

namespace Molten.Math
{
	///<summary>A <see cref = "double"/> vector comprised of 2 components.</summary>
	public partial struct Vector2D
	{
		/// <summary>
        /// Saturates this instance in the range [0,1]
        /// </summary>
        public void Saturate()
        {
			X = X < 0D ? 0D : X > 1D ? 1D : X;
			Y = Y < 0D ? 0D : Y > 1D ? 1D : Y;
        }
	}
}
