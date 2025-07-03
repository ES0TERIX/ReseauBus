using ReseauBus.Core.Models;
using ReseauBus.UI.Forms;

namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Panneau d'affichage textuel soigné sans clignotement
    /// </summary>
    public class PanneauTextuel : Panel, IPanneauSimulation
    {
        private Simulation _simulation;
        private ConfigurationSimulation _configuration;
        private CheckedListBox _checkBoxLignes;
        private RichTextBox _richTextBoxEvenements;
        private Label _labelNombreInfo;
        private Button _btnPrecedent;
        private Button _btnSuivant;
        private Button _btnHaut;
        private Button _btnBas;
        private Label _labelTitre;
        
        private List<InfoBus> _evenementsAffiches;
        private int _indexEvenementActuel;
        private const int EVENEMENTS_PAR_PAGE = 4;
        private string _dernierContenu = string.Empty;
        private bool _miseAJourEnCours = false;

        public PanneauTextuel(Simulation simulation, ConfigurationSimulation configuration)
        {
            _simulation = simulation;
            _configuration = configuration;
            _evenementsAffiches = new List<InfoBus>();
            _indexEvenementActuel = 0;
            
            // Activer le double buffering pour éviter le scintillement
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer, true);
            
            InitialiserControles();
            MettreAJourEvenements();
        }

        private void InitialiserControles()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.WhiteSmoke;

            // Suspendre le layout pendant la création
            this.SuspendLayout();

            // Titre du panneau
            _labelTitre = new Label
            {
                Text = $"Simulation: {_simulation.Nom}",
                Font = new Font("Arial", 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                BackColor = Color.LightSteelBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Panel pour les contrôles (lignes + navigation)
            var panelControles = new TableLayoutPanel
            {
                Location = new Point(5, 35),
                Size = new Size(this.Width - 10, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };
            panelControles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            panelControles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // GroupBox pour les lignes
            var groupBoxLignes = new GroupBox
            {
                Text = "Lignes à afficher :",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(3)
            };

            _checkBoxLignes = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Font = new Font("Arial", 9),
                Margin = new Padding(5)
            };

            // Ajouter les lignes avec couleurs
            foreach (var ligne in _simulation.ListeLignes)
            {
                _checkBoxLignes.Items.Add($"● {ligne.Nom}", true);
            }
            _checkBoxLignes.ItemCheck += CheckBoxLignes_ItemCheck;

            groupBoxLignes.Controls.Add(_checkBoxLignes);

            // GroupBox pour la navigation
            var groupBoxNavigation = new GroupBox
            {
                Text = "Navigation :",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(3)
            };

            var panelBoutons = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5)
            };

            // Boutons de navigation avec style
            _btnHaut = CreerBoutonNavigation("▲", new Point(60, 15), "Page précédente");
            _btnBas = CreerBoutonNavigation("▼", new Point(60, 50), "Page suivante");
            _btnPrecedent = CreerBoutonNavigation("◄", new Point(20, 32), "Événement précédent");
            _btnSuivant = CreerBoutonNavigation("►", new Point(100, 32), "Événement suivant");

            _btnPrecedent.Click += BtnPrecedent_Click;
            _btnSuivant.Click += BtnSuivant_Click;
            _btnHaut.Click += BtnHaut_Click;
            _btnBas.Click += BtnBas_Click;

            // Label nombre d'informations
            _labelNombreInfo = new Label
            {
                Text = "0 informations",
                Location = new Point(10, 80),
                Size = new Size(140, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Arial", 8, FontStyle.Bold)
            };

            panelBoutons.Controls.AddRange(new Control[] {
                _btnPrecedent, _btnHaut, _btnBas, _btnSuivant, _labelNombreInfo
            });

            groupBoxNavigation.Controls.Add(panelBoutons);

            panelControles.Controls.Add(groupBoxLignes, 0, 0);
            panelControles.Controls.Add(groupBoxNavigation, 1, 0);

            // Zone de texte enrichie pour les événements
            _richTextBoxEvenements = new RichTextBox
            {
                Location = new Point(5, 160),
                Size = new Size(this.Width - 10, this.Height - 165),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            this.Controls.AddRange(new Control[] {
                _labelTitre, panelControles, _richTextBoxEvenements
            });

            // Reprendre le layout
            this.ResumeLayout(false);
        }

        private Button CreerBoutonNavigation(string texte, Point position, string tooltip)
        {
            var bouton = new Button
            {
                Text = texte,
                Location = position,
                Size = new Size(30, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                FlatStyle = FlatStyle.Popup
            };

            var toolTip = new ToolTip();
            toolTip.SetToolTip(bouton, tooltip);

            return bouton;
        }

        private void CheckBoxLignes_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Délai pour éviter les mises à jour trop fréquentes
            if (!_miseAJourEnCours)
            {
                _miseAJourEnCours = true;
                this.BeginInvoke(new Action(() =>
                {
                    MettreAJourAffichage();
                    _miseAJourEnCours = false;
                }));
            }
        }

        private void BtnPrecedent_Click(object? sender, EventArgs e)
        {
            if (_indexEvenementActuel > 0)
            {
                _indexEvenementActuel--;
                MettreAJourAffichage();
            }
        }

        private void BtnSuivant_Click(object? sender, EventArgs e)
        {
            if (_indexEvenementActuel < _evenementsAffiches.Count - 1)
            {
                _indexEvenementActuel++;
                MettreAJourAffichage();
            }
        }

        private void BtnHaut_Click(object? sender, EventArgs e)
        {
            if (_indexEvenementActuel >= EVENEMENTS_PAR_PAGE)
            {
                _indexEvenementActuel -= EVENEMENTS_PAR_PAGE;
                MettreAJourAffichage();
            }
        }

        private void BtnBas_Click(object? sender, EventArgs e)
        {
            if (_indexEvenementActuel + EVENEMENTS_PAR_PAGE < _evenementsAffiches.Count)
            {
                _indexEvenementActuel += EVENEMENTS_PAR_PAGE;
                MettreAJourAffichage();
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
                MettreAJourEvenements();
                MettreAJourAffichage();
            }
            finally
            {
                _miseAJourEnCours = false;
            }
        }

        private void MettreAJourEvenements()
        {
            var anciennesTaille = _evenementsAffiches.Count;
            _evenementsAffiches.Clear();
            var numeroInfo = 1;

            foreach (var ligne in _simulation.ListeLignes)
            {
                if (EstLigneSelectionnee(ligne.Nom))
                {
                    foreach (var evenement in ligne.ListeEvenements.OrderBy(e => e.Heure))
                    {
                        var infoBus = new InfoBus
                        {
                            Heure = evenement.Heure,
                            Immatriculation = evenement.Bus,
                            LieuDepart = evenement.Depart.Nom,
                            LieuArrivee = evenement.Arrivee.Nom,
                            Sens = DeterminerSens(ligne, evenement),
                            Duree = evenement.Duree,
                            Bus = ligne.Nom
                        };

                        infoBus.ListeEvenements = $"Info {numeroInfo}";
                        _evenementsAffiches.Add(infoBus);
                        numeroInfo++;
                    }
                }
            }

            // Ne mettre à jour l'affichage que si les données ont changé
            if (_evenementsAffiches.Count != anciennesTaille)
            {
                _dernierContenu = string.Empty; // Forcer la mise à jour
            }
        }

        private bool EstLigneSelectionnee(string nomLigne)
        {
            for (int i = 0; i < _checkBoxLignes.Items.Count; i++)
            {
                if (_checkBoxLignes.Items[i].ToString()?.Contains(nomLigne) == true)
                {
                    return _checkBoxLignes.GetItemChecked(i);
                }
            }
            return false;
        }

        private int DeterminerSens(LigneBus ligne, Evenement evenement)
        {
            var indexDepart = ligne.ListArret.FindIndex(a => a.Id == evenement.Depart.Id);
            var indexArrivee = ligne.ListArret.FindIndex(a => a.Id == evenement.Arrivee.Id);
            return indexArrivee > indexDepart ? 1 : -1;
        }

        private void MettreAJourAffichage()
        {
            // Mettre à jour le compteur
            _labelNombreInfo.Text = $"{_evenementsAffiches.Count} informations";

            if (_evenementsAffiches.Count == 0)
            {
                var contenuVide = "Aucun événement à afficher pour les lignes sélectionnées.";
                if (_dernierContenu != contenuVide)
                {
                    _richTextBoxEvenements.Text = contenuVide;
                    _dernierContenu = contenuVide;
                }
                return;
            }

            // Générer le nouveau contenu
            var nouveauContenu = GenererContenuAffichage();
            
            // Ne mettre à jour que si le contenu a changé
            if (_dernierContenu != nouveauContenu)
            {
                // Sauvegarder la position de scroll
                var scrollPos = _richTextBoxEvenements.SelectionStart;
                
                // Supprimer le scintillement
                _richTextBoxEvenements.SuspendLayout();
                
                try
                {
                    _richTextBoxEvenements.Clear();
                    AjouterContenuFormate();
                    
                    // Restaurer la position si possible
                    if (scrollPos < _richTextBoxEvenements.Text.Length)
                    {
                        _richTextBoxEvenements.SelectionStart = scrollPos;
                        _richTextBoxEvenements.ScrollToCaret();
                    }
                }
                finally
                {
                    _richTextBoxEvenements.ResumeLayout();
                }
                
                _dernierContenu = nouveauContenu;
            }

            // Mettre à jour l'état des boutons
            MettreAJourBoutons();
        }

        private string GenererContenuAffichage()
        {
            var evenementsDePage = _evenementsAffiches
                .Skip(_indexEvenementActuel)
                .Take(EVENEMENTS_PAR_PAGE)
                .ToList();

            return string.Join("|", evenementsDePage.Select(e => 
                $"{e.Heure}-{e.Immatriculation}-{e.LieuDepart}-{e.LieuArrivee}"));
        }

        private void AjouterContenuFormate()
        {
            var evenementsDePage = _evenementsAffiches
                .Skip(_indexEvenementActuel)
                .Take(EVENEMENTS_PAR_PAGE)
                .ToList();

            foreach (var infoBus in evenementsDePage)
            {
                AjouterEvenementFormate(infoBus);
            }
        }

        private void AjouterEvenementFormate(InfoBus infoBus)
        {
            // En-tête de l'événement
            _richTextBoxEvenements.SelectionFont = new Font("Arial", 11, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkBlue;
            _richTextBoxEvenements.AppendText($"{infoBus.Heure} - {infoBus.ListeEvenements} : Sur la ligne : {infoBus.Bus}\n");

            // Détails du bus
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
            _richTextBoxEvenements.SelectionColor = Color.Black;

            _richTextBoxEvenements.AppendText($"   Le bus immatriculé : ");
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkGreen;
            _richTextBoxEvenements.AppendText($"{infoBus.Immatriculation}\n");

            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   En circulation\n");

            _richTextBoxEvenements.AppendText($"   De : ");
            _richTextBoxEvenements.SelectionColor = Color.DarkRed;
            _richTextBoxEvenements.AppendText($"{infoBus.LieuDepart}\n");

            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Vers : ");
            _richTextBoxEvenements.SelectionColor = Color.DarkRed;
            _richTextBoxEvenements.AppendText($"{infoBus.LieuArrivee}\n");

            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Sens circulation : ");
            _richTextBoxEvenements.SelectionColor = infoBus.Sens == 1 ? Color.Green : Color.Orange;
            _richTextBoxEvenements.AppendText($"{infoBus.Sens}\n");

            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Pour une durée de : ");
            _richTextBoxEvenements.SelectionColor = Color.Blue;
            _richTextBoxEvenements.AppendText($"{infoBus.Duree}\n");

            // Séparateur
            _richTextBoxEvenements.SelectionColor = Color.LightGray;
            _richTextBoxEvenements.AppendText("\n" + new string('─', 50) + "\n\n");
        }

        private void MettreAJourBoutons()
        {
            _btnPrecedent.Enabled = _indexEvenementActuel > 0;
            _btnSuivant.Enabled = _indexEvenementActuel < _evenementsAffiches.Count - 1;
            _btnHaut.Enabled = _indexEvenementActuel >= EVENEMENTS_PAR_PAGE;
            _btnBas.Enabled = _indexEvenementActuel + EVENEMENTS_PAR_PAGE < _evenementsAffiches.Count;

            // Changer la couleur des boutons selon leur état
            foreach (var btn in new[] { _btnPrecedent, _btnSuivant, _btnHaut, _btnBas })
            {
                btn.BackColor = btn.Enabled ? Color.LightBlue : Color.LightGray;
                btn.ForeColor = btn.Enabled ? Color.DarkBlue : Color.Gray;
            }
        }

        public void Demarrer()
        {
            // Logique de démarrage si nécessaire
        }

        public void Arreter()
        {
            // Logique d'arrêt si nécessaire
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Réajuster la taille des contrôles si nécessaire
        }
    }
}