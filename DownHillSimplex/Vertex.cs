namespace Optimisation
{
    public class Vertex : IVertex
    {
        public Vertex(double[] parameters)
        {
            Parameters = parameters;
        }
        public double[] Parameters { get; set; }
    }
}