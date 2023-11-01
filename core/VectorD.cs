using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nour.Play {
    // ? Maybe find a better name
    public struct VectorD : IEquatable<VectorD> {
        public static readonly VectorD Zero2D = new VectorD(new double[] { 0, 0 });

        private double[] _value;
        private double _length;
        private bool _isInitialized; // false on initialization

        public double[] Value => _value;
        public bool IsEmpty => !_isInitialized;
        public double MagnitudeSq => _value.Sum(a => a * a);
        public double Magnitude => Double.IsNaN(_length) ? _length = Math.Sqrt(MagnitudeSq) : _length;

        public VectorD WithMagnitude(double newMagnitude) {
            var mag = Magnitude;
            var res = new VectorD(_value.Select(a => mag < Double.Epsilon ? 0D : newMagnitude * a / mag));
            // Console.WriteLine($"{this} of l {Magnitude:F2} after receiving mag {newMagnitude:F2} = {res}");
            return res;
        }

        public VectorD(IEnumerable<double> dimensions) {
            dimensions.ThrowIfNull("dimensions");
            _value = dimensions.ToArray();
            _length = Double.NaN;
            _isInitialized = true;
        }

        public Vector RoundToInt() =>
            new Vector(_value.Select(a => (int)Math.Round(a)));

        public static VectorD operator -(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another, () => new VectorD(one._value.Zip(another._value, (a, b) => a - b)));

        public static VectorD operator +(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another, () => new VectorD(one._value.Zip(another._value, (a, b) => a + b)));

        public static VectorD operator +(VectorD one, Vector another) =>
            one.IsEmpty || another.IsEmpty ? throw new InvalidOperationException("Cannot operate on an empty vector") :
            new VectorD(one._value.Zip(another.Value, (a, b) => a + b));

        public static double[] operator /(VectorD dividend, double divisor) =>
            ThrowIfEmptyOrApply(dividend, VectorD.Zero2D,
            () => dividend._value.Select(e => divisor < Double.Epsilon ?
                throw new InvalidOperationException("Can't divide by zero") : e / divisor)).ToArray();

        public override bool Equals(object obj) => this.Equals((VectorD)obj);
        public override int GetHashCode() =>
            IsEmpty ? _value.GetHashCode() :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "00" : String.Join("x", _value.Select(v => v.ToString("F2"))) + "D";
        public bool Equals(VectorD another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.Zip(another._value, (a, b) => Math.Abs(a - b) < Double.Epsilon).All(a => a));

        private static T ThrowIfEmptyOrApply<T>(VectorD one, VectorD another, Func<T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply();
        }
    }
}