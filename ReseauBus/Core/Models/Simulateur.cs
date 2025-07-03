using ReseauBus.Core.Interfaces;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Simulateur principal (Singleton) avec gestion temps réel - Conforme au diagramme UML
    /// </summary>
    public class Simulateur
    {
        private static Simulateur? _instance;
        private static readonly object _lock = new object();

        public List<Simulation> Simulations { get; private set; }
        public Horloge Horloge { get; private set; }
        private List<IObserver> _observateurs;
        private System.Threading.Timer? _timerMiseAJour;

        private Simulateur()
        {
            Simulations = new List<Simulation>();
            Horloge = new Horloge();
            _observateurs = new List<IObserver>();
            
            // S'abonner aux changements d'heure pour notifier les observateurs
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
            }
            
            // Configurer l'horloge avec l'heure de début de la simulation
            if (!Horloge.EnMarche)
            {
                Horloge.DefinirHeureDebut(simulation.HeureDebut);
                Horloge.Start();
                DemarrerMiseAJourPeriodique();
            }
            
            // Exécuter la simulation
            simulation.Executer();
            
            // Notifier tous les observateurs
            NotifierObservateurs();
        }

        /// <summary>
        /// Arrête une simulation
        /// </summary>
        public void ArreterSimulation(Simulation simulation)
        {
            if (Simulations.Contains(simulation))
            {
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
        }

        /// <summary>
        /// Démarre la mise à jour périodique
        /// </summary>
        private void DemarrerMiseAJourPeriodique()
        {
            // Timer pour mettre à jour les observateurs toutes les 2 secondes (au lieu de 500ms)
            _timerMiseAJour = new System.Threading.Timer(
                callback: _ => NotifierObservateurs(),
                state: null,
                dueTime: TimeSpan.FromSeconds(2),
                period: TimeSpan.FromSeconds(2)
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
        /// Gestionnaire de changement d'heure
        /// </summary>
        private void Horloge_TempsChange(object? sender, DateTime nouvelleHeure)
        {
            // Vérifier si les simulations doivent s'arrêter
            var simulationsArreter = Simulations
                .Where(s => s.EnCours && nouvelleHeure >= s.HeureFin)
                .ToList();

            foreach (var simulation in simulationsArreter)
            {
                ArreterSimulation(simulation);
            }

            // Générer de nouveaux événements pour les simulations en cours
            foreach (var simulation in Simulations.Where(s => s.EnCours))
            {
                GenererNouveauxEvenements(simulation, nouvelleHeure);
            }
        }

        /// <summary>
        /// Génère de nouveaux événements pour une simulation
        /// </summary>
        private void GenererNouveauxEvenements(Simulation simulation, DateTime heure)
        {
            // Logique simple pour ajouter des événements en temps réel
            var random = new Random();
            
            foreach (var ligne in simulation.ListeLignes)
            {
                // Probabilité de générer un nouvel événement
                if (random.Next(100) < 20) // 20% de chance par minute
                {
                    var arretDepart = ligne.ListArret[random.Next(ligne.ListArret.Count - 1)];
                    var indexSuivant = ligne.ListArret.IndexOf(arretDepart) + 1;
                    var arretArrivee = ligne.ListArret[indexSuivant];
                    
                    var evenement = new Evenement(
                        heure.ToString("HH:mm"),
                        $"{ligne.Nom.Replace(" ", "")}-{random.Next(1, 10):D2}",
                        arretDepart,
                        arretArrivee,
                        $"00:0{random.Next(2, 6)}:00"
                    );
                    
                    ligne.AjouterEvenement(evenement);
                }
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
            var observateursACopie = _observateurs.ToList(); // Copie pour éviter les modifications concurrentes
            
            foreach (var observateur in observateursACopie)
            {
                try
                {
                    observateur.Actualiser();
                }
                catch (Exception ex)
                {
                    // Log l'erreur mais continue avec les autres observateurs
                    Console.WriteLine($"Erreur lors de la notification d'un observateur : {ex.Message}");
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
            Horloge.Stop();
            _observateurs.Clear();
        }
    }
}