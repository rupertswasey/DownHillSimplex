using System.Collections.Generic;
using Moq;
using Ninject;
using Optimisation;

namespace DownHillSimplexTests
{
    public static class TestHelpers
    {
        public static IDownHillSimplex GetDhsWithMockedSimplicesAndVertices_1()
        {
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = kernel.Get<IDownHillSimplex>();
            var mockSimplex = new Mock<ISimplex>();
            var mockVertex1 = new Mock<IVertex>();
            var mockVertex2 = new Mock<IVertex>();
            var mockVertex3 = new Mock<IVertex>();
            mockVertex1.Setup(e => e.Parameters).Returns(new[] { 1d, 7d });
            mockVertex2.Setup(e => e.Parameters).Returns(new[] { 1d, 8d });
            mockVertex3.Setup(e => e.Parameters).Returns(new[] { 2d, 7d });
            mockSimplex.Setup(e => e.Vertices).Returns(new Dictionary<int, IVertex>
                                                           {
                                                               {1, mockVertex1.Object},
                                                               {2, mockVertex2.Object},
                                                               {3, mockVertex3.Object}
                                                           });
            dhs.CurrentSimplex = mockSimplex.Object;
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);
            return dhs;
        }

    }
}