using System.Timers;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Horloge unique pour toute l'application - Singleton thread-safe
    /// </summary>
    public sealed class Horloge
    {
        private static readonly object _lock = new object();
        private static Horloge? _instance;
        
        private System.Timers.Timer? _timer;
        private readonly object _timerLock = new object();

        public DateTime TempsActuel { get; private set; }
        public bool EnMarche { get; private set; }
        
        // Événement pour notifier les changements de temps
        public event EventHandler<DateTime>? TempsChange;

        private Horloge()
        {
            TempsActuel = DateTime.Today.AddHours(6); // Commence à 6h00
            EnMarche = false;
        }

        /// <summary>
        /// Instance unique de l'horloge
        /// </summary>
        public static Horloge Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new Horloge();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Démarre l'horloge de simulation
        /// </summary>
        public void Start()
        {
            lock (_timerLock)
            {
                if (EnMarche) return;

                EnMarche = true;
                _timer = new System.Timers.Timer(1000); // 1 seconde = 1 minute de simulation
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
                
                Console.WriteLine($"[HORLOGE] Démarrage à {TempsActuel:HH:mm}");
            }
        }

        /// <summary>
        /// Arrête l'horloge de simulation
        /// </summary>
        public void Stop()
        {
            lock (_timerLock)
            {
                if (!EnMarche) return;

                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
                EnMarche = false;
                
                Console.WriteLine($"[HORLOGE] Arrêt à {TempsActuel:HH:mm}");
            }
        }

        /// <summary>
        /// Définit l'heure de départ de la simulation
        /// </summary>
        public void DefinirHeureDebut(DateTime heureDebut)
        {
            lock (_timerLock)
            {
                if (EnMarche)
                {
                    throw new InvalidOperationException("Impossible de changer l'heure pendant que l'horloge fonctionne");
                }
                
                TempsActuel = heureDebut;
                Console.WriteLine($"[HORLOGE] Heure définie à {TempsActuel:HH:mm}");
            }
        }

        /// <summary>
        /// Avance le temps de simulation d'une minute
        /// </summary>
        private void Avancer()
        {
            var ancienneHeure = TempsActuel;
            TempsActuel = TempsActuel.AddMinutes(1);
            
            // Notifier le changement
            TempsChange?.Invoke(this, TempsActuel);
            
            // Log toutes les 10 minutes pour éviter le spam
            if (TempsActuel.Minute % 10 == 0)
            {
                Console.WriteLine($"[HORLOGE] {TempsActuel:HH:mm}");
            }
        }

        /// <summary>
        /// Gestionnaire du timer
        /// </summary>
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Avancer();
        }

        /// <summary>
        /// Remet à zéro l'horloge (pour les tests)
        /// </summary>
        public void Reset()
        {
            lock (_timerLock)
            {
                if (EnMarche)
                {
                    Stop();
                }
                
                TempsActuel = DateTime.Today.AddHours(6);
                Console.WriteLine($"[HORLOGE] Reset à {TempsActuel:HH:mm}");
            }
        }

        /// <summary>
        /// Dispose l'instance (pour les tests principalement)
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }
}