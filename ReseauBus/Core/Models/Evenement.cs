namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Événement de simulation amélioré - Conforme au diagramme UML
    /// </summary>
    public class Evenement
    {
        public string Heure { get; set; }
        public string Bus { get; set; }
        public Arret Depart { get; set; }
        public Arret Arrivee { get; set; }
        public string Duree { get; set; }
        
        // Nouvelles propriétés pour un affichage amélioré
        public TypeEvenement Type { get; set; }
        public int TempsRestantMinutes { get; set; }
        public string SensDestination { get; set; } // Nom du terminus
        public int SensDirection { get; set; } // 1 ou -1

        public Evenement(string heure, string bus, Arret depart, Arret arrivee, string duree)
        {
            Heure = heure;
            Bus = bus;
            Depart = depart;
            Arrivee = arrivee;
            Duree = duree;
            Type = TypeEvenement.EnRoute;
            TempsRestantMinutes = 0;
            SensDestination = string.Empty;
            SensDirection = 1;
        }

        /// <summary>
        /// Exécute l'événement
        /// </summary>
        public void Executer()
        {
            // Logique d'exécution de l'événement
        }

        public override string ToString()
        {
            return $"{Heure} - Bus {Bus}: {Depart.Nom} → {Arrivee.Nom} ({Duree})";
        }

        /// <summary>
        /// Formatage amélioré pour l'affichage
        /// </summary>
        public string FormaterPourAffichageAmeliore(int numeroInfo)
        {
            var statut = Type switch
            {
                TypeEvenement.AArret => "À l'arrêt",
                TypeEvenement.EnRoute => "En circulation",
                TypeEvenement.Arrivee => "Arrivée",
                TypeEvenement.Depart => "Départ",
                _ => "En circulation"
            };

            var tempsInfo = Type switch
            {
                TypeEvenement.AArret => $"Temps d'arrêt restant : {TempsRestantMinutes} min",
                TypeEvenement.EnRoute => $"Arrivée prévue dans : {TempsRestantMinutes} min",
                TypeEvenement.Arrivee => "Vient d'arriver",
                TypeEvenement.Depart => "Vient de partir",
                _ => $"Temps restant : {TempsRestantMinutes} min"
            };

            var direction = SensDirection == 1 ? "aller" : "retour";
            var destination = !string.IsNullOrEmpty(SensDestination) 
                ? $" (direction {SensDestination})" 
                : "";

            return $"{Heure} - Info {numeroInfo} : Sur la ligne : [LIGNE]\n" +
                   $"   Le bus immatriculé : {Bus}\n" +
                   $"   {statut}\n" +
                   $"   De : {Depart.Nom}\n" +
                   $"   Vers : {Arrivee.Nom}\n" +
                   $"   Sens circulation : {direction}{destination}\n" +
                   $"   {tempsInfo}";
        }
    }

    /// <summary>
    /// Types d'événements pour un affichage plus précis
    /// </summary>
    public enum TypeEvenement
    {
        AArret,     // Bus à l'arrêt (temps d'arrêt en cours)
        EnRoute,    // Bus en circulation entre deux arrêts
        Arrivee,    // Bus vient d'arriver à un arrêt
        Depart      // Bus vient de partir d'un arrêt
    }
}