
using System;
using System.Numerics;

namespace Nour.Play.Maze {
    public static class VectorExtensions {
        public static Vector<T> CreateFrom<T>(T[] array)
            where T : struct {
            if (array.Length != Vector<T>.Count) {
                var tmp = new T[Vector<T>.Count];
                if (array.Length > tmp.Length) {
                    throw new ArgumentOutOfRangeException("Too many components in a vector");
                }
                Array.Copy(array, tmp, array.Length);
                array = tmp;
            }
            return new Vector<T>(array);
        }
    }
}