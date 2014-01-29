using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using Ninject.Parameters;
using Optimisation;

namespace DownHillSimplexTests
{
    [TestClass]
    public class IntegrationTests
    {
        [TestCategory("Integration")]
        [TestMethod]
        public void GetSolutionReport_UsingInitialParamsConstructor()
        {
            Func<IVertex, double> objFunction = vert =>
                {
                    var x = vert.Parameters[0];
                    var y = vert.Parameters[1];
                    return (x*x + y - 11)*(x*x + y - 11) + (x + y*y - 7)*(x + y*y - 7);
                };


            var initialGuess = new[] { 1.0, 1.0 };
            var dhs = new DownHillSimplex(initialGuess);
            dhs.ObjectiveFunction = objFunction; 

            if (!dhs.IsValid())
            {
                throw new Exception("Did not pass the IsValid method");
            }

            var solution = dhs.GetSolution();

            Assert.AreEqual(1.6368619236455376E-24, solution.ObjectiveMeasure);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GetSolutionReport_UsingEmptyConstructor()
        {
            var dhs = new DownHillSimplex();
            dhs.InitialGuess = new[] {1d, 7d};
            IVertex initialVertex = new Vertex(dhs.InitialGuess);
            dhs.InitialSimplex = new Simplex(initialVertex);
            dhs.CurrentSimplex = dhs.InitialSimplex;
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);

            if (!dhs.IsValid())
            {
                throw new Exception("Did not pass the IsValid method");
            }

            var solution = dhs.GetSolution();

            var isSolved = solution.ObjectiveMeasure.IsEqualTo(0d);
            Assert.IsTrue(isSolved);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GetSolutionReport()
        {
            var kernel = new StandardKernel();
            var initialParams = new[] { 1d, 7d };
            var initialVertex = kernel.Get<Vertex>(new ConstructorArgument("parameters", initialParams));
            var simplex = kernel.Get<Simplex>(new ConstructorArgument("initialVertex", initialVertex));
            var dhs = kernel.Get<DownHillSimplex>(new ConstructorArgument("vertex", initialVertex),
                                                  new ConstructorArgument("initialSimplex", simplex));
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);
            dhs.MinNumberOfIterations = 1;
            var solution = dhs.GetSolution();

            var isSolved = solution.ObjectiveMeasure.IsEqualTo(0d);

            Assert.AreEqual(initialParams.Length + 1, dhs.InitialSimplex.Vertices.Count);
            Assert.IsTrue(isSolved);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void RunDownHillSimplex()
        {
            var kernel = new StandardKernel();
            var initialParams = new[] { 1d, 7d};
            var initialVertex = kernel.Get<Vertex>(new ConstructorArgument("parameters", initialParams));
            var simplex = kernel.Get<Simplex>(new ConstructorArgument("initialVertex", initialVertex));
            var dhs = kernel.Get<DownHillSimplex>(new ConstructorArgument("vertex", initialVertex),
                                                  new ConstructorArgument("initialSimplex", simplex));
            dhs.ObjectiveFunction = vertex => ObjectiveMeasure.SumOfSquares(vertex);
            dhs.MinNumberOfIterations = 1;
            var solution = dhs.Solve(dhs.ObjectiveFunction, dhs.GetAverageOfAllButWorstVertices, dhs.GetAlteredVertex,
                                     dhs.DoesReflectedSimplexPass, dhs.DoesContractedSimplexPass,
                                     dhs.ApplyReductionToCurrentSimplex);

            var isSolved = solution.ObjectiveMeasure.IsEqualTo(0d);

            Assert.AreEqual(initialParams.Length + 1, dhs.InitialSimplex.Vertices.Count);
            Assert.IsTrue(isSolved);
            
        }

        

    }
}