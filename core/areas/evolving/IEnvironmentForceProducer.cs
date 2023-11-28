
namespace Nour.Play.Areas.Evolving {
    public interface IEnvironmentForceProducer {
        VectorD GetEnvironmentForce(
            FloatingArea area, Vector environmentSize);
    }
}