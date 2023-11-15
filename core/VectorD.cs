using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nour.Play {
    // ? Maybe find a better name
    public struct VectorD : IEquatable<VectorD> {
        public const double MIN = 1E-10;

        public static readonly VectorD Zero2D = new VectorD(new double[] { 0, 0 });

        private double[] _value;
        private double _length;
        private bool _isInitialized; // false on initialization

        public double[] Value => _value;
        public bool IsEmpty => !_isInitialized;

        public bool IsTwoDimensional => !IsEmpty && _value.Length == 2;
        public bool IsThreeDimensional => !IsEmpty && _value.Length == 3;
        public double X => IsTwoDimensional || IsThreeDimensional ? _value[0] :
            throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        public double Y => IsTwoDimensional || IsThreeDimensional ? _value[1] :
            throw new InvalidOperationException("X and Y are only supported in two- or three-dimensional space");
        /// <summary>
        /// A quick hack to avoid sq.rt
        /// </summary>
        public double Average => _value.Average();
        public double MagnitudeSq => _value.Sum(a => a * a);
        public double Magnitude => Double.IsNaN(_length) ? _length = Math.Sqrt(MagnitudeSq) : _length;

        public VectorD WithMagnitude(double newMagnitude) {
            var mag = Magnitude;
            return new VectorD(_value.Select(a => mag < MIN ? 0D : newMagnitude * a / mag));
        }

        public VectorD(IEnumerable<double> dimensions) {
            dimensions.ThrowIfNull("dimensions");
            _value = dimensions.ToArray();
            _length = Double.NaN;
            _isInitialized = true;
        }

        public VectorD(double x, double y) :
            this(new double[] { x, y }) { }

        public VectorD(Vector intVector) :
            this(intVector.Value.Select(v => (double)v)) { }

        public Vector RoundToInt() =>
            new Vector(_value.Select(a => (int)Math.Round(a)));

        public bool IsZero() => _value.All(v => v >= -MIN && v <= MIN);
        public VectorD NotZero(VectorD direction) {
            return direction / direction / 10D;
        }

        // public VectorD NotZeroVerbose(Log log, VectorD direction) {
        //     const double minVal = 10E-16;
        //     var nonZero = new List<double>();
        //     StringBuilder debugString = new StringBuilder();
        //     for (int i = 0; i < this._value.Length; i++) {
        //         var a = _value[i];
        //         var dir = direction._value[i];
        //         debugString.Append($"{i}({a:F2},{dir:F2},");
        //         if (Math.Abs(a) > 0.1) {
        //             debugString.Append($"case 0({Math.Abs(a)})");
        //             nonZero.Add(a);
        //         } else if (a > minVal) {
        //             debugString.Append($"case 1({a})");
        //             nonZero.Add(0.1);
        //         } else if (a < -minVal) {
        //             debugString.Append($"case 2({a})");
        //             nonZero.Add(-0.1);
        //         } else if (dir > minVal) {
        //             debugString.Append($"case 3({dir})");
        //             nonZero.Add(0.1);
        //         } else if (dir < -minVal) {
        //             debugString.Append($"case 4({dir})");
        //             nonZero.Add(-0.1);
        //         } else {
        //             debugString.Append("case 5"); nonZero.Add(0);
        //         }
        //         debugString.Append(")");
        //     }
        //     log.Buffered.D(debugString.ToString());
        //     return new VectorD(nonZero);
        // }

        public double DotProduct(VectorD other) {
            return this._value.Zip(other._value, (a, b) => a * b)
                              .Aggregate((acc, a) => acc + a);
        }

        public VectorD Inc(double val) {
            return new VectorD(this._value.Select(v =>
                v >= MIN ? v + val :
                v <= -MIN ? v - val :
                v));
        }

        public VectorD UnitVector => new VectorD(X / Magnitude, Y / Magnitude);

        public static VectorD operator -(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another, () => new VectorD(one._value.Zip(another._value, (a, b) => a - b)));

        public static VectorD operator -(VectorD one, double another) =>
            ThrowIfEmptyOrApply(one, VectorD.Zero2D, () => new VectorD(one._value.Select((a) => a - another)));

        public static VectorD operator +(VectorD one, VectorD another) =>
            ThrowIfEmptyOrApply(one, another, () => new VectorD(one._value.Zip(another._value, (a, b) => a + b)));

        public static VectorD operator +(VectorD one, double another) =>
            ThrowIfEmptyOrApply(one, VectorD.Zero2D, () => new VectorD(one._value.Select((a) => a + another)));

        public static VectorD operator +(VectorD one, Vector another) =>
            one.IsEmpty || another.IsEmpty ? throw new InvalidOperationException("Cannot operate on an empty vector") :
            new VectorD(one._value.Zip(another.Value, (a, b) => a + b));

        public static VectorD operator /(VectorD dividend, double divisor) =>
            ThrowIfEmptyOrApply(dividend, VectorD.Zero2D,
            () => new VectorD(dividend._value.Select(e => divisor < MIN ?
                throw new InvalidOperationException("Can't divide by zero") : e / divisor)));

        public static VectorD operator /(double dividend, VectorD divisor) {
            if (divisor.IsZero()) {
                throw new InvalidOperationException("Can't divide by zero");
            }
            return ThrowIfEmptyOrApply(divisor, VectorD.Zero2D,
            () => new VectorD(divisor._value.Select(
                e => Math.Abs(e) < MIN ? 0 : dividend / e)));
        }

        public static VectorD operator /(VectorD dividend, VectorD divisor) {
            if (divisor.IsZero()) {
                throw new InvalidOperationException("Can't divide by zero");
            }
            return ThrowIfEmptyOrApply(divisor, VectorD.Zero2D,
            () => new VectorD(dividend._value.Zip(divisor._value,
                (a, b) => Math.Abs(b) < MIN ? 0 : a / b)));
        }
        public static VectorD operator *(VectorD one, double another) =>
            ThrowIfEmptyOrApply(one, VectorD.Zero2D, () => new VectorD(one._value.Select(e => e * another)));

        public override bool Equals(object obj) => this.Equals((VectorD)obj);
        public override int GetHashCode() =>
            IsEmpty ? _value.GetHashCode() :
            ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<int>.Default);
        public override string ToString() => IsEmpty ? "<empty>" : _value.Length == 0 ? "--" : String.Join("x", _value.Select(v => v.ToString("F2")));
        public bool Equals(VectorD another) =>
            (this.IsEmpty && another.IsEmpty)
            || (!this.IsEmpty && !another.IsEmpty && this._value.Zip(another._value, (a, b) => Math.Abs(a - b) < MIN).All(a => a));

        private static T ThrowIfEmptyOrApply<T>(VectorD one, VectorD another, Func<T> apply) {
            if (one.IsEmpty || another.IsEmpty)
                throw new InvalidOperationException("Cannot operate on an empty vector");
            return apply();
        }

        internal static VectorD Parse(string v) =>
            new VectorD(v.Trim().Split('x').Select(s => {
                double val;
                if (!Double.TryParse(s, out val)) {
                    throw new FormatException($"Input string was not in a correct format ({s}).");
                }
                return val;
            }));
    }
}