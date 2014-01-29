using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.Parameters;

namespace Optimisation
{
    public class DownHillSimplex : IDownHillSimplex
    {
        public bool SaveDebugSimplexPathDataToSolutionReport { get; set; }
        public double[] InitialGuess { get; set; }
        public ISimplex InitialSimplex { get; set; }
        public IVertex InitialVertex { get; set; }
        public ISimplex CurrentSimplex { get; set; }
        public double AlphaReflection { get; set; }
        public double GammaReflectionExpansion { get; set; }
        public double RhoContraction { get; set; }
        public double SigmaReduction { get; set; }
        public int MaxNumberOfIterations { get; set; }
        public int MinNumberOfIterations { get; set; }
        public double ObjectiveMeasureTolerance { get; set; }


        public DownHillSimplex()
        {
            LoadConstants();
        }

        public DownHillSimplex(double[] initialParameters)
        {
            InitialGuess = initialParameters;
            var kernel = NinJectBindings.GetBoundKernel();
            var initialVertex = kernel.Get<IVertex>();
            initialVertex.Parameters = InitialGuess;
            InitialSimplex = kernel.Get<ISimplex>();
            InitialSimplex.Create(initialVertex);
            CurrentSimplex = InitialSimplex;
            LoadConstants();
        }

        public DownHillSimplex(IVertex vertex, ISimplex initialSimplex)
        {
            InitialVertex = vertex;
            InitialGuess = InitialVertex.Parameters;
            InitialSimplex = initialSimplex;
            InitialSimplex.Create(InitialVertex);
            CurrentSimplex = InitialSimplex;
            LoadConstants();
        }

        private void LoadConstants()
        {
            AlphaReflection = 1d;
            GammaReflectionExpansion = 2d;
            RhoContraction = -0.5;
            SigmaReduction = 0.5;
            MaxNumberOfIterations = 1000;
            MinNumberOfIterations = 100;
            ObjectiveMeasureTolerance = 0.000001;
            SaveDebugSimplexPathDataToSolutionReport = false;
        }

        public ISolutionReport GetSolution()
        {
            if (!IsValid())
            {
                return new SolutionReport
                    {
                        Status = SolutionReportStatus.Failed
                    };
            }
            var solutionReport = Solve(ObjectiveFunction, GetAverageOfAllButWorstVertices, GetAlteredVertex,
                           DoesReflectedSimplexPass, 
                           DoesContractedSimplexPass,
                           ApplyReductionToCurrentSimplex);
            solutionReport.Status = SolutionReportStatus.Success;
            return solutionReport;
        }

        public ISolutionReport Solve(Func<IVertex, double> getObjectiveFunction, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex, Func<Func<IVertex, double>, Func<StandardKernel, int, IVertex>, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesReflectedSimplexPass, Func<Func<IVertex, double>, 
            Func<StandardKernel, int, IVertex>, Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesContractedSimplexPass, 
            Action<IDictionary<int, double>, Func<IVertex, double>> applyReductionToCurrentSimplex)
        {
            var kernel = NinJectBindings.GetBoundKernel();
            var solutionReport = kernel.Get<ISolutionReport>();
            int counter = 0;
            bool hasReachedTolerance = false;
            var measures = CurrentSimplex.GetObjectiveMeasures(getObjectiveFunction);

            while (counter < MaxNumberOfIterations && !hasReachedTolerance)
            {
                if (SaveDebugSimplexPathDataToSolutionReport)
                {
                    var simplexCopy = CurrentSimplex.GetAsCopy();
                    var measuresCopy = measures.GetAsCopy();
                    solutionReport.SimplexPath.Add(counter, simplexCopy);
                    solutionReport.ObjectiveMeasuresPath.Add(counter, measuresCopy);
                }

                hasReachedTolerance = measures.Min(e => e.Value) - 0d <=
                                      ObjectiveMeasureTolerance && counter >= MinNumberOfIterations - 1;

                counter++;
                var didReflectionWork = doesReflectedSimplexPass(getObjectiveFunction, getAverageOfAllButWorstVertices, getAlteredVertex, measures);
                if (didReflectionWork)
                {
                    solutionReport.SimplexTransformationPath.Add(counter, Transformation.Reflection);
                    continue;
                }
                var didContractionWork = doesContractedSimplexPass(getObjectiveFunction, getAverageOfAllButWorstVertices, getAlteredVertex, measures);
                if (didContractionWork)
                {
                    solutionReport.SimplexTransformationPath.Add(counter, Transformation.Contraction);
                    continue; 
                }
                solutionReport.SimplexTransformationPath.Add(counter, Transformation.Reduction);
                applyReductionToCurrentSimplex(measures, getObjectiveFunction);
            }
            
            var solution =
                        CurrentSimplex.GetObjectiveMeasures(getObjectiveFunction).OrderByDescending(e => e.Value).ToList()[CurrentSimplex.Vertices.Count - 1];
            solutionReport.ParameterSet = CurrentSimplex.Vertices[solution.Key].Parameters;
            solutionReport.NumOfIterationsMade = counter;
            solutionReport.ObjectiveMeasure = solution.Value;
            return solutionReport;
        }

