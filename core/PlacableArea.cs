
using System;
using System.Collections.Generic;

internal struct PlaceableArea {
    public PlaceableArea(Dimensions position, Dimensions size) {
        Position = position;
        Size = size;
    }

    public Dimensions Position { get; private set; }
    public Dimensions Size { get; private set; }

    // TODO: Implement this
    internal IEnumerable<PlaceableArea> Slice(PlaceableArea zone) {
        var xLeft = this.Position.Columns;
        var xRight = this.Position.Columns + this.Size.Columns;
        var yTop = this.Position.Rows;
        var yBottom = this.Position.Rows + this.Size.Rows;
        if (zone.Position.Columns > xRight && zone.Position.Rows > yBottom)
            yield return zone;
        else if (zone.Position.Columns + zone.Size.Columns < xLeft && zone.Position.Rows + zone.Size.Rows < yTop)
            yield return zone;
        throw new NotImplementedException();
    }

    internal bool Fits(Dimensions area) => this.Size >= area;

    internal bool Contains(PlaceableArea area) => this.Position <= area.Position && this.Position + this.Size >= area.Position + area.Size;
}