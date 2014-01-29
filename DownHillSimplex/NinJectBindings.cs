using System.Collections.Generic;
using Ninject;

namespace Optimisation
{
    public static class NinJectBindings
    {
        public static StandardKernel GetBoundKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<IDownHillSimplex>().To<DownHillSimplex>();
            kernel.Bind<IVertex>().To<Vertex>();
            kernel.Bind<ISimplex>().To<Simplex>();
            kernel.Bind<ISolutionReport>().To<SolutionReport>();
            kernel.Bind<IDictionary<int, double>>().To<Dictionary<int, double>>();
            return kernel;
        }
    }
}