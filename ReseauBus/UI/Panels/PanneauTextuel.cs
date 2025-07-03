using ReseauBus.Core.Models;
using ReseauBus.UI.Forms;

namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Panneau d'affichage textuel sans navigation - Affiche tous les bus
    /// </summary>
    public class PanneauTextuel : Panel, IPanneauSimulation
    {
        private Simulation _simulation;
        private ConfigurationSimulation _configuration;
        private CheckedListBox _checkBoxLignes;
        private RichTextBox _richTextBoxEvenements;
        private Label _labelNombreInfo;
        private Label _labelTitre;
        
        private List<InfoBusAmeliore> _evenementsAffiches;
        private string _dernierContenu = string.Empty;
        private bool _miseAJourEnCours = false;

        public PanneauTextuel(Simulation simulation, ConfigurationSimulation configuration)
        {
            _simulation = simulation;
            _configuration = configuration;
            _evenementsAffiches = new List<InfoBusAmeliore>();
            
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

            // Panel pour les contrôles (lignes seulement)
            var panelControles = new Panel
            {
                Location = new Point(5, 35),
                Size = new Size(this.Width - 10, 80),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            // GroupBox pour les lignes
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

            // Ajouter les lignes avec couleurs
            foreach (var ligne in _simulation.ListeLignes)
            {
                _checkBoxLignes.Items.Add($"● {ligne.Nom}", true);
            }
            _checkBoxLignes.ItemCheck += CheckBoxLignes_ItemCheck;

            groupBoxLignes.Controls.Add(_checkBoxLignes);

            // Label nombre d'informations (à droite)
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

            // Zone de texte enrichie pour les événements
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

            this.Controls.AddRange(new Control[] {
                _labelTitre, panelControles, _richTextBoxEvenements
            });

            // Reprendre le layout
            this.ResumeLayout(false);
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
            var simulateur = Simulateur.Instance;
            var heureActuelle = simulateur.Horloge.TempsActuel;

            // Utiliser les bus actifs au lieu des événements stockés
            foreach (var ligne in _simulation.ListeLignes)
            {
                if (EstLigneSelectionnee(ligne.Nom))
                {
                    // Récupérer tous les bus actifs de cette ligne
                    var busLigne = _simulation.ObtenirBusParLigne(ligne.Nom);
                    
                    foreach (var bus in busLigne)
                    {
                        // Créer un InfoBusAmeliore à partir de l'état actuel du bus
                        var infoBusAmeliore = CreerInfoBusDepuisBus(bus, numeroInfo, heureActuelle);
                        
                        if (infoBusAmeliore != null)
                        {
                            _evenementsAffiches.Add(infoBusAmeliore);
                            numeroInfo++;
                        }
                    }
                }
            }

            // Ne mettre à jour l'affichage que si les données ont changé
            if (_evenementsAffiches.Count != anciennesTaille)
            {
                _dernierContenu = string.Empty; // Forcer la mise à jour
            }
        }

        /// <summary>
        /// Crée un InfoBusAmeliore à partir de l'état actuel d'un bus
        /// </summary>
        private InfoBusAmeliore? CreerInfoBusDepuisBus(Bus bus, int numeroInfo, DateTime heureActuelle)
        {
            try
            {
                // Déterminer le statut
                var statut = bus.Statut == StatutBus.AArret ? "À l'arrêt" : "En circulation";
                
                // Déterminer le lieu de départ et d'arrivée
                string lieuDepart, lieuArrivee;
                
                if (bus.Statut == StatutBus.AArret)
                {
                    lieuDepart = bus.ArretActuel.Nom;
                    lieuArrivee = bus.ArretSuivant?.Nom ?? "Terminus";
                }
                else
                {
                    // En circulation - il va vers l'arrêt actuel
                    lieuDepart = "En circulation";
                    lieuArrivee = bus.ArretActuel.Nom;
                }
                
                var sensNom = bus.SensAller ? "aller" : "retour";
                var destination = bus.Destination;

                return new InfoBusAmeliore
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
            // Mettre à jour le compteur
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
        }

        private string GenererContenuAffichage()
        {
            // Afficher TOUS les événements (pas de pagination)
            return string.Join("|", _evenementsAffiches.Select(e => 
                $"{e.Heure}-{e.Immatriculation}-{e.Statut}-{e.TempsRestant}"));
        }

        private void AjouterContenuFormate()
        {
            // Afficher TOUS les événements (pas de pagination)
            foreach (var infoBus in _evenementsAffiches)
            {
                AjouterEvenementFormate(infoBus);
            }
        }

        private void AjouterEvenementFormate(InfoBusAmeliore infoBus)
        {
            // En-tête de l'événement
            _richTextBoxEvenements.SelectionFont = new Font("Arial", 11, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkBlue;
            _richTextBoxEvenements.AppendText($"{infoBus.Heure} - Info {infoBus.NumeroInfo} : Sur la ligne : {infoBus.Ligne}\n");

            // Immatriculation du bus
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Le bus immatriculé : ");
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            _richTextBoxEvenements.SelectionColor = Color.DarkGreen;
            _richTextBoxEvenements.AppendText($"{infoBus.Immatriculation}\n");

            // Statut du bus avec couleur appropriée
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9, FontStyle.Bold);
            var couleurStatut = infoBus.Statut switch
            {
                "À l'arrêt" => Color.Red,
                "En circulation" => Color.Blue,
                "Vient d'arriver" => Color.Green,
                "Vient de partir" => Color.Orange,
                _ => Color.Black
            };
            _richTextBoxEvenements.SelectionColor = couleurStatut;
            _richTextBoxEvenements.AppendText($"   {infoBus.Statut}\n");

            // Localisation
            _richTextBoxEvenements.SelectionFont = new Font("Consolas", 9);
            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Localisation : ");
            _richTextBoxEvenements.SelectionColor = Color.DarkRed;
            _richTextBoxEvenements.AppendText($"{infoBus.LieuDepart}\n");

            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Vers : ");
            _richTextBoxEvenements.SelectionColor = Color.DarkRed;
            _richTextBoxEvenements.AppendText($"{infoBus.LieuArrivee}\n");

            // Sens avec destination
            _richTextBoxEvenements.SelectionColor = Color.Black;
            _richTextBoxEvenements.AppendText($"   Sens circulation : ");
            _richTextBoxEvenements.SelectionColor = Color.Purple;
            _richTextBoxEvenements.AppendText($"{infoBus.SensNom} (direction {infoBus.Destination})\n");

            // Temps restant avec message approprié
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

            // Séparateur
            _richTextBoxEvenements.SelectionColor = Color.LightGray;
            _richTextBoxEvenements.AppendText("\n" + new string('─', 50) + "\n\n");
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

    /// <summary>
    /// Classe pour les informations de bus améliorées pour l'affichage
    /// </summary>
    public class InfoBusAmeliore
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