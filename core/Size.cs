using System.Linq;
using System.Numerics;

public struct Size {
    private Vector<int> _value;

    public Vector<int> Value => _value;

    public int Rows => _value[0];
    public int Columns => _value[1];
    public int Area => Rows * Columns;

    public Size(int rows, int columns) : this(new int[] { rows, columns }) { }

    public Size(int[] dimensions) : this(VectorExtensions.CreateFrom(dimensions)) { }

    private Size(Vector<int> vector) {
        _value = vector;
    }

    public Size Rotate2d() {
        return new Size(Columns, Rows);
    }

    public static bool operator ==(Size one, Size another) =>
        Vector.EqualsAll(one.Value, another.Value);
    public static bool operator !=(Size one, Size another) =>
        !Vector.EqualsAll(one.Value, another.Value);
    public static bool operator >(Size one, Size another) =>
        one.Columns > another.Columns && one.Rows > another.Rows;
    public static bool operator >=(Size one, Size another) =>
        one.Columns >= another.Columns && one.Rows >= another.Rows;
    public static bool operator <(Size one, Size another) =>
        one.Columns < another.Columns && one.Rows < another.Rows;
    public static bool operator <=(Size one, Size another) =>
        one.Columns <= another.Columns && one.Rows <= another.Rows;
    public static Size operator +(Size one, Size another) =>
        new Size(Vector.Add(one.Value, another.Value));
    public static Size operator -(Size one, Size another) =>
        new Size(Vector.Subtract(one.Value, another.Value));

    public override bool Equals(object obj) =>
        Vector.EqualsAll(this.Value, ((Size)obj).Value);
    public override int GetHashCode() => this.Value.GetHashCode();
    public override string ToString() => $"{Rows}x{Columns}";
}