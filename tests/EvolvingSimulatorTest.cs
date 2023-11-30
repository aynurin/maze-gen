using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Nour.Play.Areas.Evolving {

    [TestFixture]
    internal class EvolvingSimulatorTest {
        [Test]
        public void EvolvingSimulator_ThrowsIfMaxEpochsIsZero() {
            Assert.Throws<ArgumentException>(() => new EvolvingSimulator(0, 1));
        }

        [Test]
        public void EvolvingSimulator_ReturnsEpochsIfEvolitionIsNotComplete() {
            var simulator = new EvolvingSimulator(10, 1);
            var moq = new Mock<SimulatedSystem>();
            moq.Setup(
                s => s.CompleteEpoch(
                    It.IsAny<EpochResult[]>(),
                    It.IsAny<GenerationImpact[]>()))
                .Returns(new EpochResult() { CompleteEvolution = false });
            var epochs = simulator.Evolve(moq.Object);
            Assert.AreEqual(10, epochs);
        }
    }
}