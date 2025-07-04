using System.Drawing;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Ligne de bus - Version nettoyée
    /// </summary>
    public class LigneBus
    {
        public string Nom { get; set; }
        public List<Arret> ListArret { get; set; }
        public Color Couleur { get; set; }

        public LigneBus(string nom, Color couleur)
        {
            Nom = nom;
            Couleur = couleur;
            ListArret = new List<Arret>();
        }

        /// <summary>
        /// Ajoute un arrêt à la ligne
        /// </summary>
        public void AjouterArret(Arret arret)
        {
            ListArret.Add(arret);
        }

        /// <summary>
        /// Retourne l'arrêt suivant dans la ligne
        /// </summary>
        public Arret? ArretSuivant(Arret arretActuel, bool sensAller = true)
        {
            int index = ListArret.IndexOf(arretActuel);
            if (index == -1) return null;

            if (sensAller)
            {
                return index < ListArret.Count - 1 ? ListArret[index + 1] : null;
            }
            else
            {
                return index > 0 ? ListArret[index - 1] : null;
            }
        }

        /// <summary>
        /// Vérifie si c'est un terminus
        /// </summary>
        public bool EstTerminus(Arret arret)
        {
            return arret == ListArret.First() || arret == ListArret.Last();
        }

        /// <summary>
        /// Retourne le nombre total d'arrêts
        /// </summary>
        public int NombreArrets => ListArret.Count;
    }
}