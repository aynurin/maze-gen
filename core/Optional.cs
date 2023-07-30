using System;

public class Optional<T>
    where T : class {

    private readonly T _value;

    public Optional(T value) {
        _value = value;
    }
    private Optional() {
        _value = null;
    }

    public static Optional<T> Empty() {
        return new Optional<T>();
    }

    public bool HasValue { get => _value != null; }

    public T Value {
        get {
            if (_value == null) throw new NullReferenceException("This optional doesn't have value. Please use 'HasValue' prior to accessing 'Value'");
            return _value;
        }
    }
}