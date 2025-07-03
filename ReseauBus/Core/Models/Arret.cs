namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Représente un arrêt de bus avec ses coordonnées
    /// </summary>
    public class Arret
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public Arret(int id, string nom, float x, float y)
        {
            Id = id;
            Nom = nom;
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"{Id} - {Nom}";
        }

        /// <summary>
        /// Calcule la distance euclidienne entre deux arrêts
        /// </summary>
        public double DistanceVers(Arret autre)
        {
            return Math.Sqrt(Math.Pow(autre.X - X, 2) + Math.Pow(autre.Y - Y, 2));
        }
    }
}