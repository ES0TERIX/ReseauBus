namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Représente un arrêt de bus - Version simplifiée sans coordonnées
    /// </summary>
    public class Arret
    {
        public int Id { get; set; }
        public string Nom { get; set; }

        public Arret(int id, string nom)
        {
            Id = id;
            Nom = nom;
        }

        public override string ToString()
        {
            return $"{Id} - {Nom}";
        }
    }
}