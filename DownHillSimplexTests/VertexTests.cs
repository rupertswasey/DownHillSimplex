using Microsoft.VisualStudio.TestTools.UnitTesting;
using Optimisation;

namespace DownHillSimplexTests
{
    [TestClass]
    public class VertexTests
    {
        [TestMethod]
        public void VertexCreate_Instantiates()
        {
            // Arrange
            var parameters = new[] {1.0, 1.0, 1.0, 1.0};

            // Act
            var vertex = new Vertex(parameters);

            // Assert
            Assert.IsTrue(vertex.Parameters == parameters);

        }
    }
}