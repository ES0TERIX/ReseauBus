namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Simulation avec logique de bus réaliste - Conforme au diagramme UML
    /// </summary>
    public class Simulation
    {
        public string Nom { get; set; }
        public List<LigneBus> ListeLignes { get; set; }
        public DateTime HeureDebut { get; set; }
        public DateTime HeureFin { get; set; }
        public bool EnCours { get; private set; }
        
        private List<BusVirtuel> _busVirtuels;
        private Random _random;
        private int _prochainIdBus;

        public Simulation(string nom)
        {
            Nom = nom;
            ListeLignes = new List<LigneBus>();
            _busVirtuels = new List<BusVirtuel>();
            _random = new Random();
            _prochainIdBus = 1;
            HeureDebut = DateTime.Today.AddHours(6);
            HeureFin = DateTime.Today.AddHours(23);
        }

        /// <summary>
        /// Ajoute une ligne à la simulation
        /// </summary>
        public void AjouterLigne(LigneBus ligne)
        {
            ListeLignes.Add(ligne);
        }

        /// <summary>
        /// Exécute la simulation avec logique réaliste
        /// </summary>
        public void Executer()
        {
            EnCours = true;
            
            // Créer des bus virtuels pour chaque ligne
            CreerBusVirtuels();
            
            // Générer les événements basés sur la simulation
            GenererEvenementsRealistes();
        }

        /// <summary>
        /// Arrête la simulation
        /// </summary>
        public void Arreter()
        {
            EnCours = false;
        }

        /// <summary>
        /// Crée des bus virtuels pour chaque ligne
        /// </summary>
        private void CreerBusVirtuels()
        {
            _busVirtuels.Clear();
            
            foreach (var ligne in ListeLignes)
            {
                // Créer 2-4 bus par ligne
                int nombreBus = _random.Next(2, 5);
                
                for (int i = 0; i < nombreBus; i++)
                {
                    var bus = new BusVirtuel
                    {
                        Id = _prochainIdBus++,
                        Immatriculation = $"{ligne.Nom.Replace(" ", "")}-{i + 1:D2}",
                        Ligne = ligne,
                        ArretActuelIndex = 0,
                        SensAller = true,
                        ProchainDepartPrevu = HeureDebut.AddMinutes(i * 15 + _random.Next(0, 10))
                    };
                    
                    _busVirtuels.Add(bus);
                }
            }
        }

        /// <summary>
        /// Génère des événements réalistes basés sur la simulation de bus
        /// </summary>
        private void GenererEvenementsRealistes()
        {
            var simulateur = Simulateur.Instance;
            var heureActuelle = HeureDebut;
            
            while (heureActuelle < HeureFin)
            {
                foreach (var bus in _busVirtuels)
                {
                    if (heureActuelle >= bus.ProchainDepartPrevu)
                    {
                        GenererEvenementBus(bus, heureActuelle);
                    }
                }
                
                heureActuelle = heureActuelle.AddMinutes(1);
            }
        }

        /// <summary>
        /// Génère un événement pour un bus spécifique
        /// </summary>
        private void GenererEvenementBus(BusVirtuel bus, DateTime heure)
        {
            var arretActuel = bus.Ligne.ListArret[bus.ArretActuelIndex];
            Arret arretSuivant;
            
            // Déterminer l'arrêt suivant
            if (bus.SensAller)
            {
                if (bus.ArretActuelIndex < bus.Ligne.ListArret.Count - 1)
                {
                    arretSuivant = bus.Ligne.ListArret[bus.ArretActuelIndex + 1];
                    bus.ArretActuelIndex++;
                }
                else
                {
                    // Terminus - changer de sens
                    bus.SensAller = false;
                    arretSuivant = bus.Ligne.ListArret[bus.ArretActuelIndex - 1];
                    bus.ArretActuelIndex--;
                }
            }
            else
            {
                if (bus.ArretActuelIndex > 0)
                {
                    arretSuivant = bus.Ligne.ListArret[bus.ArretActuelIndex - 1];
                    bus.ArretActuelIndex--;
                }
                else
                {
                    // Terminus - changer de sens
                    bus.SensAller = true;
                    arretSuivant = bus.Ligne.ListArret[bus.ArretActuelIndex + 1];
                    bus.ArretActuelIndex++;
                }
            }
            
            // Calculer durée du trajet
            var distance = arretActuel.DistanceVers(arretSuivant);
            var dureeMinutes = Math.Max(2, distance * 0.5 + _random.NextDouble() * 2);
            var dureeFormatee = TimeSpan.FromMinutes(dureeMinutes).ToString(@"hh\:mm\:ss");
            
            // Créer l'événement
            var evenement = new Evenement(
                heure.ToString("HH:mm"),
                bus.Immatriculation,
                arretActuel,
                arretSuivant,
                dureeFormatee
            );
            
            bus.Ligne.AjouterEvenement(evenement);
            
            // Programmer le prochain départ
            bus.ProchainDepartPrevu = heure.AddMinutes(dureeMinutes + _random.Next(1, 3));
        }
    }

    /// <summary>
    /// Représentation virtuelle d'un bus pour la simulation
    /// </summary>
    internal class BusVirtuel
    {
        public int Id { get; set; }
        public string Immatriculation { get; set; } = string.Empty;
        public LigneBus Ligne { get; set; } = null!;
        public int ArretActuelIndex { get; set; }
        public bool SensAller { get; set; }
        public DateTime ProchainDepartPrevu { get; set; }
    }
}