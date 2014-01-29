using System.Collections.Generic;

namespace Optimisation
{
    public class Simplex : ISimplex
    {
        private const double DimensionalStepSizeForInitialisingSimplex = 0.25; // HARDCODING Expose this as a parameter in Dhs
        private IVertex _initialVertex;
        public Simplex(IVertex initialVertex) 
        {
            _initialVertex = initialVertex;
            Vertices = new Dictionary<int, IVertex>();
            Create(_initialVertex);
        }

        public Dictionary<int, IVertex> Vertices { get; set; }

        public void Create(IVertex vertex)
        {
            Vertices.Clear();
            Vertices.Add(1, vertex);
            for (int i = 0; i < vertex.Parameters.Length; i++)
            {
                var copiedVertex = vertex.Parameters.GetAsCopy();
                copiedVertex[i] = vertex.Parameters[i] + DimensionalStepSizeForInitialisingSimplex;
                var thisStepVert = new Vertex(copiedVertex);
                Vertices.Add(i + 2, thisStepVert);
            }
        }
    }
}