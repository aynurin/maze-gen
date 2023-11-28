
namespace Nour.Play.Areas.Evolving {
    public interface IAreaForceProducer {
        VectorD GetAreaForce(
            FloatingArea area, FloatingArea other);
    }
}