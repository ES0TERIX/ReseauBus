using System.Timers;

namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Horloge de simulation - Conforme au diagramme UML
    /// </summary>
    public class Horloge
    {
        public DateTime TempsActuel { get; private set; }
        public bool EnMarche { get; private set; }
        
        private System.Timers.Timer? _timer;
        private readonly object _lock = new object();

        public event EventHandler<DateTime>? TempsChange;

        public Horloge()
        {
            TempsActuel = DateTime.Now;
            EnMarche = false;
        }

        /// <summary>
        /// Démarre l'horloge
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                if (EnMarche) return;

                EnMarche = true;
                _timer = new System.Timers.Timer(1000); // 1 seconde = 1 minute de simulation
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
            }
        }

        /// <summary>
        /// Arrête l'horloge
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (!EnMarche) return;

                _timer?.Stop();
                _timer?.Dispose();
                _timer = null;
                EnMarche = false;
            }
        }

        /// <summary>
        /// Avance le temps de simulation
        /// </summary>
        public void Avancer()
        {
            TempsActuel = TempsActuel.AddMinutes(1);
            TempsChange?.Invoke(this, TempsActuel);
        }

        /// <summary>
        /// Gestionnaire du timer
        /// </summary>
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Avancer();
        }

        /// <summary>
        /// Définit l'heure de départ de la simulation
        /// </summary>
        public void DefinirHeureDebut(DateTime heureDebut)
        {
            TempsActuel = heureDebut;
        }
    }
}