namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Simulation avec bus autonomes qui se gèrent eux-mêmes
    /// </summary>
    public class Simulation : IDisposable
    {
        public string Nom { get; set; }
        public List<LigneBus> ListeLignes { get; set; }
        public DateTime HeureDebut { get; set; }
        public DateTime HeureFin { get; set; }
        public bool EnCours { get; private set; }
        
        // Liste des bus autonomes
        public List<Bus> ListeBus { get; private set; }
        
        // Événements pour notifier l'interface
        public event EventHandler<BusEventArgs>? BusArrive;
        public event EventHandler<BusEventArgs>? BusPart;
        public event EventHandler<BusEventArgs>? BusChangeStatut;

        private Random _random;
        private int _prochainIdBus;
        private bool _disposed = false;

        public Simulation(string nom)
        {
            Nom = nom;
            ListeLignes = new List<LigneBus>();
            ListeBus = new List<Bus>();
            _random = new Random();
            _prochainIdBus = 1;
            HeureDebut = DateTime.Today.AddHours(6);
            HeureFin = DateTime.Today.AddHours(23);
        }

        public void AjouterLigne(LigneBus ligne)
        {
            ListeLignes.Add(ligne);
        }

        public void Executer()
        {
            if (EnCours) return;
            
            EnCours = true;
            
            // Configurer l'horloge unique SEULEMENT si elle n'est pas déjà en marche
            if (!Horloge.Instance.EnMarche)
            {
                Horloge.Instance.DefinirHeureDebut(HeureDebut);
            }
            
            // Créer les bus autonomes
            CreerBusAutonomes();
            
            // Démarrer l'horloge si elle n'est pas déjà en marche
            if (!Horloge.Instance.EnMarche)
            {
                Horloge.Instance.Start();
            }
            
            Console.WriteLine($"[SIMULATION] {Nom} démarrée avec {ListeBus.Count} bus");
        }

        public void Arreter()
        {
            if (!EnCours) return;
            
            EnCours = false;
            
            // Disposer tous les bus
            foreach (var bus in ListeBus)
            {
                DesabonnerEvenementsBus(bus);
                bus.Dispose();
            }
            ListeBus.Clear();
            
            Console.WriteLine($"[SIMULATION] {Nom} arrêtée");
        }

        private void CreerBusAutonomes()
        {
            ListeBus.Clear();
    
            foreach (var ligne in ListeLignes)
            {
                // MODIFICATION : Créer 1 seul bus par ligne
                int nombreBus = 1;
        
                for (int i = 0; i < nombreBus; i++)
                {
                    // Choisir un arrêt de départ aléatoire
                    int arretDepart = _random.Next(0, ligne.ListArret.Count);
            
                    // Choisir un sens aléatoire
                    bool sensAller = _random.NextDouble() > 0.5;
            
                    var bus = new Bus(
                        immatriculation: $"{ligne.Nom.Replace(" ", "")}-{i + 1:D2}",
                        ligne: ligne,
                        arretInitialIndex: arretDepart,
                        sensAller: sensAller
                    );
            
                    // S'abonner aux événements du bus
                    AbonnerEvenementsBus(bus);
            
                    ListeBus.Add(bus);
                }
            }
        }

        private void AbonnerEvenementsBus(Bus bus)
        {
            bus.ArriveeArret += OnBusArrive;
            bus.DepartArret += OnBusPart;
            bus.StatutChange += OnBusChangeStatut;
        }

        private void DesabonnerEvenementsBus(Bus bus)
        {
            bus.ArriveeArret -= OnBusArrive;
            bus.DepartArret -= OnBusPart;
            bus.StatutChange -= OnBusChangeStatut;
        }

        private void OnBusArrive(object? sender, BusEventArgs e)
        {
            BusArrive?.Invoke(this, e);
        }

        private void OnBusPart(object? sender, BusEventArgs e)
        {
            BusPart?.Invoke(this, e);
        }

        private void OnBusChangeStatut(object? sender, BusEventArgs e)
        {
            BusChangeStatut?.Invoke(this, e);
        }

        /// <summary>
        /// Retourne tous les bus actifs pour une ligne donnée
        /// </summary>
        public List<Bus> ObtenirBusParLigne(string nomLigne)
        {
            return ListeBus.Where(b => b.Ligne.Nom == nomLigne).ToList();
        }

        /// <summary>
        /// Retourne tous les événements récents formatés pour l'affichage
        /// </summary>
        public List<string> ObtenirEvenementsRecents(int limiteMinutes = 10)
        {
            var evenements = new List<string>();
            var heureActuelle = Horloge.Instance.TempsActuel;
            int numeroInfo = 1;

            foreach (var bus in ListeBus.OrderBy(b => b.Ligne.Nom).ThenBy(b => b.Immatriculation))
            {
                // Inclure tous les bus actifs (pas de limite de temps pour le moment)
                evenements.Add(bus.FormaterInfo(numeroInfo++));
            }

            return evenements;
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            Arreter();
            _disposed = true;
        }
    }
}