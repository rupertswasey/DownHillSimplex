using System.Linq;

namespace Optimisation
{
    public static class ObjectiveMeasure
    {
        /// <summary>
        /// This is currently a placeholder that has a solution vector of zero
        /// </summary>
        public static double SumOfSquares(IVertex vertex)
        {
            return vertex.Parameters.Sum(val => val * val);
        }
    }
}