using System.Collections.Generic;
using Nour.Play.Areas;

public class GeneratorOptions {
    public FillFactorOption FillFactor { get; set; }
    public MapAreaOptions MapAreasOptions { get; set; }
    public List<MapArea> MapAreas { get; set; }
    public RandomAreaGenerator.GeneratorSettings AreaGeneratorSettings { get; set; } = RandomAreaGenerator.GeneratorSettings.Default;

    public enum FillFactorOption {
        Full,
        FullWidth,
        FullHeight,
        Quarter,
        Half,
        ThreeQuarters,
        NinetyPercent
    }

    public enum MapAreaOptions {
        Manual,
        Auto
    }
}