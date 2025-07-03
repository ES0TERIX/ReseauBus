using ReseauBus.Core.Models;

namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Panneau d'en-tête simplifié - Affichage uniquement de l'heure de simulation
    /// </summary>
    public class PanneauEnTete : Panel
    {
        private Label _labelHeure;
        private Label _labelNomSimulation;
        private Simulation? _simulation;
        private Simulateur _simulateur;
        private string _derniereHeure = string.Empty;
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

            // Heure actuelle (très grande et claire - centrée)
            _labelHeure = new Label
            {
                Text = DateTime.Now.ToString("HH:mm"),
                Font = new Font("Arial", 36, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.None,
                AutoSize = false
            };

            // Nom de la simulation (plus petit, en bas)
            _labelNomSimulation = new Label
            {
                Text = _simulation?.Nom ?? "Simulation",
                Font = new Font("Arial", 14, FontStyle.Regular),
                ForeColor = Color.DarkBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Dock = DockStyle.None,
                AutoSize = false
            };

            this.Controls.AddRange(new Control[] {
                _labelHeure, _labelNomSimulation
            });

            // Positionner les contrôles après ajout
            PositionnerControles();

            // Reprendre le layout
            this.ResumeLayout(false);
        }

        private void PositionnerControles()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                // Centrer l'heure horizontalement et verticalement - plus large verticalement
                _labelHeure.Size = new Size(200, 80);
                _labelHeure.Location = new Point(
                    (this.Width - _labelHeure.Width) / 2,
                    (this.Height - _labelHeure.Height) / 2 - 10
                );

                // Nom de simulation en bas, centré
                _labelNomSimulation.Size = new Size(300, 20);
                _labelNomSimulation.Location = new Point(
                    (this.Width - _labelNomSimulation.Width) / 2,
                    this.Height - 25
                );
            }
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
            }
            finally
            {
                _miseAJourEnCours = false;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            // Repositionner les contrôles quand la fenêtre change de taille
            PositionnerControles();
        }
    }
}