namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Informations détaillées d'un bus - Version améliorée avec statuts précis
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
        
        // Nouvelles propriétés pour l'affichage amélioré
        public TypeStatutBus Statut { get; set; }
        public int TempsRestant { get; set; }
        public string Destination { get; set; }
        public string SensNom { get; set; }

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
            Statut = TypeStatutBus.EnRoute;
            TempsRestant = 0;
            Destination = string.Empty;
            SensNom = "aller";
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
        /// Formatage amélioré pour l'affichage textuel
        /// </summary>
        public string FormaterPourAffichageAmeliore()
        {
            var statutTexte = Statut switch
            {
                TypeStatutBus.AArret => "À l'arrêt",
                TypeStatutBus.EnRoute => "En circulation",
                TypeStatutBus.Arrivee => "Vient d'arriver",
                TypeStatutBus.Depart => "Vient de partir",
                _ => "En circulation"
            };

            var tempsInfo = Statut switch
            {
                TypeStatutBus.AArret => $"Temps d'arrêt restant : {TempsRestant} min",
                TypeStatutBus.EnRoute => $"Arrivée prévue dans : {TempsRestant} min",
                TypeStatutBus.Arrivee => "Vient d'arriver à l'arrêt",
                TypeStatutBus.Depart => "Vient de quitter l'arrêt",
                _ => $"Temps restant : {TempsRestant} min"
            };

            var destinationInfo = !string.IsNullOrEmpty(Destination) 
                ? $" (direction {Destination})" 
                : "";

            return $"Le bus immatriculé : {Immatriculation}\n" +
                   $"{statutTexte}\n" +
                   $"De : {LieuDepart}\n" +
                   $"Vers : {LieuArrivee}\n" +
                   $"Sens circulation : {SensNom}{destinationInfo}\n" +
                   $"{tempsInfo}";
        }

        /// <summary>
        /// Formate pour l'affichage console simple
        /// </summary>
        public string FormaterPourConsole()
        {
            return $"{Heure} - Info : Sur la ligne : {Bus}\n" +
                   FormaterPourAffichageAmeliore();
        }

        /// <summary>
        /// Version compacte pour les logs
        /// </summary>
        public string FormaterCompact()
        {
            var statutCourt = Statut switch
            {
                TypeStatutBus.AArret => "ARRET",
                TypeStatutBus.EnRoute => "ROUTE",
                TypeStatutBus.Arrivee => "ARRIVE",
                TypeStatutBus.Depart => "PART",
                _ => "ROUTE"
            };

            return $"{Heure} | {Immatriculation} | {statutCourt} | {LieuDepart} → {LieuArrivee} | {SensNom} | {TempsRestant}min";
        }
    }

    /// <summary>
    /// Types de statuts pour un bus
    /// </summary>
    public enum TypeStatutBus
    {
        AArret,     // Bus en stationnement à un arrêt
        EnRoute,    // Bus en déplacement entre arrêts
        Arrivee,    // Bus vient d'arriver (notification)
        Depart      // Bus vient de partir (notification)
    }
}