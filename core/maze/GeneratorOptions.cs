public class GeneratorOptions {
    public FillFactorOption FillFactor { get; set; }

    public enum FillFactorOption {
        Full,
        FullWidth,
        FullHeight,
        Quarter,
        Half,
        ThreeQuarters,
        NinetyPercent
    }
}