using ReseauBus.Core.Models;
using ReseauBus.Core.Interfaces;
using ReseauBus.UI.Panels;
using ReseauBus.Data;

namespace ReseauBus.UI.Forms
{
    public partial class FormSimulation : Form, IInterfaceUtilisateur
    {
        private List<ConfigurationSimulation> _configurations;
        private Simulateur _simulateur;
        private List<Simulation> _simulations;
        private PanneauEnTete _panneauEnTete;
        private List<IPanneauSimulation> _panneaux;

        public FormSimulation(List<ConfigurationSimulation> configurations)
        {
            _configurations = configurations;
            _simulations = new List<Simulation>();
            _panneaux = new List<IPanneauSimulation>();
            _simulateur = Simulateur.Instance;
            
            InitializeComponent();
            InitialiserSimulations();
            CreerInterface();
            
            _simulateur.AjouterObservateur(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Simulation du réseau de bus - Version textuelle";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private void InitialiserSimulations()
        {
            var lignes = DonneesAmiens.ObtenirLignes();

            foreach (var config in _configurations)
            {
                var simulation = new Simulation(config.NomSimulation);
                
                simulation.HeureDebut = config.HeureDebut;
                simulation.HeureFin = config.HeureFin;
                
                Console.WriteLine($"[SIMULATION] {simulation.Nom} configurée: {simulation.HeureDebut:HH:mm} - {simulation.HeureFin:HH:mm}");
                
                if (config.NomSimulation.Contains("Amiens"))
                {
                    foreach (var ligne in lignes)
                    {
                        simulation.AjouterLigne(ligne);
                    }
                }

                _simulations.Add(simulation);
            }
        }

        private void CreerInterface()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(5)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            _panneauEnTete = new PanneauEnTete(_simulations.FirstOrDefault());
            mainPanel.Controls.Add(_panneauEnTete, 0, 0);
            mainPanel.SetColumnSpan(_panneauEnTete, 2);

            CreerPanneauxSimulation(mainPanel);

            this.Controls.Add(mainPanel);
        }

        private void CreerPanneauxSimulation(TableLayoutPanel mainPanel)
        {
            var positions = new (int col, int row)[] { (0, 1), (1, 1), (0, 2), (1, 2) };

            for (int i = 0; i < _configurations.Count && i < 4; i++)
            {
                var config = _configurations[i];
                var simulation = _simulations[i];
                IPanneauSimulation panneau;

                if (config.TypeVisualisation == TypeVisualisation.Textuelle)
                {
                    panneau = new PanneauTextuel(simulation, config);
                    _panneaux.Add(panneau);
                    mainPanel.Controls.Add((Control)panneau, positions[i].col, positions[i].row);
                }
                else
                {
                    var panneauVide = new Panel
                    {
                        BackColor = Color.LightGray,
                        BorderStyle = BorderStyle.FixedSingle
                    };
                    
                    var labelVide = new Label
                    {
                        Text = "Aucune visualisation",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Arial", 12, FontStyle.Italic),
                        ForeColor = Color.Gray
                    };
                    
                    panneauVide.Controls.Add(labelVide);
                    mainPanel.Controls.Add(panneauVide, positions[i].col, positions[i].row);
                }
            }
        }

        public void Actualiser()
        {
            _panneauEnTete?.MettreAJour();

            foreach (var panneau in _panneaux)
            {
                panneau.MettreAJour();
            }
        }

        private void FormSimulation_Load(object sender, EventArgs e)
        {
            if (_simulations.Count > 0)
            {
                var premiereHeure = _simulations.Min(s => s.HeureDebut);
                Console.WriteLine($"[FORM] Définition de l'heure de début à {premiereHeure:HH:mm}");
                
                if (Horloge.Instance.EnMarche)
                {
                    Horloge.Instance.Stop();
                }
                
                Horloge.Instance.DefinirHeureDebut(premiereHeure);
            }
            
            foreach (var simulation in _simulations)
            {
                _simulateur.LancerSimulation(simulation);
            }
        }

        private void FormSimulation_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var simulation in _simulations)
            {
                _simulateur.ArreterSimulation(simulation);
            }

            _simulateur.SupprimerObservateur(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            FormSimulation_Load(this, e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            FormSimulation_FormClosing(this, e);
            base.OnFormClosing(e);
        }
    }
}