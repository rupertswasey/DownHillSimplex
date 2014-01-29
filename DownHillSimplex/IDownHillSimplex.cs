using System;
using System.Collections.Generic;
using Ninject;

namespace Optimisation
{
    public interface IDownHillSimplex
    {
        bool SaveDebugSimplexPathDataToSolutionReport { get; set; }
        ISolutionReport GetSolution();
        ISolutionReport Solve(Func<IVertex, double> getObjectiveFunction, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex, Func<Func<IVertex, double>, Func<StandardKernel, int, IVertex>, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesReflectedSimplexPass, Func<Func<IVertex, double>, 
            Func<StandardKernel, int, IVertex>, Func<StandardKernel, IVertex, IVertex, double, IVertex>, IDictionary<int, double>, bool> doesContractedSimplexPass, 
            Action<IDictionary<int, double>, Func<IVertex, double>> applyReductionToCurrentSimplex);
        int MaxNumberOfIterations { get; set; }
        int MinNumberOfIterations { get; set; }
        double ObjectiveMeasureTolerance { get; set; }
        double AlphaReflection { get; set; }
        double GammaReflectionExpansion { get; set; }
        double RhoContraction { get; set; }
        double SigmaReduction { get; set; }
        double[] InitialGuess { get; set; }
        ISimplex InitialSimplex { get; set; }
        Func<IVertex, double> ObjectiveFunction { get; set; }
        bool IsValid();
        bool DoesReflectedSimplexPass(Func<IVertex, double> objectiveMeasure, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getReflectedVertex, IDictionary<int, double> objectiveMeasures);
        ISimplex CurrentSimplex { get; set; }
        IVertex GetAverageOfAllButWorstVertices(StandardKernel kernel, int firstWorstValueAsKey);
        void AssignVertexToCurrentSimplex(int numParams, ISimplex simplex, int firstWorstValueAsKey, IVertex vertex);
        IVertex GetAlteredVertex(StandardKernel kernel, IVertex avgVertex, IVertex worstVertex, double alterationFactor);
        bool DoesContractedSimplexPass(Func<IVertex, double> getObjectiveMeasure, Func<StandardKernel, int, IVertex> getAverageOfAllButWorstVertices, 
            Func<StandardKernel, IVertex, IVertex, double, IVertex> getAlteredVertex, IDictionary<int, double> objectiveMeasures);
        void ApplyReductionToCurrentSimplex(IDictionary<int, double> measures, Func<IVertex, double> getObjectiveFunction);
    }
}