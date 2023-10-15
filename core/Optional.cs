using System;
using System.Threading.Tasks;

namespace Nour.Play {
    public class Optional<T> : IEquatable<T>, IEquatable<Optional<T>>, IComparable<Optional<T>>, IComparable<T>, IComparable
    where T : class {

        public bool HasValue { get; private set; }

        private readonly T _value;

        public Optional(T value) {
            _value = value;
            if (_value != null) {
                HasValue = true;
            }
        }

        public Optional() {
            _value = null;
        }

        public static Optional<T> Empty => new Optional<T>();

        public T Value {
            get {
                if (!HasValue) throw new NullReferenceException("This optional doesn't have value. Please use 'HasValue' prior to accessing 'Value'");
                return _value;
            }
        }

        public override bool Equals(object obj) {
            if (obj is T) {
                return this.Equals(obj as T);
            } else if (obj is Optional<T>) {
                return this.Equals(obj as Optional<T>);
            }
            return false;
        }

        public static bool operator ==(Optional<T> one, Optional<T> other) =>
            one == null || other == null ? false : one.Equals(other);

        public static bool operator !=(Optional<T> one, Optional<T> other) =>
            one == null || other == null ? false : !one.Equals(other);

        public override int GetHashCode() {
            if (HasValue) {
                return _value.GetHashCode();
            }
            return base.GetHashCode();
        }

        public override string ToString() {
            return $"Optional<{typeof(T).Name}>" +
                $"({(HasValue ? _value.ToString() : "empty")})";
        }

        public bool Equals(Optional<T> other) =>
            (this.HasValue && other.HasValue) ? this.Value.Equals(other) : Object.ReferenceEquals(this, other);

        public bool Equals(T other) =>
            this.HasValue ? this.Value.Equals(other) : false;

        public int CompareTo(T other) {
            if (!HasValue) throw new InvalidOperationException($"Cannot compare to an empty Optional<{typeof(T).Name}>");
            if (other is IComparable) {
                return -((IComparable)other).CompareTo(this.Value);
            } else if (other is IComparable<T>) {
                return -((IComparable<T>)other).CompareTo(this.Value);
            }
            throw new InvalidOperationException($"{typeof(T).Name} is not IComparable");
        }

        public int CompareTo(Optional<T> other) {
            if (HasValue && other.HasValue) return CompareTo(other.Value);
            throw new InvalidOperationException($"Cannot compare empty Optional<{typeof(T).Name}> instances");
        }

        public int CompareTo(object obj) {
            if (obj is Optional<T>) {
                return CompareTo((Optional<T>)obj);
            } else if (obj is T) {
                return CompareTo((T)obj);
            }
            throw new InvalidOperationException($"Cannot compare Optional<{typeof(T).Name}> with {obj.GetType().FullName}");
        }

        public static implicit operator T(Optional<T> optional) =>
            optional.HasValue ? optional.Value :
            throw new InvalidOperationException($"This Optional<{typeof(T).Name}> is empty");

        public static implicit operator Optional<T>(T val) => new Optional<T>(val);
    }
}