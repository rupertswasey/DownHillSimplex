using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Optimisation;

namespace DownHillSimplexTests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void Simplex_GetObjectiveMeasures()
        {
            // Arrange
            var params1 = new[] { 1.2, 1.3 };
            var params2 = new[] { 1.1, 2.3 };
            var kernel = NinJectBindings.GetBoundKernel();
            var simplex = kernel.Get<Simplex>();
            simplex.Vertices = new Dictionary<int, IVertex>
                                   {
                                       {1, new Vertex(params1)},
                                       {2, new Vertex(params2)}
                                   };

            // Act
            var objMeasures = simplex.GetObjectiveMeasures(vertex => ObjectiveMeasure.SumOfSquares(vertex));

            // Assert
            Assert.IsTrue(objMeasures[1].IsEqualTo(3.13));
            Assert.IsTrue(objMeasures[2].IsEqualTo(6.5));
        }

        [TestMethod]
        public void Simplex_GetAsCopy()
        {
            // Arrange
            var kernel = NinJectBindings.GetBoundKernel();
            var firstSimplex = kernel.Get<Simplex>();
            var vertex = kernel.Get<Vertex>();

            // Act
            var secondSimplex = firstSimplex.GetAsCopy();
            secondSimplex.Vertices.Add(2, vertex);

            // Assert
            Assert.AreNotEqual(firstSimplex.Vertices.Count, secondSimplex.Vertices.Count);
        }
    }
}