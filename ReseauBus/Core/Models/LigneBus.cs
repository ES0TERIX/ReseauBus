using System.Drawing;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Ligne de bus - Conforme au diagramme UML
    /// </summary>
    public class LigneBus
    {
        public string Nom { get; set; }
        public List<Arret> ListArret { get; set; }
        public List<Evenement> ListeEvenements { get; set; }
        public string ListeInfo { get; set; }
        public Color Couleur { get; set; }

        public LigneBus(string nom, Color couleur)
        {
            Nom = nom;
            Couleur = couleur;
            ListArret = new List<Arret>();
            ListeEvenements = new List<Evenement>();
            ListeInfo = string.Empty;
        }

        /// <summary>
        /// Filtre les événements par heure
        /// </summary>
        public List<Evenement> FiltrerParHeure(string heureDebut, string heureFin)
        {
            // Logique de filtrage par heure
            return ListeEvenements.Where(e => 
                string.Compare(e.Heure, heureDebut) >= 0 && 
                string.Compare(e.Heure, heureFin) <= 0).ToList();
        }

        /// <summary>
        /// Ajoute un arrêt à la ligne
        /// </summary>
        public void AjouterArret(Arret arret)
        {
            ListArret.Add(arret);
        }

        /// <summary>
        /// Ajoute un événement à la ligne
        /// </summary>
        public void AjouterEvenement(Evenement evenement)
        {
            ListeEvenements.Add(evenement);
            
            // Mettre à jour la liste d'infos
            if (!string.IsNullOrEmpty(ListeInfo))
            {
                ListeInfo += "\n";
            }
            ListeInfo += evenement.ToString();
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