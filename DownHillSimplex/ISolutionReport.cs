using System.Collections.Generic;

namespace Optimisation
{
    public interface ISolutionReport
    {
        SolutionReportStatus Status { get; set; }
        int NumOfIterationsMade { get; set; }
        double ObjectiveMeasure { get; set; }
        double[] ParameterSet { get; set; }
        IDictionary<int, ISimplex> SimplexPath { get; set; }
        IDictionary<int, IDictionary<int,double>> ObjectiveMeasuresPath { get; set; }
        IDictionary<int, Transformation> SimplexTransformationPath { get; set; }
    }
}