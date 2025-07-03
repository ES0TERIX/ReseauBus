using ReseauBus.Data;
using ReseauBus.Core.Models;
using ReseauBus.UI.Forms;

namespace ReseauBus
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application de simulation de bus
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            // Test rapide des données en console
            Console.WriteLine("=== Simulateur de réseau de bus d'Amiens ===");
            Console.WriteLine("Chargement des données...");
            
            var arrets = DonneesAmiens.ObtenirArrets();
            var lignes = DonneesAmiens.ObtenirLignes();
            
            Console.WriteLine($"✓ {arrets.Count} arrêts chargés");
            Console.WriteLine($"✓ {lignes.Count} lignes chargées");
            Console.WriteLine("✓ Architecture UML implémentée");
            Console.WriteLine("\nLancement de l'interface de configuration...\n");
            
            // Lancer l'interface graphique
            try
            {
                Application.Run(new FormConfiguration());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du lancement de l'application :\n{ex.Message}", 
                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Fallback sur les tests console
                Console.WriteLine("Fallback sur les tests console...");
                TestSimulateurUML();
            }
        }
        
        private static void TestSimulateurUML()
        {
            Console.WriteLine("=== Test du Simulateur (Pattern UML) ===");
            
            // Test du pattern Singleton
            var simulateur1 = Simulateur.Instance;
            var simulateur2 = Simulateur.GetInstance();
            
            Console.WriteLine($"Test Singleton : {simulateur1 == simulateur2}");
            Console.WriteLine($"Horloge présente : {simulateur1.Horloge != null}");
            
            // Créer des observateurs
            var observateur1 = new ObservateurTest("Interface Admin");
            var observateur2 = new ObservateurTest("Interface Utilisateur");
            
            simulateur1.AjouterObservateur(observateur1);
            simulateur1.AjouterObservateur(observateur2);
            
            // Créer une simulation
            var simulation = new Simulation("Amiens Semaine Test");
            var lignes = DonneesAmiens.ObtenirLignes();
            
            // Ajouter quelques lignes à la simulation
            foreach (var ligne in lignes.Take(2))
            {
                simulation.AjouterLigne(ligne);
            }
            
            Console.WriteLine($"Simulation créée avec {simulation.ListeLignes.Count} lignes");
            
            // Lancer la simulation via le simulateur
            Console.WriteLine("Lancement de la simulation...");
            simulateur1.LancerSimulation(simulation);
            
            // Afficher les événements générés
            Console.WriteLine("\n=== Événements générés ===");
            foreach (var ligne in simulation.ListeLignes)
            {
                Console.WriteLine($"\nLigne {ligne.Nom} - {ligne.ListeEvenements.Count} événements :");
                foreach (var evenement in ligne.ListeEvenements.Take(3))
                {
                    Console.WriteLine($"  {evenement}");
                }
                
                // Test du filtrage par heure
                var evenementsMatin = ligne.FiltrerParHeure("08:00", "12:00");
                Console.WriteLine($"  Événements entre 8h et 12h : {evenementsMatin.Count}");
            }
            
            // Test de l'horloge
            Console.WriteLine($"\nHorloge en marche : {simulateur1.Horloge.EnMarche}");
            Console.WriteLine($"Temps actuel : {simulateur1.Horloge.TempsActuel:HH:mm:ss}");
            
            // Arrêter la simulation
            simulateur1.ArreterSimulation(simulation);
            Console.WriteLine($"Simulation arrêtée. Horloge en marche : {simulateur1.Horloge.EnMarche}");
            
            // Test des InfoBus
            Console.WriteLine("\n=== Test InfoBus ===");
            var infoBus = new InfoBus();
            infoBus.Heure = DateTime.Now.ToString("HH:mm");
            infoBus.Immatriculation = "Nemo1-01";
            infoBus.LieuDepart = "Etouvie";
            infoBus.LieuArrivee = "Martinique";
            infoBus.Sens = 1;
            infoBus.Duree = "00:03:30";
            
            Console.WriteLine(infoBus.FormaterPourAffichage());
            
            Console.WriteLine("\nTest terminé. Interface graphique disponible au prochain lancement.");
        }
    }
}