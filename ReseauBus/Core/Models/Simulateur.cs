using ReseauBus.Core.Interfaces;

namespace ReseauBus.Core.Models
{
    public class Simulateur
    {
        private static Simulateur? _instance;
        private static readonly object _lock = new object();

        public List<Simulation> Simulations { get; private set; }
        public Horloge Horloge => Horloge.Instance;
        
        private List<IObserver> _observateurs;
        private System.Threading.Timer? _timerMiseAJour;
        private bool _horlogeDemarree = false;
        private DateTime? _heureDebutGlobale = null;

        private Simulateur()
        {
            Simulations = new List<Simulation>();
            _observateurs = new List<IObserver>();
            
            Horloge.TempsChange += Horloge_TempsChange;
        }

        public static Simulateur Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new Simulateur();
                }
            }
        }

        public static Simulateur GetInstance()
        {
            return Instance;
        }

        public void LancerSimulation(Simulation simulation)
        {
            if (!Simulations.Contains(simulation))
            {
                Simulations.Add(simulation);
                
                simulation.BusArrive += OnSimulationEvent;
                simulation.BusPart += OnSimulationEvent;
                simulation.BusChangeStatut += OnSimulationEvent;
            }
            
            if (_heureDebutGlobale == null)
            {
                _heureDebutGlobale = simulation.HeureDebut;
                Console.WriteLine($"[SIMULATEUR] Première simulation - Heure de début globale: {_heureDebutGlobale:HH:mm}");
            }
            else
            {
                if (simulation.HeureDebut < _heureDebutGlobale)
                {
                    _heureDebutGlobale = simulation.HeureDebut;
                    Console.WriteLine($"[SIMULATEUR] Nouvelle heure de début globale (plus tôt): {_heureDebutGlobale:HH:mm}");
                    
                    if (_horlogeDemarree && Horloge.EnMarche)
                    {
                        Console.WriteLine($"[SIMULATEUR] Redéfinition de l'heure de l'horloge active");
                        Horloge.Stop();
                        Horloge.DefinirHeureDebut(_heureDebutGlobale.Value);
                        Horloge.Start();
                    }
                }
            }
            
            if (!_horlogeDemarree)
            {
                if (!Horloge.EnMarche && _heureDebutGlobale.HasValue)
                {
                    Console.WriteLine($"[SIMULATEUR] Définition heure début globale: {_heureDebutGlobale:HH:mm}");
                    Horloge.DefinirHeureDebut(_heureDebutGlobale.Value);
                }
                
                if (!Horloge.EnMarche)
                {
                    Horloge.Start();
                }
                
                _horlogeDemarree = true;
            }
            
            simulation.Executer();
            
            if (_timerMiseAJour == null)
            {
                DemarrerMiseAJourPeriodique();
            }
            
            NotifierObservateurs();
            
            Console.WriteLine($"[SIMULATEUR] Simulation '{simulation.Nom}' lancée (Début: {simulation.HeureDebut:HH:mm}, Fin: {simulation.HeureFin:HH:mm})");
            Console.WriteLine($"[SIMULATEUR] Heure actuelle de l'horloge: {Horloge.TempsActuel:HH:mm}");
        }

        public void ArreterSimulation(Simulation simulation)
        {
            if (Simulations.Contains(simulation))
            {
                simulation.BusArrive -= OnSimulationEvent;
                simulation.BusPart -= OnSimulationEvent;
                simulation.BusChangeStatut -= OnSimulationEvent;
                
                simulation.Arreter();
                Simulations.Remove(simulation);
            }
            
            if (Simulations.Count == 0)
            {
                Horloge.Stop();
                ArreterMiseAJourPeriodique();
                _horlogeDemarree = false;
                _heureDebutGlobale = null;
            }
            
            NotifierObservateurs();
            Console.WriteLine($"[SIMULATEUR] Simulation '{simulation.Nom}' arrêtée");
        }

        private void OnSimulationEvent(object? sender, BusEventArgs e)
        {
            if (e.TypeEvenement == "Arrivée")
            {
                Console.WriteLine($"[EVENT] {e.Bus.Immatriculation} arrive à {e.ArretActuel.Nom}");
            }
            else if (e.TypeEvenement == "Départ")
            {
                Console.WriteLine($"[EVENT] {e.Bus.Immatriculation} part vers {e.ArretSuivant?.Nom}");
            }
            
            NotifierObservateurs();
        }

        private void DemarrerMiseAJourPeriodique()
        {
            _timerMiseAJour = new System.Threading.Timer(
                callback: _ => NotifierObservateurs(),
                state: null,
                dueTime: TimeSpan.FromSeconds(3),
                period: TimeSpan.FromSeconds(3)
            );
        }

        private void ArreterMiseAJourPeriodique()
        {
            _timerMiseAJour?.Dispose();
            _timerMiseAJour = null;
        }

        private void Horloge_TempsChange(object? sender, DateTime nouvelleHeure)
        {
            var simulationsArreter = Simulations
                .Where(s => s.EnCours && nouvelleHeure >= s.HeureFin)
                .ToList();

            foreach (var simulation in simulationsArreter)
            {
                Console.WriteLine($"[SIMULATEUR] Fin de simulation '{simulation.Nom}' à {nouvelleHeure:HH:mm}");
                ArreterSimulation(simulation);
            }
        }

        public void AjouterObservateur(IObserver observateur)
        {
            if (!_observateurs.Contains(observateur))
            {
                _observateurs.Add(observateur);
            }
        }

        public void SupprimerObservateur(IObserver observateur)
        {
            _observateurs.Remove(observateur);
        }

        private void NotifierObservateurs()
        {
            var observateursACopie = _observateurs.ToList();
            
            foreach (var observateur in observateursACopie)
            {
                try
                {
                    observateur.Actualiser();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERREUR] Notification observateur : {ex.Message}");
                }
            }
        }

        public List<Simulation> ObtenirSimulations()
        {
            return new List<Simulation>(Simulations);
        }

        public DateTime? ObtenirHeureDebutGlobale()
        {
            return _heureDebutGlobale;
        }

        public void Dispose()
        {
            ArreterMiseAJourPeriodique();
            
            var simulations = Simulations.ToList();
            foreach (var simulation in simulations)
            {
                ArreterSimulation(simulation);
            }
            
            Horloge.Dispose();
            _observateurs.Clear();
            _horlogeDemarree = false;
            _heureDebutGlobale = null;
        }
    }
}