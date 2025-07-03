namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Informations détaillées d'un bus - Conforme au diagramme UML avec formatage amélioré
    /// </summary>
    public class InfoBus
    {
        public string Heure { get; set; }
        public string TypeErreur { get; set; }
        public string Bus { get; set; }
        public string LieuDepart { get; set; }
        public string LieuArrivee { get; set; }
        public int Sens { get; set; }
        public string Duree { get; set; }
        public string ListeEvenements { get; set; }
        public string Immatriculation { get; set; }

        public InfoBus()
        {
            Heure = string.Empty;
            TypeErreur = string.Empty;
            Bus = string.Empty;
            LieuDepart = string.Empty;
            LieuArrivee = string.Empty;
            Sens = 1;
            Duree = string.Empty;
            ListeEvenements = string.Empty;
            Immatriculation = string.Empty;
        }

        /// <summary>
        /// Ajoute un événement à la liste
        /// </summary>
        public void AjouterEvenement(Evenement evenement)
        {
            if (!string.IsNullOrEmpty(ListeEvenements))
            {
                ListeEvenements += "\n";
            }
            ListeEvenements += evenement.ToString();
        }

        /// <summary>
        /// Formate les informations pour l'affichage textuel soigné
        /// </summary>
        public string FormaterPourAffichage()
        {
            return $"Le bus immatriculé : {Immatriculation}\n" +
                   $"En circulation\n" +
                   $"De : {LieuDepart}\n" +
                   $"Vers : {LieuArrivee}\n" +
                   $"Sens circulation : {Sens}\n" +
                   $"Pour une durée de : {Duree}";
        }

        /// <summary>
        /// Formate pour l'affichage console simple
        /// </summary>
        public string FormaterPourConsole()
        {
            return $"{Heure} - Info : Sur la ligne\n" +
                   FormaterPourAffichage();
        }

        /// <summary>
        /// Version compacte pour les logs
        /// </summary>
        public string FormaterCompact()
        {
            return $"{Heure} | {Immatriculation} | {LieuDepart} → {LieuArrivee} | Sens {Sens} | {Duree}";
        }
    }
}