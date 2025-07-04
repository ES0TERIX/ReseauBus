using System.Timers;

namespace ReseauBus.Core.Models
{
    public sealed class Horloge
    {
        private static readonly object _lock = new object();
        private static Horloge? _instance;
        
        private System.Timers.Timer? _timer;
        private readonly object _timerLock = new object();

        public DateTime TempsActuel { get; private set; }
        public bool EnMarche { get; private set; }
        
        public event EventHandler<DateTime>? TempsChange;

        private Horloge()
        {
            TempsActuel = DateTime.Today.AddHours(6);
            EnMarche = false;
        }

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

        public void Start()
        {
            lock (_timerLock)
            {
                if (EnMarche) return;

                EnMarche = true;
                _timer = new System.Timers.Timer(1000);
                _timer.Elapsed += OnTimerElapsed;
                _timer.Start();
                
                Console.WriteLine($"[HORLOGE] Démarrage à {TempsActuel:HH:mm}");
            }
        }

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

        private void Avancer()
        {
            var ancienneHeure = TempsActuel;
            TempsActuel = TempsActuel.AddMinutes(1);
            
            TempsChange?.Invoke(this, TempsActuel);
            
            if (TempsActuel.Minute % 10 == 0)
            {
                Console.WriteLine($"[HORLOGE] {TempsActuel:HH:mm}");
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            Avancer();
        }

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

        public void Dispose()
        {
            Stop();
        }
    }
}