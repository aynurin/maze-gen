
using System;
using System.Collections.Generic;

internal struct PlaceableArea {
    public PlaceableArea(Point position, Size size) {
        Position = position;
        Size = size;
    }

    public Point Position { get; private set; }
    public Size Size { get; private set; }

    // TODO: Implement this
    internal IEnumerable<PlaceableArea> Slice(PlaceableArea zone) {
        var xLeft = this.Position.Column;
        var xRight = this.Position.Column + this.Size.Columns;
        var yTop = this.Position.Row;
        var yBottom = this.Position.Row + this.Size.Rows;
        if (zone.Position.Column > xRight && zone.Position.Row > yBottom)
            yield return zone;
        else if (zone.Position.Column + zone.Size.Columns < xLeft && zone.Position.Row + zone.Size.Rows < yTop)
            yield return zone;
        throw new NotImplementedException();
    }

    internal bool Fits(Size area) => this.Size >= area;

    internal bool Contains(PlaceableArea area) => this.Position <= area.Position && this.Position + this.Size >= area.Position + area.Size;
}