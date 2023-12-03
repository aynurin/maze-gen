using System;

namespace Nour.Play {
    public class Optional<T>
    where T : class {

        public bool HasValue { get; private set; }

        private readonly T _value;

        // TODO: Test this with null
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
                if (!HasValue) throw new InvalidOperationException("This optional doesn't have value. Please use 'HasValue' prior to accessing 'Value'");
                return _value;
            }
        }

        public override bool Equals(object obj) {
            if (obj is T) {
                return this.Value.Equals(obj);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            if (HasValue) {
                return _value.GetHashCode();
            }
            return base.GetHashCode();
        }

        public override string ToString() {
            return $"Optional<{typeof(T).Name}>" +
                $"({(HasValue ? _value.ToString() : "<empty>")})";
        }

        public static explicit operator T(Optional<T> optional) =>
            optional.HasValue ? optional.Value :
            throw new InvalidOperationException($"This Optional<{typeof(T).Name}> is empty");

        public static implicit operator Optional<T>(T val) => new Optional<T>(val);
    }
}