        public Func<IVertex, double> ObjectiveFunction { get; set; }
       
        public bool IsValid()
        {
            var one = InitialSimplex.Vertices.Count > 0 && InitialSimplex.Vertices.Count == InitialGuess.Length + 1;
            var two = ObjectiveFunction != null;
            return one && two;
        }

        public void ApplyReductionToCurrentSimplex(IDictionary<int, double> measures, Func<IVertex, double> getObjectiveFunction)
        {
            var minVal = measures.Min(f => f.Value);
            var bestValueAsKey = measures.First(e => e.Value == minVal).Key;
            foreach (var vertex in CurrentSimplex.Vertices.Where(e => e.Key != bestValueAsKey))
            {
                for (int i = 0; i < CurrentSimplex.Vertices.Count - 1; i++)
                {
                    var bestValParam = CurrentSimplex.Vertices[bestValueAsKey].Parameters[i];
                    vertex.Value.Parameters[i] = bestValParam + SigmaReduction * (vertex.Value.Parameters[i] - bestValParam);
                }
                measures[vertex.Key] = getObjectiveFunction(vertex.Value);
            }
        } 

        public bool DoesContractedSimplexPass(Func<IVertex, double> getObjectiveMeasure, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices,
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex, IDictionary<int,double> objectiveMeasures )
        {
            var numParams = CurrentSimplex.Vertices.Count - 1;
            ISimplex contractedSimplex = CurrentSimplex.GetAsCopy();
            var firstWorstValueAsKey = objectiveMeasures.OrderByDescending(e => e.Value).ToList()[0].Key;
            IVertex worstVertex = contractedSimplex.Vertices[firstWorstValueAsKey];
            var kernel = NinJectBindings.GetBoundKernel();

            IVertex avgVertex = getAverageOfAllButWorstVertices(kernel, firstWorstValueAsKey);
            var contractedVertex = getAlteredVertex(kernel, avgVertex, worstVertex, RhoContraction);
            var contractedVertexObjMeasure = getObjectiveMeasure(contractedVertex);

            if (contractedVertexObjMeasure >= objectiveMeasures[firstWorstValueAsKey])
            {
                return false;
            }
            AssignVertexToCurrentSimplex(numParams, contractedSimplex, firstWorstValueAsKey, contractedVertex);
            objectiveMeasures[firstWorstValueAsKey] = contractedVertexObjMeasure;
            return true;
        }



