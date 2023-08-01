using System.Diagnostics.CodeAnalysis;
using System.Linq;

public struct Dimensions {
    public static readonly Dimensions None = new Dimensions(-1, -1);

    private readonly int[] _dimensions;

    public int[] Values => _dimensions;
    public int Rows => _dimensions[0];
    public int Columns => _dimensions[1];
    public int Product => _dimensions.Aggregate((a, b) => a * b);

    public Dimensions Rotate2d() => new Dimensions(_dimensions.Reverse().ToArray());

    public Dimensions(int rows, int columns) :
        this(new int[] {rows, columns}) { }

    public Dimensions(int[] dimensions) {
        _dimensions = dimensions;
    }

    public static bool operator ==(Dimensions one, Dimensions another) => 
        one._dimensions.Equals(another._dimensions);
    public static bool operator !=(Dimensions one, Dimensions another) => 
        !one._dimensions.Equals(another._dimensions);
    public static bool operator >(Dimensions one, Dimensions another) => one.Columns > another.Columns && one.Rows > another.Rows;
    public static bool operator >=(Dimensions one, Dimensions another) => one.Columns >= another.Columns && one.Rows >= another.Rows;
    public static bool operator <(Dimensions one, Dimensions another) => one.Columns < another.Columns && one.Rows < another.Rows;
    public static bool operator <=(Dimensions one, Dimensions another) => one.Columns <= another.Columns && one.Rows <= another.Rows;
    public static Dimensions operator +(Dimensions one, Dimensions another) => new Dimensions(one._dimensions.Zip(another._dimensions, (a, b) => a + b).ToArray());
    public static Dimensions operator -(Dimensions one, Dimensions another) => new Dimensions(one._dimensions.Zip(another._dimensions, (a, b) => a - b).ToArray());

    public override string ToString() => $"{Rows}x{Columns}";
    public override bool Equals(object obj) =>
        this._dimensions.Equals(((Dimensions)obj)._dimensions);
    public override int GetHashCode() => this._dimensions.GetHashCode();
}