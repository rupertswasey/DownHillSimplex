using System.Collections.Generic;

namespace Optimisation
{
    public enum Transformation
    {
        Reflection,
        Contraction,
        Reduction
    }
    public class SolutionReport : ISolutionReport
    {
        public SolutionReport()
        {
            SimplexPath = new Dictionary<int, ISimplex>();
            ObjectiveMeasuresPath = new Dictionary<int, IDictionary<int, double>>();
            SimplexTransformationPath = new Dictionary<int, Transformation>();
        }
        public SolutionReportStatus Status { get; set; }
        public int NumOfIterationsMade { get; set; }
        public double ObjectiveMeasure { get; set; }
        public double[] ParameterSet { get; set; }
        public IDictionary<int, ISimplex> SimplexPath { get; set; }
        public IDictionary<int, Transformation> SimplexTransformationPath { get; set; }
        public IDictionary<int, IDictionary<int, double>> ObjectiveMeasuresPath { get; set; }

    }
}