        public bool DoesReflectedSimplexPass(Func<IVertex, double> getObjectiveMeasure, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices,
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex, IDictionary<int, double> objectiveMeasures)
        {
            var numParams = CurrentSimplex.Vertices.Count - 1;
            ISimplex reflectedSimplex = CurrentSimplex.GetAsCopy();
            var firstWorstValueAsKey = objectiveMeasures.OrderByDescending(e => e.Value).ToList()[0].Key;
            IVertex worstVertex = reflectedSimplex.Vertices[firstWorstValueAsKey];
            var secondWorstValueAsKey = objectiveMeasures.OrderByDescending(e => e.Value).ToList()[1].Key;
            var bestValueAsKey = objectiveMeasures.OrderByDescending(e => e.Value).ToList()[numParams].Key;
            var kernel = NinJectBindings.GetBoundKernel();

            IVertex avgVertex = getAverageOfAllButWorstVertices(kernel, firstWorstValueAsKey);
            var reflectedVertex = getAlteredVertex(kernel, avgVertex, worstVertex, AlphaReflection);
            var reflectedVertexObjMeasure = getObjectiveMeasure(reflectedVertex);

            if (reflectedVertexObjMeasure >= objectiveMeasures[secondWorstValueAsKey])
            {
                return false;
            }

            if (reflectedVertexObjMeasure >= objectiveMeasures[bestValueAsKey] && reflectedVertexObjMeasure < objectiveMeasures[secondWorstValueAsKey])
            {
                AssignVertexToCurrentSimplex(numParams, reflectedSimplex, firstWorstValueAsKey, reflectedVertex);
                objectiveMeasures[firstWorstValueAsKey] = reflectedVertexObjMeasure;
                return true;
            }

            var expandedVertex = getAlteredVertex(kernel, avgVertex, worstVertex, GammaReflectionExpansion);
            var expandedVertexObjMeasure = getObjectiveMeasure(expandedVertex);
            if (expandedVertexObjMeasure < reflectedVertexObjMeasure)
            {
                AssignVertexToCurrentSimplex(numParams, reflectedSimplex, firstWorstValueAsKey, expandedVertex);
                objectiveMeasures[firstWorstValueAsKey] = expandedVertexObjMeasure;
                return true;
            }

            AssignVertexToCurrentSimplex(numParams, reflectedSimplex, firstWorstValueAsKey, reflectedVertex);
            objectiveMeasures[firstWorstValueAsKey] = reflectedVertexObjMeasure;
            return true;
        }

        public void AssignVertexToCurrentSimplex(int numParams, ISimplex simplex, int firstWorstValueAsKey, IVertex vertex)
        {
            for (int i = 0; i < numParams; i++)
            {
                simplex.Vertices[firstWorstValueAsKey].Parameters[i] = vertex.Parameters[i];
            }
            CurrentSimplex = simplex;
        }

        public IVertex GetAlteredVertex(StandardKernel kernel, IVertex avgVertex, IVertex worstVertex, double alterationFactor)
        {
            var vertex = kernel.Get<IVertex>(new ConstructorArgument("parameters", new double[CurrentSimplex.Vertices.Count - 1]));
            for (int index = 0; index < vertex.Parameters.Length; index++)
            {
                vertex.Parameters[index] = avgVertex.Parameters[index] +
                                                    alterationFactor *
                                                    (avgVertex.Parameters[index] - worstVertex.Parameters[index]);
            }
            return vertex;
        }

        public IVertex GetAverageOfAllButWorstVertices(StandardKernel kernel, int firstWorstValueAsKey)
        {
            int numParams = CurrentSimplex.Vertices.Count - 1;
            var avgVertex = kernel.Get<IVertex>(new ConstructorArgument("parameters", new double[numParams]));
            for (int index = 0; index < numParams; index++)
            {
                foreach (var vertex in CurrentSimplex.Vertices.Where(vertex => vertex.Key != firstWorstValueAsKey))
                {
                    avgVertex.Parameters[index] += vertex.Value.Parameters[index];
                }
                avgVertex.Parameters[index] /= (numParams);
            }
            return avgVertex;
        }
    }

}