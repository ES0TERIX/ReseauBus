using ReseauBus.Core.Models;

namespace ReseauBus.UI.Panels
{
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

            this.SuspendLayout();

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

            PositionnerControles();

            this.ResumeLayout(false);
        }

        private void PositionnerControles()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                _labelHeure.Size = new Size(200, 80);
                _labelHeure.Location = new Point(
                    (this.Width - _labelHeure.Width) / 2,
                    (this.Height - _labelHeure.Height) / 2 - 10
                );

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

            if (_miseAJourEnCours) return;

            _miseAJourEnCours = true;
            try
            {
                var nouvelleHeure = _simulateur.Horloge.TempsActuel.ToString("HH:mm");
                
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
            
            PositionnerControles();
        }
    }
}