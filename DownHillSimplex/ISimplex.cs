using System.Collections.Generic;

namespace Optimisation
{
    public interface ISimplex
    {
        Dictionary<int, IVertex> Vertices { get; set; }
        void Create(IVertex vertex);
    }
}