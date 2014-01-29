using System;
using System.Linq;
using Optimisation;

namespace RunDownHillSimplex
{
    // An example of using the downhill simplex class to solve the Rosenbrock function in 10 dimensions
    internal class Program
    {
        // Some inputs
        private const int NumDimensions = 10;
        private const bool CreatePathDataForDebugging = false;
        private const int PathIntervalToBeDisplayedWhenDebugging = 100;

        static void Main()
        {
            // Set up a some initial parameters
            var initialParameterSet = new double[NumDimensions];
            var rng = new Random(3);
            foreach (var i in Enumerable.Range(0, initialParameterSet.Length))
            {
                initialParameterSet[i] = rng.Next(-10, 10);
            }

            // Create a downhill simplex object and change a few of the default settings.
            var dhs = new DownHillSimplex(initialParameterSet);
            dhs.MinNumberOfIterations = 10;
            dhs.MaxNumberOfIterations = 10000;
            dhs.ObjectiveMeasureTolerance = 0.000001;

            // Create and assign an objective function. This needs to have an output minimum of 0 E.g. like a sum of squares type function
            Func<IVertex, double> objFunctionRosenbrock = vert =>
                        {
                            var sum = 0.0;
                            for (int i = 0; i < vert.Parameters.Length - 1; i++)
                            {
                                sum += (1 - vert.Parameters[i]) * (1 - vert.Parameters[i]) + 
                                    100 * 
                                    (vert.Parameters[i + 1] - vert.Parameters[i] * vert.Parameters[i]) * 
                                    (vert.Parameters[i + 1] - vert.Parameters[i] * vert.Parameters[i]);
                            }
                            return sum;
                        };
            dhs.ObjectiveFunction = objFunctionRosenbrock;

            // Setting this to true will generate all the path data
            // E.g. At each step you can see the current parameters, the objectives measures and the type of transformation
            // from either of Reflection, Contraction or Reduction. Note that the transformation info is lagged so if iteration 61 says 
            // Reflection then it means iteration 62 has been reflected and not iteration 61.
            dhs.SaveDebugSimplexPathDataToSolutionReport = CreatePathDataForDebugging;

            // Run the method
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            var solution = dhs.GetSolution();
            stopWatch.Stop();

            // Format string together the output
            string paramSet;
            string initialParamSet;
            try
            {
                paramSet = solution.ParameterSet.Aggregate(string.Empty, (current, p) => current + (Math.Round(p, 4) + "; "));
                initialParamSet = initialParameterSet.Aggregate(string.Empty, (current, p) => current + (Math.Round(p, 4) + "; "));
            }
            catch(Exception)
            {
                paramSet = string.Empty;
                initialParamSet = string.Empty;
            }

             // Print the output
            Console.WriteLine("Unit tested code:");
            Console.WriteLine("Initial Params: {0}", initialParamSet);
            Console.WriteLine("Run status: {0}\nNumber of iterations: {1}\nObjective measure: {2}\nSolution parameters: {3}",
                solution.Status,
                solution.NumOfIterationsMade, Math.Round(solution.ObjectiveMeasure,4), paramSet);
            Console.WriteLine("Time taken (ms): {0}", stopWatch.ElapsedMilliseconds);

            // Information for debugging
            #region If CreatePathDataForDebugging is set to true then the following code block will be run
            if (dhs.SaveDebugSimplexPathDataToSolutionReport)
            {
                for (int i = 0; i < solution.NumOfIterationsMade; i++)
                {
                    if (i % PathIntervalToBeDisplayedWhenDebugging != 0)
                    {
                        continue;
                    }
                    var measures = string.Empty;
                    var vertices = "(";
                    for (int j = 0; j <= solution.ParameterSet.Length; j++)
                    {
                        measures += Math.Round(solution.ObjectiveMeasuresPath[i][j],4) + "; ";
                        vertices = solution.SimplexPath[i].Vertices[j + 1].Parameters.Aggregate(vertices, (current, p) => current + (Math.Round(p,4) + " "));
                        vertices += "/ ";
                    }
                    vertices += ")";
                    Console.WriteLine("Iteration Number: {0}", i);
                    Console.WriteLine("Transformation type: {0}", solution.SimplexTransformationPath[i + 1]);
                    Console.WriteLine("Measures: {0}", measures);
                    Console.WriteLine("Vertices: {0}", vertices);
                }
            }
            #endregion

            Console.ReadLine();
        }
    }
}
