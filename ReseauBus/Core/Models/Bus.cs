using ReseauBus.Core.Interfaces;
using System.Timers;

namespace ReseauBus.Core.Models
{
    public class Bus : IDisposable
    {
        private System.Timers.Timer? _timer;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public string Immatriculation { get; private set; }
        public LigneBus Ligne { get; private set; }
        public int ArretActuelIndex { get; private set; }
        public bool SensAller { get; private set; }
        public StatutBus Statut { get; private set; }
        public DateTime ProchainChangementStatut { get; private set; }
        public DateTime HeureDebutSimulation { get; private set; }
        public bool PeutDemarrer => Horloge.Instance.TempsActuel >= HeureDebutSimulation;

        public int TempsRestantMinutes =>
            Math.Max(0, (int)(ProchainChangementStatut - Horloge.Instance.TempsActuel).TotalMinutes);

        public event EventHandler<BusEventArgs>? StatutChange;
        public event EventHandler<BusEventArgs>? ArriveeArret;
        public event EventHandler<BusEventArgs>? DepartArret;

        public Bus(string immatriculation, LigneBus ligne, DateTime heureDebutSimulation, int arretInitialIndex = 0,
            bool sensAller = true)
        {
            Immatriculation = immatriculation;
            Ligne = ligne;
            ArretActuelIndex = arretInitialIndex;
            SensAller = sensAller;
            HeureDebutSimulation = heureDebutSimulation;

            if (PeutDemarrer)
            {
                Statut = StatutBus.AArret;
                var random = new Random();
                ProchainChangementStatut = Horloge.Instance.TempsActuel.AddMinutes(random.Next(1, 6));
            }
            else
            {
                Statut = StatutBus.AArret;
                var random = new Random();
                ProchainChangementStatut = HeureDebutSimulation.AddMinutes(random.Next(1, 6));

                Console.WriteLine(
                    $"[BUS] {Immatriculation} attend jusqu'à {ProchainChangementStatut:HH:mm} pour démarrer");
            }

            DemarrerTimer();
        }

        private void DemarrerTimer()
        {
            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += VerifierStatut;
            _timer.Start();
        }

        private void VerifierStatut(object? sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                if (_disposed) return;

                var heureActuelle = Horloge.Instance.TempsActuel;

                if (!PeutDemarrer)
                {
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
            if (!PeutDemarrer)
            {
                Console.WriteLine($"[BUS] {Immatriculation} tente de changer de statut avant son heure de début");
                return;
            }

            var random = new Random();
            var heureActuelle = Horloge.Instance.TempsActuel;

            if (Statut == StatutBus.AArret)
            {
                Statut = StatutBus.EnCirculation;

                CalculerArretSuivant();

                var dureeTrajet = random.Next(2, 7);
                ProchainChangementStatut = heureActuelle.AddMinutes(dureeTrajet);

                NotifierDepartArret();
            }
            else
            {
                Statut = StatutBus.AArret;

                var dureeArret = random.Next(1, 5);
                ProchainChangementStatut = heureActuelle.AddMinutes(dureeArret);

                NotifierArriveeArret();
            }

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
                    SensAller = true;
                    ArretActuelIndex++;
                }
            }
        }

        private void NotifierArriveeArret()
        {
            var eventArgs = CreerBusEventArgs("Arrivée");
            ArriveeArret?.Invoke(this, eventArgs);

            Console.WriteLine($"[{Horloge.Instance.TempsActuel:HH:mm}] {Immatriculation} arrive à {ArretActuel.Nom}");
        }

        private void NotifierDepartArret()
        {
            var eventArgs = CreerBusEventArgs("Départ");
            DepartArret?.Invoke(this, eventArgs);

            Console.WriteLine(
                $"[{Horloge.Instance.TempsActuel:HH:mm}] {Immatriculation} part vers {ArretSuivant?.Nom ?? "Terminus"}");
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

        public Arret ArretActuel => Ligne.ListArret[ArretActuelIndex];

        public Arret? ArretSuivant
        {
            get
            {
                if (Statut == StatutBus.AArret)
                {
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
                    return ArretActuel;
                }
            }
        }

        public string Destination => SensAller
            ? Ligne.ListArret.Last().Nom
            : Ligne.ListArret.First().Nom;

        public string FormaterInfo(int numeroInfo)
        {
            var heure = Horloge.Instance.TempsActuel.ToString("HH:mm");
            var heureActuelle = Horloge.Instance.TempsActuel;

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