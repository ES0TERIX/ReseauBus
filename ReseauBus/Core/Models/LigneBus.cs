using System.Drawing;

namespace ReseauBus.Core.Models
{
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

        public void AjouterArret(Arret arret)
        {
            ListArret.Add(arret);
        }

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

        public bool EstTerminus(Arret arret)
        {
            return arret == ListArret.First() || arret == ListArret.Last();
        }

        public int NombreArrets => ListArret.Count;
    }
}