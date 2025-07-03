using ReseauBus.Core.Models;

namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Panneau d'en-tête optimisé sans clignotement
    /// </summary>
    public class PanneauEnTete : Panel
    {
        private Label _labelHeure;
        private Label _labelFinSimulation;
        private Label _labelNomSimulation;
        private ProgressBar _progressBar;
        private Simulation? _simulation;
        private Simulateur _simulateur;
        private string _derniereHeure = string.Empty;
        private int _dernierPourcentage = -1;
        private bool _miseAJourEnCours = false;

        public PanneauEnTete(Simulation? simulation)
        {
            _simulation = simulation;
            _simulateur = Simulateur.Instance;
            
            // Activer le double buffering
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer, true);
            
            InitialiserControles();
        }

        private void InitialiserControles()
        {
            this.BackColor = Color.LightBlue;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Dock = DockStyle.Fill;

            // Suspendre le layout
            this.SuspendLayout();

            // Heure actuelle (grande)
            _labelHeure = new Label
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(10, 10),
                Size = new Size(120, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Fin de simulation
            _labelFinSimulation = new Label
            {
                Text = "Fin de la simulation à 23:00",
                Font = new Font("Arial", 12),
                Location = new Point(140, 10),
                Size = new Size(200, 20),
                ForeColor = Color.DarkBlue
            };

            // Nom de la simulation
            _labelNomSimulation = new Label
            {
                Text = _simulation?.Nom ?? "Simulation",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(140, 30),
                Size = new Size(200, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Barre de progression
            _progressBar = new ProgressBar
            {
                Location = new Point(350, 20),
                Size = new Size(200, 20),
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            this.Controls.AddRange(new Control[] {
                _labelHeure, _labelFinSimulation, _labelNomSimulation, _progressBar
            });

            // Reprendre le layout
            this.ResumeLayout(false);
        }

        public void MettreAJour()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(MettreAJour));
                return;
            }

            // Éviter les mises à jour trop fréquentes
            if (_miseAJourEnCours) return;

            _miseAJourEnCours = true;
            try
            {
                var nouvelleHeure = _simulateur.Horloge.TempsActuel.ToString("HH:mm");
                
                // Ne mettre à jour l'heure que si elle a changé
                if (_derniereHeure != nouvelleHeure)
                {
                    _labelHeure.Text = nouvelleHeure;
                    _derniereHeure = nouvelleHeure;
                }

                // Mettre à jour la barre de progression
                if (_simulation != null)
                {
                    var heureDebut = new DateTime(2024, 1, 1, 6, 0, 0);
                    var heureFin = new DateTime(2024, 1, 1, 23, 0, 0);
                    var heureActuelle = _simulateur.Horloge.TempsActuel;

                    var totalMinutes = (heureFin - heureDebut).TotalMinutes;
                    var minutesEcoulees = (heureActuelle - heureDebut).TotalMinutes;

                    if (totalMinutes > 0)
                    {
                        var pourcentage = (int)Math.Max(0, Math.Min(100, (minutesEcoulees / totalMinutes) * 100));
                        
                        // Ne mettre à jour la barre de progression que si le pourcentage a changé
                        if (_dernierPourcentage != pourcentage)
                        {
                            _progressBar.Value = pourcentage;
                            _dernierPourcentage = pourcentage;
                        }
                    }
                }
            }
            finally
            {
                _miseAJourEnCours = false;
            }
        }
    }
}