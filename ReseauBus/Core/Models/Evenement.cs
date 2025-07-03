namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Événement de simulation - Conforme au diagramme UML
    /// </summary>
    public class Evenement
    {
        public string Heure { get; set; }
        public string Bus { get; set; }
        public Arret Depart { get; set; }
        public Arret Arrivee { get; set; }
        public string Duree { get; set; }

        public Evenement(string heure, string bus, Arret depart, Arret arrivee, string duree)
        {
            Heure = heure;
            Bus = bus;
            Depart = depart;
            Arrivee = arrivee;
            Duree = duree;
        }

        /// <summary>
        /// Exécute l'événement
        /// </summary>
        public void Executer()
        {
            // Logique d'exécution de l'événement
            // Notification des observateurs, etc.
        }

        public override string ToString()
        {
            return $"{Heure} - Bus {Bus}: {Depart.Nom} → {Arrivee.Nom} ({Duree})";
        }
    }
}