
using System;
using System.Numerics;

public struct Point {
    public static Point None {
        get {
            var point = new Point(new Vector<int>(0));
            point._empty = true;
            return point;
        }
    } 

    private Vector<int> _value;
    private bool _empty;

    public Vector<int> Value => _value;

    public int Row => _value[0];
    public int Column => _value[1];
    
    public Point(int rows, int columns) : this (new int[] { rows, columns }) { }

    public Point(int[] coordinates) : this(VectorExtensions.CreateFrom(coordinates)) { }

    private Point(Vector<int> vector) {
        _value = vector;
        _empty = false;
    }

    public static bool operator ==(Point one, Point another) =>
        Vector.EqualsAll(one.Value, another.Value);
    public static bool operator !=(Point one, Point another) =>
        !Vector.EqualsAll(one.Value, another.Value);
    public static bool operator >(Point one, Point another) =>
        one.Column > another.Column && one.Row > another.Row;
    public static bool operator >=(Point one, Point another) =>
        one.Column >= another.Column && one.Row >= another.Row;
    public static bool operator <(Point one, Point another) =>
        one.Column < another.Column && one.Row < another.Row;
    public static bool operator <=(Point one, Point another) =>
        one.Column <= another.Column && one.Row <= another.Row;
    public static Point operator +(Point one, Point another) =>
        new Point(Vector.Add(one.Value, another.Value));
    public static Point operator -(Point one, Point another) =>
        new Point(Vector.Subtract(one.Value, another.Value));
    public static Point operator +(Point one, Size another) =>
        new Point(Vector.Add(one.Value, another.Value));
    public static Point operator -(Point one, Size another) =>
        new Point(Vector.Subtract(one.Value, another.Value));

    public override bool Equals(object obj) =>
        Vector.EqualsAll(this.Value, ((Point)obj).Value);
    public override int GetHashCode() => this.Value.GetHashCode();
    public override string ToString() => $"{Row}x{Column}";
}