﻿namespace Molten
{
    /// <summary>
    /// Represents an implementation of a transposable matrix.
    /// </summary>
    /// <typeparam name="S">The source (untransposed) type.</typeparam>
    /// <typeparam name="D">The destination (transposed) type.</typeparam>
    public interface ITransposableMatrix<S,D> 
        where S : unmanaged
        where D : unmanaged
    {
        /// <summary>
        /// Transposes the current matrix and outputs it to <paramref name="result"/>.
        /// </summary>
        /// <param name="result"></param>
        void Transpose(out D result);

        /// <summary>
        /// Transposes the current matrix in-place.
        /// </summary>
        D Transpose();

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix whose transpose is to be calculated.</param>
        /// <param name="result">When the method completes, contains the transpose of the specified matrix.</param>
        static abstract void Transpose(ref S matrix, out D result);

        /// <summary>
        /// Calculates the transpose of the specified matrix.
        /// </summary>
        /// <param name="matrix">The matrix whose transpose is to be calculated.</param>
        /// <returns>The transpose of the specified matrix.</returns>
        static abstract D Transpose(S matrix);
    }
}
