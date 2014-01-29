using System;
using System.Collections.Generic;
using Ninject;

namespace Optimisation
{
    public static class DownHillSimplexExtensions
    {
        public static double[] GetAsCopy(this double[] values)
        {
            var array = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                array[i] = values[i];
            }
            return array;
        }

        public static IDictionary<int,double> GetAsCopy(this IDictionary<int,double> thisDictionary)
        {
            var kernel = NinJectBindings.GetBoundKernel();
            var dictionary = kernel.Get<IDictionary<int, double>>();
            for (int i = 0; i < thisDictionary.Count; i++)
            {
                dictionary.Add(i, thisDictionary[i + 1]);
            }
            return dictionary;
        }
        public static ISimplex GetAsCopy(this ISimplex thisSimplex)
        {
            var kernel = NinJectBindings.GetBoundKernel();
            var simplex = kernel.Get<Simplex>();
            simplex.Vertices.Clear();
            var numParams = thisSimplex.Vertices.Count - 1;
            foreach (var v in thisSimplex.Vertices)
            {
                var vertex = kernel.Get<Vertex>();
                vertex.Parameters = new double[numParams];
                for (int i = 0; i < numParams; i++)
                {
                    vertex.Parameters[i] = v.Value.Parameters[i];
                }
                simplex.Vertices.Add(v.Key, vertex);
            }
            return simplex;
        }

        public static IDictionary<int, double> GetObjectiveMeasures(this ISimplex thisSimplex, Func<IVertex, double> objectiveFunction)
        {
            var kernel = new StandardKernel();
            IDictionary<int, double> measures = kernel.Get<Dictionary<int, double>>();
            foreach (var vertex in thisSimplex.Vertices)
            {
                var thisMeasure = objectiveFunction(vertex.Value);
                measures.Add(vertex.Key, thisMeasure);
            }
            return measures;
        }
    }
}