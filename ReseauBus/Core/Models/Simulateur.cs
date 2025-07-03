using ReseauBus.Core.Interfaces;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Simulateur simplifié - Les bus sont maintenant autonomes
    /// </summary>
    public class Simulateur
    {
        private static Simulateur? _instance;
        private static readonly object _lock = new object();

        public List<Simulation> Simulations { get; private set; }
        public Horloge Horloge => Horloge.Instance;
        
        private List<IObserver> _observateurs;
        private System.Threading.Timer? _timerMiseAJour;

        private Simulateur()
        {
            Simulations = new List<Simulation>();
            _observateurs = new List<IObserver>();
            
            // S'abonner aux changements d'heure pour vérifier la fin des simulations
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

        /// <summary>
        /// Obtient l'instance du simulateur
        /// </summary>
        public static Simulateur GetInstance()
        {
            return Instance;
        }

        /// <summary>
        /// Lance une simulation
        /// </summary>
        public void LancerSimulation(Simulation simulation)
        {
            if (!Simulations.Contains(simulation))
            {
                Simulations.Add(simulation);
                
                // S'abonner aux événements de la simulation
                simulation.BusArrive += OnSimulationEvent;
                simulation.BusPart += OnSimulationEvent;
                simulation.BusChangeStatut += OnSimulationEvent;
            }
            
            // Exécuter la simulation (les bus vont se gérer eux-mêmes)
            simulation.Executer();
            
            // Démarrer la mise à jour périodique des observateurs
            if (_timerMiseAJour == null)
            {
                DemarrerMiseAJourPeriodique();
            }
            
            // Notifier les observateurs
            NotifierObservateurs();
            
            Console.WriteLine($"[SIMULATEUR] Simulation '{simulation.Nom}' lancée");
        }

        /// <summary>
        /// Arrête une simulation
        /// </summary>
        public void ArreterSimulation(Simulation simulation)
        {
            if (Simulations.Contains(simulation))
            {
                // Se désabonner des événements
                simulation.BusArrive -= OnSimulationEvent;
                simulation.BusPart -= OnSimulationEvent;
                simulation.BusChangeStatut -= OnSimulationEvent;
                
                simulation.Arreter();
                Simulations.Remove(simulation);
            }
            
            // Arrêter l'horloge si plus de simulations
            if (Simulations.Count == 0)
            {
                Horloge.Stop();
                ArreterMiseAJourPeriodique();
            }
            
            NotifierObservateurs();
            Console.WriteLine($"[SIMULATEUR] Simulation '{simulation.Nom}' arrêtée");
        }

        /// <summary>
        /// Gestionnaire des événements de simulation (arrivée/départ de bus)
        /// </summary>
        private void OnSimulationEvent(object? sender, BusEventArgs e)
        {
            // Les bus notifient déjà leurs changements, 
            // ici on peut faire du logging ou des traitements spécifiques
            if (e.TypeEvenement == "Arrivée")
            {
                Console.WriteLine($"[EVENT] {e.Bus.Immatriculation} arrive à {e.ArretActuel.Nom}");
            }
            else if (e.TypeEvenement == "Départ")
            {
                Console.WriteLine($"[EVENT] {e.Bus.Immatriculation} part vers {e.ArretSuivant?.Nom}");
            }
            
            // Notifier les observateurs de l'interface
            NotifierObservateurs();
        }

        /// <summary>
        /// Démarre la mise à jour périodique des observateurs
        /// </summary>
        private void DemarrerMiseAJourPeriodique()
        {
            // Timer pour mettre à jour les observateurs toutes les 3 secondes
            _timerMiseAJour = new System.Threading.Timer(
                callback: _ => NotifierObservateurs(),
                state: null,
                dueTime: TimeSpan.FromSeconds(3),
                period: TimeSpan.FromSeconds(3)
            );
        }

        /// <summary>
        /// Arrête la mise à jour périodique
        /// </summary>
        private void ArreterMiseAJourPeriodique()
        {
            _timerMiseAJour?.Dispose();
            _timerMiseAJour = null;
        }

        /// <summary>
        /// Gestionnaire de changement d'heure - Vérifie la fin des simulations
        /// </summary>
        private void Horloge_TempsChange(object? sender, DateTime nouvelleHeure)
        {
            // Vérifier si des simulations doivent s'arrêter
            var simulationsArreter = Simulations
                .Where(s => s.EnCours && nouvelleHeure >= s.HeureFin)
                .ToList();

            foreach (var simulation in simulationsArreter)
            {
                Console.WriteLine($"[SIMULATEUR] Fin de simulation '{simulation.Nom}' à {nouvelleHeure:HH:mm}");
                ArreterSimulation(simulation);
            }
        }

        /// <summary>
        /// Ajoute un observateur
        /// </summary>
        public void AjouterObservateur(IObserver observateur)
        {
            if (!_observateurs.Contains(observateur))
            {
                _observateurs.Add(observateur);
            }
        }

        /// <summary>
        /// Supprime un observateur
        /// </summary>
        public void SupprimerObservateur(IObserver observateur)
        {
            _observateurs.Remove(observateur);
        }

        /// <summary>
        /// Notifie tous les observateurs
        /// </summary>
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

        /// <summary>
        /// Retourne toutes les simulations
        /// </summary>
        public List<Simulation> ObtenirSimulations()
        {
            return new List<Simulation>(Simulations);
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public void Dispose()
        {
            ArreterMiseAJourPeriodique();
            
            // Arrêter toutes les simulations
            var simulations = Simulations.ToList();
            foreach (var simulation in simulations)
            {
                ArreterSimulation(simulation);
            }
            
            Horloge.Dispose();
            _observateurs.Clear();
        }
    }
}