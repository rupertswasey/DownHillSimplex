using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Optimisation;

namespace DownHillSimplexTests
{
    [TestClass]
    public class SimplexTests
    {
        [TestMethod]
        public void Create_CorrectNumberOfVerticesAdded()
        {
            // Arrange
            var mockVertex = new Mock<IVertex>();
            mockVertex.Setup(e => e.Parameters).Returns(new[] {1.0, 1.0});
            var simplex = new Simplex(mockVertex.Object);

            // Act
            simplex.Create(mockVertex.Object);

            // Assert
            Assert.IsTrue(simplex.Vertices.Count == mockVertex.Object.Parameters.Length + 1);
            Assert.AreEqual(1, simplex.Vertices[1].Parameters[0]);
        }

        [TestMethod]
        public void Create_StepSizeInAllDirections()
        {
            // Arrange
            var mockVertex = new Mock<IVertex>();
            mockVertex.Setup(e => e.Parameters).Returns(new[] { 1.0, 1.0 });
            var simplex = new Simplex(mockVertex.Object);

            // Act
            simplex.Create(mockVertex.Object);

            // Assert
            Assert.AreEqual(1d, simplex.Vertices[1].Parameters[0]);
            Assert.AreEqual(1d, simplex.Vertices[1].Parameters[1]);
            Assert.AreEqual(1.25, simplex.Vertices[2].Parameters[0]);
            Assert.AreEqual(1d, simplex.Vertices[2].Parameters[1]);
            Assert.AreEqual(1d, simplex.Vertices[3].Parameters[0]);
            Assert.AreEqual(1.25, simplex.Vertices[3].Parameters[1]);
        }

    }
}