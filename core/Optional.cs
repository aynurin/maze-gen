using System;

namespace Nour.Play {
    public class Optional<T>
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
            var other = obj as Optional<T>;
            if (obj != null) {
                return this.HasValue && other.HasValue && this._value.Equals(other._value);
            }
            return false;
        }

        public static bool operator ==(Optional<T> one, Optional<T> other) =>
            one.Equals(other);

        public static bool operator !=(Optional<T> one, Optional<T> other) =>
            !one.Equals(other);

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
    }
}