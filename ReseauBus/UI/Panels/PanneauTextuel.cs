using ReseauBus.Core.Models;
using ReseauBus.UI.Forms;

namespace ReseauBus.UI.Panels
{
    public class PanneauTextuel : Panel, IPanneauSimulation
    {
        private Simulation _simulation;
        private ConfigurationSimulation _configuration;
        private CheckedListBox _checkBoxLignes;
        private RichTextBox _richTextBoxEvenements;
        private Label _labelNombreInfo;
        private Label _labelTitre;

        private List<InfoBus> _evenementsAffiches;
        private string _dernierContenu = string.Empty;
        private bool _miseAJourEnCours = false;

        public PanneauTextuel(Simulation simulation, ConfigurationSimulation configuration)
        {
            _simulation = simulation;
            _configuration = configuration;
            _evenementsAffiches = new List<InfoBus>();

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

            this.SuspendLayout();

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

            var panelControles = new Panel
            {
                Location = new Point(5, 35),
                Size = new Size(this.Width - 10, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            var groupBoxLignes = new GroupBox
            {
                Text = "Lignes à afficher :",
                Font = new Font("Arial", 9, FontStyle.Bold),
                Location = new Point(5, 5),
                Size = new Size(panelControles.Width - 200, 70),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _checkBoxLignes = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                CheckOnClick = true,
                Font = new Font("Arial", 9),
                Margin = new Padding(5)
            };

            foreach (var ligne in _simulation.ListeLignes)
            {
                _checkBoxLignes.Items.Add($"● {ligne.Nom}", true);
            }

            _checkBoxLignes.ItemCheck += CheckBoxLignes_ItemCheck;

            groupBoxLignes.Controls.Add(_checkBoxLignes);

            _labelNombreInfo = new Label
            {
                Text = "0 informations",
                Location = new Point(panelControles.Width - 180, 25),
                Size = new Size(160, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            panelControles.Controls.Add(groupBoxLignes);
            panelControles.Controls.Add(_labelNombreInfo);

            _richTextBoxEvenements = new RichTextBox
            {
                Location = new Point(5, 120),
                Size = new Size(this.Width - 10, this.Height - 125),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap = false,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            this.Controls.AddRange(new Control[]
            {
                _labelTitre, panelControles, _richTextBoxEvenements
            });

            this.ResumeLayout(false);
        }

        private void CheckBoxLignes_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
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
            var simulateur = Simulateur.Instance;
            var heureActuelle = simulateur.Horloge.TempsActuel;

            foreach (var ligne in _simulation.ListeLignes)
            {
                if (EstLigneSelectionnee(ligne.Nom))
                {
                    var busLigne = _simulation.ObtenirBusParLigne(ligne.Nom);

                    foreach (var bus in busLigne)
                    {
                        var infoBus = CreerInfoBusDepuisBus(bus, numeroInfo, heureActuelle);

                        if (infoBus != null)
                        {
                            _evenementsAffiches.Add(infoBus);
                            numeroInfo++;
                        }
                    }
                }
            }

            if (_evenementsAffiches.Count != anciennesTaille)
            {
                _dernierContenu = string.Empty;
            }
        }

        private InfoBus? CreerInfoBusDepuisBus(Bus bus, int numeroInfo, DateTime heureActuelle)
        {
            try
            {
                if (!bus.PeutDemarrer)
                {
                    var minutesAvantDemarrage = (int)(bus.HeureDebutSimulation - heureActuelle).TotalMinutes;
                    return new InfoBus
                    {
                        Heure = heureActuelle.ToString("HH:mm"),
                        NumeroInfo = numeroInfo,
                        Ligne = bus.Ligne.Nom,
                        Immatriculation = bus.Immatriculation,
                        Statut = "En attente de démarrage",
                        LieuDepart = bus.ArretActuel.Nom,
                        LieuArrivee = "Démarrage prévu",
                        SensNom = bus.SensAller ? "aller" : "retour",
                        Destination = bus.Destination,
                        TempsRestant = minutesAvantDemarrage
                    };
                }

                var statut = bus.Statut == StatutBus.AArret ? "À l'arrêt" : "En circulation";

                string lieuDepart, lieuArrivee;

                if (bus.Statut == StatutBus.AArret)
                {
                    lieuDepart = bus.ArretActuel.Nom;
                    lieuArrivee = bus.ArretSuivant?.Nom ?? "Terminus";
                }
                else
                {
                    lieuDepart = "En circulation";
                    lieuArrivee = bus.ArretActuel.Nom;
                }

                var sensNom = bus.SensAller ? "aller" : "retour";
                var destination = bus.Destination;

                return new InfoBus
                {
                    Heure = heureActuelle.ToString("HH:mm"),
                    NumeroInfo = numeroInfo,
                    Ligne = bus.Ligne.Nom,
                    Immatriculation = bus.Immatriculation,
                    Statut = statut,
                    LieuDepart = lieuDepart,
                    LieuArrivee = lieuArrivee,
                    SensNom = sensNom,
                    Destination = destination,
                    TempsRestant = bus.TempsRestantMinutes
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERREUR] Création InfoBus pour {bus.Immatriculation}: {ex.Message}");
                return null;
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

        private void MettreAJourAffichage()
        {
            _labelNombreInfo.Text = $"{_evenementsAffiches.Count} informations";

            if (_evenementsAffiches.Count == 0)
            {
                var contenuVide = "Aucun événement récent à afficher pour les lignes sélectionnées.";
                if (_dernierContenu != contenuVide)
                {
                    _richTextBoxEvenements.Text = contenuVide;
                    _dernierContenu = contenuVide;
                }

                return;
            }

            var nouveauContenu = GenererContenuAffichage();

            if (_dernierContenu != nouveauContenu)
            {
                var scrollPos = _richTextBoxEvenements.SelectionStart;

                _richTextBoxEvenements.SuspendLayout();

                try
                {
                    _richTextBoxEvenements.Clear();
                    AjouterContenuFormate();

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
        }

        private string GenererContenuAffichage()
        {
            return string.Join("|", _evenementsAffiches.Select(e =>
                $"{e.Heure}-{e.Immatriculation}-{e.Statut}-{e.TempsRestant}"));
        }

        private void AjouterContenuFormate()
        {
            foreach (var infoBus in _evenementsAffiches)
            {
                AjouterEvenementFormate(infoBus);
            }
        }

        private void AjouterEvenementFormate(InfoBus infoBus)
        {
            _richTextBoxEvenements.SelectionFont = new Font("Arial", 11, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkBlue;
            _richTextBoxEvenements.AppendText(
                $"{infoBus.Heure} - Info {infoBus.NumeroInfo} : Sur la ligne : {infoBus.Ligne}\n");

            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Le bus immatriculé : ");
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkGreen;
            _richTextBoxEvenements.AppendText($"{infoBus.Immatriculation}\n");

            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            var couleurStatut = infoBus.Statut switch
            {
                "À l'arrêt" => Color.Red,
                "En circulation" => Color.Blue,
                "En attente de démarrage" => Color.Purple,
                "Vient d'arriver" => Color.Green,
                "Vient de partir" => Color.Orange,
                _ => Color.Black
            };
            _richTextBoxEvenements.SelectionColor = couleurStatut;
            _richTextBoxEvenements.AppendText($"   {infoBus.Statut}\n");

            if (infoBus.Statut == "En attente de démarrage")
            {
                _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Lieu de démarrage : ");
                _richTextBoxEvenements.SelectionColor = Color.DarkRed;
                _richTextBoxEvenements.AppendText($"{infoBus.LieuDepart}\n");

                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Sens circulation : ");
                _richTextBoxEvenements.SelectionColor = Color.Purple;
                _richTextBoxEvenements.AppendText($"{infoBus.SensNom} (direction {infoBus.Destination})\n");

                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Démarrage dans : ");
                _richTextBoxEvenements.SelectionColor = Color.Purple;
                _richTextBoxEvenements.AppendText($"{infoBus.TempsRestant} min\n");
            }
            else
            {
                _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Localisation : ");
                _richTextBoxEvenements.SelectionColor = Color.DarkRed;
                _richTextBoxEvenements.AppendText($"{infoBus.LieuDepart}\n");

                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Vers : ");
                _richTextBoxEvenements.SelectionColor = Color.DarkRed;
                _richTextBoxEvenements.AppendText($"{infoBus.LieuArrivee}\n");

                _richTextBoxEvenements.SelectionColor = Color.Black;
                _richTextBoxEvenements.AppendText($"   Sens circulation : ");
                _richTextBoxEvenements.SelectionColor = Color.Purple;
                _richTextBoxEvenements.AppendText($"{infoBus.SensNom} (direction {infoBus.Destination})\n");

                _richTextBoxEvenements.SelectionColor = Color.Black;
                var messageTemps = infoBus.Statut switch
                {
                    "À l'arrêt" => $"   Temps d'arrêt restant : ",
                    "En circulation" => $"   Arrivée prévue dans : ",
                    "Vient d'arriver" => $"   Vient d'arriver à l'arrêt",
                    "Vient de partir" => $"   Vient de quitter l'arrêt",
                    _ => $"   Temps restant : "
                };

                _richTextBoxEvenements.AppendText(messageTemps);
                if (infoBus.Statut == "À l'arrêt" || infoBus.Statut == "En circulation")
                {
                    _richTextBoxEvenements.SelectionColor = Color.Blue;
                    _richTextBoxEvenements.AppendText($"{infoBus.TempsRestant} min\n");
                }
                else
                {
                    _richTextBoxEvenements.AppendText("\n");
                }
            }

            _richTextBoxEvenements.SelectionColor = Color.LightGray;
            _richTextBoxEvenements.AppendText("\n" + new string('─', 50) + "\n\n");
        }

        public void Demarrer()
        {
        }

        public void Arreter()
        {
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
        }
    }

    public class InfoBus
    {
        public string Heure { get; set; } = string.Empty;
        public int NumeroInfo { get; set; }
        public string Ligne { get; set; } = string.Empty;
        public string Immatriculation { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public string LieuDepart { get; set; } = string.Empty;
        public string LieuArrivee { get; set; } = string.Empty;
        public string SensNom { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;

        public int TempsRestant { get; set; }

    }
}