using System;

namespace DownHillSimplexTests
{
    public static class TestsExtentions
    {
        public static bool IsEqualTo(this double thisDouble, double valToCompare)
        {
            const double epsilon = 0.000001; // Arbitrarily small number for conparison in tests
            return Math.Abs(thisDouble - valToCompare) < epsilon;
        }
    }
}