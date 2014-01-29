using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ninject;
using Ninject.Parameters;
using Optimisation;

namespace DownHillSimplexTests
{
    [TestClass]
    public class DownHillSimplexTests
    {


        [TestMethod]
        public void Solve_MaxNumberOfIterations()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            dhs.MaxNumberOfIterations = 100;
            Func<IVertex, double> objMeasure = vertex => 51.1;
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;

            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });

            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) =>
                {
                    if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                    {
                        return mockReflectedVertex.Object;
                    }
                    if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                    {
                        return mockExpandedVertex.Object;
                    }
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            Func
                <Func<IVertex, double>, Func<StandardKernel, int, IVertex>,
                    Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int,double>, bool> doesReflectedSimplexPass
                        = (a, b, c, d) => false;
            Func
                <Func<IVertex, double>, Func<StandardKernel, int, IVertex>,
                    Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int,double>, bool> doesContractedSimplexPass
                        = (a, b, c, d) => true;

            Action<IDictionary<int, double>, Func<IVertex, double>> applyReductionToCurrentSimplex = (doubles, func) =>
                                                                                                         { };

            // Act
            ISolutionReport solutionReport = dhs.Solve(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex,
                doesReflectedSimplexPass, doesContractedSimplexPass, applyReductionToCurrentSimplex);

            // Assert
            Assert.AreEqual(100, solutionReport.NumOfIterationsMade);
        }

        [TestMethod]
        public void Solve_OnlyReflectedVertexIsCalled_MaxNumberOfIterations()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            dhs.MaxNumberOfIterations = 100;
            Func<IVertex, double> objMeasure = vertex => 51.1;
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;

            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });

            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) =>
                {
                    if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                    {
                        return mockReflectedVertex.Object;
                    }
                    if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                    {
                        return mockExpandedVertex.Object;
                    }
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            Func
                <Func<IVertex, double>, Func<StandardKernel, int, IVertex>,
                    Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesReflectedSimplexPass
                        = (a, b, c, d) => true;
            Func
                <Func<IVertex, double>, Func<StandardKernel, int, IVertex>,
                    Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesContractedSimplexPass
                        = (a, b, c, d) => true;

            Action<IDictionary<int, double>,Func<IVertex, double>> applyReductionToCurrentSimplex = (doubles,func) => { };

            // Act
            ISolutionReport solutionReport = dhs.Solve(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex,
                doesReflectedSimplexPass, doesContractedSimplexPass, applyReductionToCurrentSimplex);

            // Assert
            Assert.AreEqual(100 , solutionReport.NumOfIterationsMade);
        }

        [TestMethod]
        public void ApplyReductionToCurrentSimplex()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 50.0);
            measures.Add(2, 65.0);
            measures.Add(3, 53.0);
            Func<IVertex, double> getObjMeasure = vertex => vertex.Parameters.Sum(e => e * e);


            // Act
            dhs.ApplyReductionToCurrentSimplex(measures, getObjMeasure);

            // Assert
            Assert.AreEqual(1d, dhs.CurrentSimplex.Vertices[1].Parameters[0]);
            Assert.AreEqual(7d, dhs.CurrentSimplex.Vertices[1].Parameters[1]);
            Assert.AreEqual(1d, dhs.CurrentSimplex.Vertices[2].Parameters[0]);
            Assert.AreEqual(7.5, dhs.CurrentSimplex.Vertices[2].Parameters[1]);
            Assert.AreEqual(1.5, dhs.CurrentSimplex.Vertices[3].Parameters[0]);
            Assert.AreEqual(7d, dhs.CurrentSimplex.Vertices[3].Parameters[1]);
        }

        

        [TestMethod]
        public void DoesContractedSimplexPass_Fails()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 50.0);
            measures.Add(2, 65.0);
            measures.Add(3, 53.0);
            Func<IVertex, double> objMeasure = vertex => 70.1;
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;

            var mockContractedVertex = new Mock<IVertex>();
            mockContractedVertex.Setup(e => e.Parameters).Returns(new[] { 1.25, 7.5 });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex = (kernel, vertex1, vertex2, adjFactor) => mockContractedVertex.Object;

            // Act
            var doesContractedSimplexPass = dhs.DoesContractedSimplexPass(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsFalse(doesContractedSimplexPass);
        }

        [TestMethod]
        public void DoesContractedSimplexPass_Passes()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 50.0);
            measures.Add(2, 65.0);
            measures.Add(3, 53.0);
            Func<IVertex, double> objMeasure = vertex =>
                {
                    if (vertex.Parameters[0].IsEqualTo(1.25))
                    {
                        return 42.0;
                    }
                    return vertex.Parameters.Sum(e => e * e);
                };
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;

            var mockContractedVertex = new Mock<IVertex>();
            mockContractedVertex.Setup(e => e.Parameters).Returns(new[] { 1.25, 7.5 });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =  (kernel, vertex1, vertex2, adjFactor) => mockContractedVertex.Object;
                
            // Act
            var doesContractedSimplexPass = dhs.DoesContractedSimplexPass(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsTrue(doesContractedSimplexPass);
        }

        [TestMethod]
        public void GetContactedVertex()
        {
            // Arrange
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();

            var avgVertex = new Mock<IVertex>();
            avgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            var worstVertex = new Mock<IVertex>();
            worstVertex.Setup(e => e.Parameters).Returns(new[] { 1d, 8d });

            // Act
            var contractedVertex = dhs.GetAlteredVertex(kernel, avgVertex.Object, worstVertex.Object, dhs.RhoContraction);

            // Assert
            Assert.AreEqual(1.25, contractedVertex.Parameters[0]);
            Assert.AreEqual(7.5, contractedVertex.Parameters[1]);
        }

        [TestMethod]
        public void DoesReflectedSimplexPass_ObjectiveMeasureIsBestButNotBetterThanExpanded_ReturnsTrue()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 42.0);
            measures.Add(2, 42.0);
            measures.Add(3, 42.0);
            Func<IVertex, double> objMeasure = vertex =>
            {
                if (vertex.Parameters[0].IsEqualTo(1.25))
                {
                    return 42.0;
                }
                return vertex.Parameters.Sum(e => e * e);
            };
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;

            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });

            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) => {
                                                             if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                                                             {
                                                                 return mockReflectedVertex.Object;
                                                             }
                                                             if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                                                             {
                                                                 return mockExpandedVertex.Object;
                                                             } 
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            // Act
            var doesReflectedVertexPass = dhs.DoesReflectedSimplexPass(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsTrue(doesReflectedVertexPass);
        }

        [TestMethod]
        public void DoesReflectedSimplexPass_ObjectiveMeasureIsLessThanSecondToWorseAndGreaterThanBest_ReturnsTrue()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 51.0);
            measures.Add(2, 51.0);
            measures.Add(3, 51.0);
            Func<IVertex, double> objMeasure = vertex =>
            {
                if (vertex.Parameters[0].IsEqualTo(1.25))
                {
                    return 51.0;
                }
                return vertex.Parameters.Sum(e => e * e);
            };
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;
            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });
            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 5d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) =>
                {
                    if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                    {
                        return mockReflectedVertex.Object;
                    }
                    if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                    {
                        return mockExpandedVertex.Object;
                    }
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            // Act
            var doesReflectedVertexPass = dhs.DoesReflectedSimplexPass(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsTrue(doesReflectedVertexPass);
        }

        [TestMethod]
        public void DoesReflectedSimplexPass_ObjectiveMeasureIsGreaterThanSecondToWorse_ReturnsFalse()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 95.0);
            measures.Add(2, 95.0);
            measures.Add(3, 95.0);
            Func<IVertex, double> objMeasure = vertex => 95.1;
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] {1.5, 7d});
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;
            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] {2d, 6d});

            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 5d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) =>
                {
                    if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                    {
                        return mockReflectedVertex.Object;
                    }
                    if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                    {
                        return mockExpandedVertex.Object;
                    }
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            // Act
            var doesReflectedVertexPass = dhs.DoesReflectedSimplexPass(objMeasure, getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsFalse(doesReflectedVertexPass);
        }


        [TestCategory("Integration")]
        [TestMethod]
        public void DoesReflectedSimplexPass()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var measures = new Dictionary<int, double>();
            measures.Add(1, 50.0);
            measures.Add(2, 65.0);
            measures.Add(3, 53.0);
            
            var mockAvgVertex = new Mock<IVertex>();
            mockAvgVertex.Setup(e => e.Parameters).Returns(new[] { 1.5, 7d });
            Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices = (kernel, i) => mockAvgVertex.Object;
            var mockReflectedVertex = new Mock<IVertex>();
            mockReflectedVertex.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });

            var mockExpandedVertex = new Mock<IVertex>();
            mockExpandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 5d });

            var mockAlteredVertex = new Mock<IVertex>();
            mockAlteredVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 6d });
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex =
                (kernel, vertex1, vertex2, adjFactor) =>
                {
                    if (adjFactor.IsEqualTo(dhs.AlphaReflection))
                    {
                        return mockReflectedVertex.Object;
                    }
                    if (adjFactor.IsEqualTo(dhs.GammaReflectionExpansion))
                    {
                        return mockExpandedVertex.Object;
                    }
                    throw new Exception("AdjFactor should be taking in only one of AlphaReflection or GammaReflectionExpansion");
                };

            // Act
            var doesReflectionPass = dhs.DoesReflectedSimplexPass(vertex => ObjectiveMeasure.SumOfSquares(vertex), getAverageOfAllButWorstVertices, getAlteredVertex, measures);

            // Assert
            Assert.IsTrue(doesReflectionPass);
            Assert.AreEqual(2.5, dhs.CurrentSimplex.Vertices[2].Parameters[0]);
            Assert.AreEqual(5d, dhs.CurrentSimplex.Vertices[2].Parameters[1]);
            Assert.AreEqual(1d, dhs.CurrentSimplex.Vertices[1].Parameters[0]);
            Assert.AreEqual(7d, dhs.CurrentSimplex.Vertices[1].Parameters[1]);
            Assert.AreEqual(2d, dhs.CurrentSimplex.Vertices[3].Parameters[0]);
            Assert.AreEqual(7d, dhs.CurrentSimplex.Vertices[3].Parameters[1]);
        }

        [TestMethod]
        public void AssignExpandedVertexToCurrentSimplex()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
            var expandedVertex = new Mock<IVertex>();
            expandedVertex.Setup(e => e.Parameters).Returns(new[] { 2.5, 5d });
            int firstWorstValueAsKey = 2;
            var reflectedSimplex = new Mock<ISimplex>();
            var mockVertex1 = new Mock<IVertex>();
            var mockVertex2 = new Mock<IVertex>();
            var mockVertex3 = new Mock<IVertex>();
            mockVertex1.Setup(e => e.Parameters).Returns(new[] { 1d, 7d });
            mockVertex2.Setup(e => e.Parameters).Returns(new[] { 2d, 6d });
            mockVertex3.Setup(e => e.Parameters).Returns(new[] { 2d, 7d });
            reflectedSimplex.Setup(e => e.Vertices).Returns(new Dictionary<int, IVertex>
                                                                {
                                                                    {1, mockVertex1.Object},
                                                                    {2, mockVertex2.Object},
                                                                    {3, mockVertex3.Object}
                                                                });
            int numParams = 2;

            // Act
            dhs.AssignVertexToCurrentSimplex(numParams, reflectedSimplex.Object, firstWorstValueAsKey, expandedVertex.Object);

            // Assert
            Assert.AreEqual(2.5, dhs.CurrentSimplex.Vertices[2].Parameters[0]);
            Assert.AreEqual(5d, dhs.CurrentSimplex.Vertices[2].Parameters[1]);
        }

        [TestMethod]
        public void GetExpandedVertex()
        {
            // Arrange
            var avgVertex = new Mock<IVertex>();
            avgVertex.Setup(e => e.Parameters).Returns(new[] {1.5, 7d});
            var worstVertex = new Mock<IVertex>();
            worstVertex.Setup(e => e.Parameters).Returns(new[] { 1d, 8d });
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();

            // Act
            var vertex = dhs.GetAlteredVertex(kernel, avgVertex.Object, worstVertex.Object, dhs.GammaReflectionExpansion);

            // Assert
            Assert.AreEqual(2.5, vertex.Parameters[0]);
            Assert.AreEqual(5d, vertex.Parameters[1]);


        }
        [TestMethod]
        public void AssignReflectedVertexToCurrentSimplex()
        {
            // Arrange
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();
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
            const int firstWorstValueAsKey = 2;
            var reflectedVertex = new Mock<IVertex>();
            reflectedVertex.Setup(e => e.Parameters).Returns(new[] {2d, 6d});

            // Act
            dhs.AssignVertexToCurrentSimplex(2, mockSimplex.Object, firstWorstValueAsKey, reflectedVertex.Object);
            
            // Assert
            Assert.AreEqual(2d, dhs.CurrentSimplex.Vertices[2].Parameters[0]);
            Assert.AreEqual(6d, dhs.CurrentSimplex.Vertices[2].Parameters[1]);

        }
        [TestMethod]
        public void GetReflectedVertex()
        {
            // Arrange
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = TestHelpers.GetDhsWithMockedSimplicesAndVertices_1();

            var avgVertex = new Mock<IVertex>();
            avgVertex.Setup(e => e.Parameters).Returns(new[] {1.5, 7d});
            var worstVertex = new Mock<IVertex>();
            worstVertex.Setup(e => e.Parameters).Returns(new[] { 1d, 8d });

            // Act
            var reflectedVertex = dhs.GetAlteredVertex(kernel, avgVertex.Object, worstVertex.Object, dhs.AlphaReflection);

            // Assert
            Assert.AreEqual(2d, reflectedVertex.Parameters[0]);
            Assert.AreEqual(6d, reflectedVertex.Parameters[1]);
        }

        [TestMethod]
        public void GetAverageOfAllButWorstVertices()
        {
            // Arrange
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
                                                               {3,mockVertex3.Object}
                                                           });
            dhs.CurrentSimplex = mockSimplex.Object;
            const int worstKey = 2;

            // Act
            var avgVertex = dhs.GetAverageOfAllButWorstVertices(kernel, worstKey);

            // Assert
            Assert.AreEqual(1.5, avgVertex.Parameters[0]);
            Assert.AreEqual(7.0, avgVertex.Parameters[1]);
        }

        [TestMethod]
        public void IsValid()
        {
            // Arrange
            var initialParams = new[] { 1.2, 1.3 };
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = kernel.Get<IDownHillSimplex>();
            var mockSimplex = new Mock<ISimplex>();
            var dictionary = new Dictionary<int, IVertex>
                                 {
                                     {1, new Vertex(new double[2])},
                                     {2, new Vertex(new double[2])},
                                     {3, new Vertex(new double[2])}
                                 };
            mockSimplex.Setup(e => e.Vertices).Returns(dictionary);
            dhs.InitialSimplex = mockSimplex.Object;
            dhs.InitialGuess = initialParams;
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);

            // Act
            var isValid = dhs.IsValid();

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void IsValid_NoVertices_False()
        {
            // Arrange
            var initialParams = new[] { 1.2, 1.3 };
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = kernel.Get<IDownHillSimplex>();
            var mockSimplex = new Mock<ISimplex>();
            var dictionary = new Dictionary<int, IVertex>();
            mockSimplex.Setup(e => e.Vertices).Returns(dictionary);
            dhs.InitialSimplex = mockSimplex.Object;
            dhs.InitialGuess = initialParams;
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);

            // Act
            var isValid = dhs.IsValid();

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ObjectiveFunction()
        {
            // Arrange
            var initialParams = new[] { 1.2, 1.3};
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = kernel.Get<IDownHillSimplex>();
            var vertex = kernel.Get<Vertex>(new ConstructorArgument("parameters", initialParams));
            dhs.ObjectiveFunction = vertex1 => ObjectiveMeasure.SumOfSquares(vertex1);

            // Act
            var measure = dhs.ObjectiveFunction(vertex);

            // Assert
            Assert.AreEqual(3.13, measure);
        }


        [TestMethod]
        public void SetObjectiveFunction()
        {
            // Arrange
            var kernel = NinJectBindings.GetBoundKernel();
            var dhs = kernel.Get<IDownHillSimplex>();
            Func<IVertex, double> objectiveFunction = vertex => 2d;

            // Act
            dhs.ObjectiveFunction = objectiveFunction;
            var result = dhs.ObjectiveFunction(kernel.Get<Vertex>());

            // Assert
            Assert.AreEqual(2d, result);
        }

        

        [TestMethod]
        public void Constructor()
        {
            // Arrange
            var initialGuess = new[] {1.0, 1.0};
            var mockVertex = new Mock<IVertex>();
            mockVertex.Setup(e => e.Parameters).Returns(initialGuess);
            var mockSimplex = new Mock<ISimplex>();
            var dictionary = new Dictionary<int, IVertex>
                                 {
                                     {1, new Vertex(new double[2])},
                                     {2, new Vertex(new double[2])},
                                     {3, new Vertex(new double[2])}
                                 };
            mockSimplex.Setup(e => e.Vertices).Returns(dictionary);

            // Act
            var dhSimplex = new DownHillSimplex(mockVertex.Object, mockSimplex.Object);
            
            // Assert
            Assert.AreEqual(3, dhSimplex.InitialSimplex.Vertices.Count);
        }
    }
}