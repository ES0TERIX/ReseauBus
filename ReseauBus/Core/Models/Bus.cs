using ReseauBus.Core.Interfaces;
using System.Timers;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Bus autonome et conscient de lui-même - Respecte l'heure de début de sa simulation
    /// </summary>
    public class Bus : IDisposable
    {
        private System.Timers.Timer? _timer;
        private readonly object _lock = new object();
        private bool _disposed = false;

        // Propriétés publiques
        public string Immatriculation { get; private set; }
        public LigneBus Ligne { get; private set; }
        public int ArretActuelIndex { get; private set; }
        public bool SensAller { get; private set; }
        public StatutBus Statut { get; private set; }
        public DateTime ProchainChangementStatut { get; private set; }
        public DateTime HeureDebutSimulation { get; private set; } // NOUVEAU
        public bool PeutDemarrer => Horloge.Instance.TempsActuel >= HeureDebutSimulation; // NOUVEAU
        public int TempsRestantMinutes => Math.Max(0, (int)(ProchainChangementStatut - Horloge.Instance.TempsActuel).TotalMinutes);

        // Événements pour notifier l'interface
        public event EventHandler<BusEventArgs>? StatutChange;
        public event EventHandler<BusEventArgs>? ArriveeArret;
        public event EventHandler<BusEventArgs>? DepartArret;

        public Bus(string immatriculation, LigneBus ligne, DateTime heureDebutSimulation, int arretInitialIndex = 0, bool sensAller = true)
        {
            Immatriculation = immatriculation;
            Ligne = ligne;
            ArretActuelIndex = arretInitialIndex;
            SensAller = sensAller;
            HeureDebutSimulation = heureDebutSimulation; // NOUVEAU
            
            // MODIFICATION : Le bus commence en attente si l'heure n'est pas encore arrivée
            if (PeutDemarrer)
            {
                Statut = StatutBus.AArret;
                // Départ initial dans 1-5 minutes
                var random = new Random();
                ProchainChangementStatut = Horloge.Instance.TempsActuel.AddMinutes(random.Next(1, 6));
            }
            else
            {
                Statut = StatutBus.AArret;
                // Attendre jusqu'à l'heure de début + un délai aléatoire
                var random = new Random();
                ProchainChangementStatut = HeureDebutSimulation.AddMinutes(random.Next(1, 6));
                
                Console.WriteLine($"[BUS] {Immatriculation} attend jusqu'à {ProchainChangementStatut:HH:mm} pour démarrer");
            }
            
            DemarrerTimer();
        }

        private void DemarrerTimer()
        {
            _timer = new System.Timers.Timer(5000); // Vérification toutes les 5 secondes
            _timer.Elapsed += VerifierStatut;
            _timer.Start();
        }

        private void VerifierStatut(object? sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (_disposed) return;

                var heureActuelle = Horloge.Instance.TempsActuel;
                
                // NOUVEAU : Vérifier si le bus peut démarrer
                if (!PeutDemarrer)
                {
                    // Le bus n'a pas encore le droit de démarrer
                    return;
                }
                
                if (heureActuelle >= ProchainChangementStatut)
                {
                    ChangerStatut();
                }
            }
        }

        private void ChangerStatut()
        {
            // NOUVEAU : Double vérification avant de changer de statut
            if (!PeutDemarrer)
            {
                Console.WriteLine($"[BUS] {Immatriculation} tente de changer de statut avant son heure de début");
                return;
            }

            var random = new Random();
            var heureActuelle = Horloge.Instance.TempsActuel;

            if (Statut == StatutBus.AArret)
            {
                // Le bus part de l'arrêt
                Statut = StatutBus.EnCirculation;
                
                // Calculer l'arrêt suivant
                CalculerArretSuivant();
                
                // Temps de trajet : 2-6 minutes
                var dureeTrajet = random.Next(2, 7);
                ProchainChangementStatut = heureActuelle.AddMinutes(dureeTrajet);
                
                // Notifier le départ
                NotifierDepartArret();
            }
            else
            {
                // Le bus arrive à un arrêt
                Statut = StatutBus.AArret;
                
                // Temps d'arrêt : 1-4 minutes
                var dureeArret = random.Next(1, 5);
                ProchainChangementStatut = heureActuelle.AddMinutes(dureeArret);
                
                // Notifier l'arrivée
                NotifierArriveeArret();
            }
            
            // Notifier le changement de statut général
            NotifierChangementStatut();
        }

        private void CalculerArretSuivant()
        {
            if (SensAller)
            {
                if (ArretActuelIndex < Ligne.ListArret.Count - 1)
                {
                    ArretActuelIndex++;
                }
                else
                {
                    // Terminus atteint - changer de sens
                    SensAller = false;
                    ArretActuelIndex--;
                }
            }
            else
            {
                if (ArretActuelIndex > 0)
                {
                    ArretActuelIndex--;
                }
                else
                {
                    // Terminus atteint - changer de sens
                    SensAller = true;
                    ArretActuelIndex++;
                }
            }
        }

        private void NotifierArriveeArret()
        {
            var eventArgs = CreerBusEventArgs("Arrivée");
            ArriveeArret?.Invoke(this, eventArgs);
            
            // Log pour debug
            Console.WriteLine($"[{Horloge.Instance.TempsActuel:HH:mm}] {Immatriculation} arrive à {ArretActuel.Nom}");
        }

        private void NotifierDepartArret()
        {
            var eventArgs = CreerBusEventArgs("Départ");
            DepartArret?.Invoke(this, eventArgs);
            
            // Log pour debug
            Console.WriteLine($"[{Horloge.Instance.TempsActuel:HH:mm}] {Immatriculation} part vers {ArretSuivant?.Nom ?? "Terminus"}");
        }

        private void NotifierChangementStatut()
        {
            var eventArgs = CreerBusEventArgs("Changement statut");
            StatutChange?.Invoke(this, eventArgs);
        }

        private BusEventArgs CreerBusEventArgs(string typeEvenement)
        {
            return new BusEventArgs
            {
                Bus = this,
                TypeEvenement = typeEvenement,
                Heure = Horloge.Instance.TempsActuel,
                ArretActuel = ArretActuel,
                ArretSuivant = ArretSuivant,
                Destination = Destination,
                SensNom = SensAller ? "aller" : "retour",
                TempsRestant = TempsRestantMinutes
            };
        }

        // Propriétés calculées
        public Arret ArretActuel => Ligne.ListArret[ArretActuelIndex];

        public Arret? ArretSuivant
        {
            get
            {
                if (Statut == StatutBus.AArret)
                {
                    // Calculer où il va aller
                    if (SensAller)
                    {
                        return ArretActuelIndex < Ligne.ListArret.Count - 1 
                            ? Ligne.ListArret[ArretActuelIndex + 1] 
                            : null;
                    }
                    else
                    {
                        return ArretActuelIndex > 0 
                            ? Ligne.ListArret[ArretActuelIndex - 1] 
                            : null;
                    }
                }
                else
                {
                    // Il est en circulation vers cet arrêt
                    return ArretActuel;
                }
            }
        }

        public string Destination => SensAller 
            ? Ligne.ListArret.Last().Nom 
            : Ligne.ListArret.First().Nom;

        /// <summary>
        /// Formate l'information selon le format demandé
        /// </summary>
        public string FormaterInfo(int numeroInfo)
        {
            var heure = Horloge.Instance.TempsActuel.ToString("HH:mm");
            var heureActuelle = Horloge.Instance.TempsActuel;
            
            // NOUVEAU : Affichage spécial si le bus n'a pas encore démarré
            if (!PeutDemarrer)
            {
                var minutesAvantDemarrage = (int)(HeureDebutSimulation - heureActuelle).TotalMinutes;
                return $"{heure} - Info {numeroInfo} : Sur la ligne : {Ligne.Nom}\n" +
                       $"   Le bus immatriculé : {Immatriculation}\n" +
                       $"   En attente de démarrage\n" +
                       $"   Démarrage prévu dans : {minutesAvantDemarrage} min\n" +
                       $"   Lieu de démarrage : {ArretActuel.Nom}\n" +
                       $"   Direction : {Destination}";
            }
            
            var statutTexte = Statut == StatutBus.AArret ? "À l'arrêt" : "En circulation";
            var sensNom = SensAller ? "aller" : "retour";
            
            if (Statut == StatutBus.AArret)
            {
                return $"{heure} - Info {numeroInfo} : Sur la ligne : {Ligne.Nom}\n" +
                       $"   Le bus immatriculé : {Immatriculation}\n" +
                       $"   À l'arrêt\n" +
                       $"   Actuellement à : {ArretActuel.Nom}\n" +
                       $"   Arrêt Suivant : {ArretSuivant?.Nom ?? "Terminus"}\n" +
                       $"   Sens circulation : {sensNom} (direction {Destination})\n" +
                       $"   Départ prévu dans : {TempsRestantMinutes} min";
            }
            else
            {
                return $"{heure} - Info {numeroInfo} : Sur la ligne : {Ligne.Nom}\n" +
                       $"   Le bus immatriculé : {Immatriculation}\n" +
                       $"   En circulation\n" +
                       $"   Vers : {ArretSuivant?.Nom ?? "Terminus"}\n" +
                       $"   Sens circulation : {sensNom} (direction {Destination})\n" +
                       $"   Arrivée prévue dans : {TempsRestantMinutes} min";
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed) return;
                
                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
                _disposed = true;
            }
        }
    }

    /// <summary>
    /// Arguments d'événement pour les changements de statut du bus
    /// </summary>
    public class BusEventArgs : EventArgs
    {
        public Bus Bus { get; set; } = null!;
        public string TypeEvenement { get; set; } = string.Empty;
        public DateTime Heure { get; set; }
        public Arret ArretActuel { get; set; } = null!;
        public Arret? ArretSuivant { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string SensNom { get; set; } = string.Empty;
        public int TempsRestant { get; set; }
    }
}