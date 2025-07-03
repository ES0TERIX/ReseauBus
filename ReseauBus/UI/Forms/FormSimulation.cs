using ReseauBus.Core.Models;

using ReseauBus.Core.Interfaces;

using ReseauBus.UI.Panels;

using ReseauBus.Data;



namespace ReseauBus.UI.Forms

{

    /// <summary>

    /// Formulaire principal de simulation - Implémente IObserver - Version textuelle uniquement

    /// </summary>

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

            

            // S'abonner aux notifications du simulateur

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

                

                // Ajouter toutes les lignes pour "Amiens semaine"

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

            // Panel principal

            var mainPanel = new TableLayoutPanel

            {

                Dock = DockStyle.Fill,

                ColumnCount = 2,

                RowCount = 3,

                Padding = new Padding(5)

            };



            // Configuration des colonnes et lignes

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F)); // En-tête

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // Haut

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));  // Bas



            // Panneau d'en-tête

            _panneauEnTete = new PanneauEnTete(_simulations.FirstOrDefault());

            mainPanel.Controls.Add(_panneauEnTete, 0, 0);

            mainPanel.SetColumnSpan(_panneauEnTete, 2);



            // Créer les panneaux selon les configurations

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



                // MODIFICATION : Suppression du cas "Graphique"

                // Seuls les panneaux textuels sont créés maintenant

                if (config.TypeVisualisation == TypeVisualisation.Textuelle)

                {

                    panneau = new PanneauTextuel(simulation, config);

                    _panneaux.Add(panneau);

                    mainPanel.Controls.Add((Control)panneau, positions[i].col, positions[i].row);

                }

                else

                {

                    // Pour "Aucune", on peut créer un panneau vide ou ne rien mettre

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

            // Mettre à jour l'en-tête

            _panneauEnTete?.MettreAJour();



            // Mettre à jour tous les panneaux

            foreach (var panneau in _panneaux)

            {

                panneau.MettreAJour();

            }

        }



        private void FormSimulation_Load(object sender, EventArgs e)

        {

            // Démarrer les simulations

            foreach (var simulation in _simulations)

            {

                _simulateur.LancerSimulation(simulation);

            }

        }



        private void FormSimulation_FormClosing(object sender, FormClosingEventArgs e)

        {

            // Arrêter les simulations

            foreach (var simulation in _simulations)

            {

                _simulateur.ArreterSimulation(simulation);

            }



            // Se désabonner des notifications

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