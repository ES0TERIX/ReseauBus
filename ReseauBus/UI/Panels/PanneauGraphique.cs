using ReseauBus.Core.Models;
using ReseauBus.UI.Forms;

namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Panneau d'affichage graphique avec suivi réel des bus
    /// </summary>
    public class PanneauGraphique : Panel, IPanneauSimulation
    {
        private Simulation _simulation;
        private ConfigurationSimulation _configuration;
        private CheckedListBox _checkBoxLignes;
        private CheckBox _checkBoxAfficherNoms;
        private TrackBar _trackBarZoom;
        private Panel _panelDessin;
        private Dictionary<int, Color> _couleursBus;
        private Dictionary<int, Point> _positionsArrets;
        
        // Nouvelles structures pour suivre les bus réels
        private Dictionary<string, BusEnMouvement> _busEnMouvement;
        private DateTime _derniereMiseAJour;

        public PanneauGraphique(Simulation simulation, ConfigurationSimulation configuration)
        {
            _simulation = simulation;
            _configuration = configuration;
            _couleursBus = new Dictionary<int, Color>();
            _positionsArrets = new Dictionary<int, Point>();
            _busEnMouvement = new Dictionary<string, BusEnMouvement>();
            _derniereMiseAJour = DateTime.MinValue;
            
            InitialiserControles();
            CalculerPositions();
        }

        private void InitialiserControles()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            // Panel de contrôles à droite
            var panelControles = new Panel
            {
                Dock = DockStyle.Right,
                Width = 200,
                BackColor = Color.LightGray,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Barre de zoom
            var lblZoom = new Label
            {
                Text = "Zoom",
                Location = new Point(10, 10),
                Size = new Size(50, 20)
            };

            _trackBarZoom = new TrackBar
            {
                Location = new Point(10, 30),
                Size = new Size(180, 45),
                Minimum = 1,
                Maximum = 5,
                Value = 2,
                TickStyle = TickStyle.BottomRight
            };
            _trackBarZoom.ValueChanged += TrackBarZoom_ValueChanged;

            // Checkbox pour afficher les noms
            _checkBoxAfficherNoms = new CheckBox
            {
                Text = "Afficher les arrêts",
                Location = new Point(10, 80),
                Size = new Size(180, 20),
                Checked = true
            };
            _checkBoxAfficherNoms.CheckedChanged += CheckBoxAfficherNoms_CheckedChanged;

            // Liste des lignes
            var lblLignes = new Label
            {
                Text = "Lignes à afficher :",
                Location = new Point(10, 110),
                Size = new Size(180, 20)
            };

            _checkBoxLignes = new CheckedListBox
            {
                Location = new Point(10, 130),
                Size = new Size(180, 150),
                CheckOnClick = true
            };

            // Ajouter les lignes
            foreach (var ligne in _simulation.ListeLignes)
            {
                _checkBoxLignes.Items.Add(ligne.Nom, true);
                _couleursBus[ligne.GetHashCode()] = ligne.Couleur;
            }

            _checkBoxLignes.ItemCheck += CheckBoxLignes_ItemCheck;

            panelControles.Controls.AddRange(new Control[] {
                lblZoom, _trackBarZoom, _checkBoxAfficherNoms, lblLignes, _checkBoxLignes
            });

            // Panel de dessin
            _panelDessin = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _panelDessin.Paint += PanelDessin_Paint;

            this.Controls.AddRange(new Control[] { _panelDessin, panelControles });
        }

        private void CalculerPositions()
        {
            _positionsArrets.Clear();

            // Trouver les limites du réseau
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            var tousArrets = new List<Arret>();
            foreach (var ligne in _simulation.ListeLignes)
            {
                foreach (var arret in ligne.ListArret)
                {
                    if (!tousArrets.Any(a => a.Id == arret.Id))
                    {
                        tousArrets.Add(arret);
                        minX = Math.Min(minX, arret.X);
                        minY = Math.Min(minY, arret.Y);
                        maxX = Math.Max(maxX, arret.X);
                        maxY = Math.Max(maxY, arret.Y);
                    }
                }
            }

            // Calculer les positions à l'écran
            float echelle = Math.Min(
                (_panelDessin.Width - 40) / (maxX - minX),
                (_panelDessin.Height - 40) / (maxY - minY)
            ) * _trackBarZoom.Value * 0.5f;

            foreach (var arret in tousArrets)
            {
                int x = (int)((arret.X - minX) * echelle + 20);
                int y = (int)((arret.Y - minY) * echelle + 20);
                _positionsArrets[arret.Id] = new Point(x, y);
            }
        }

        private void PanelDessin_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Dessiner les lignes sélectionnées
            foreach (var ligne in _simulation.ListeLignes)
            {
                if (_checkBoxLignes.CheckedItems.Contains(ligne.Nom))
                {
                    DessinerLigne(g, ligne);
                }
            }

            // Dessiner les arrêts
            DessinerArrets(g);

            // Dessiner les bus en mouvement
            DessinerBusEnMouvement(g);

            // Dessiner les noms des arrêts si demandé
            if (_checkBoxAfficherNoms.Checked)
            {
                DessinerNomsArrets(g);
            }
        }

        private void DessinerLigne(Graphics g, LigneBus ligne)
        {
            using var pen = new Pen(ligne.Couleur, 2);
            
            for (int i = 0; i < ligne.ListArret.Count - 1; i++)
            {
                var arret1 = ligne.ListArret[i];
                var arret2 = ligne.ListArret[i + 1];

                if (_positionsArrets.ContainsKey(arret1.Id) && _positionsArrets.ContainsKey(arret2.Id))
                {
                    var pos1 = _positionsArrets[arret1.Id];
                    var pos2 = _positionsArrets[arret2.Id];
                    g.DrawLine(pen, pos1, pos2);
                }
            }
        }

        private void DessinerArrets(Graphics g)
        {
            // Trouver les arrêts où des bus sont stationnés
            var arretsAvecBus = new HashSet<int>();
            var couleurParArret = new Dictionary<int, Color>();

            var simulateur = Simulateur.Instance;
            var heureActuelle = simulateur.Horloge.TempsActuel;

            foreach (var ligne in _simulation.ListeLignes)
            {
                if (!_checkBoxLignes.CheckedItems.Contains(ligne.Nom)) continue;

                foreach (var evenement in ligne.ListeEvenements)
                {
                    try
                    {
                        // Parser l'heure et la durée correctement
                        if (!DateTime.TryParse($"2024-01-01 {evenement.Heure}:00", out var heureDepart))
                            continue;

                        var dureeString = evenement.Duree.Split(':');
                        if (dureeString.Length < 3) continue;
                        
                        var dureeMinutes = double.Parse(dureeString[0]) * 60 + 
                                         double.Parse(dureeString[1]) + 
                                         double.Parse(dureeString[2]) / 60.0;
                        
                        var heureArrivee = heureDepart.AddMinutes(dureeMinutes);

                        // Adapter l'heure actuelle
                        var heureActuelleAdaptee = new DateTime(2024, 1, 1, 
                            heureActuelle.Hour, heureActuelle.Minute, heureActuelle.Second);

                        // Vérifier si le bus vient d'arriver (dans les 30 dernières secondes)
                        var deltaArrivee = (heureActuelleAdaptee - heureArrivee).TotalSeconds;
                        if (deltaArrivee >= 0 && deltaArrivee <= 30)
                        {
                            arretsAvecBus.Add(evenement.Arrivee.Id);
                            couleurParArret[evenement.Arrivee.Id] = ligne.Couleur;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur lors du traitement de l'arrêt pour {evenement.Heure}: {ex.Message}");
                    }
                }
            }

            // Dessiner tous les arrêts
            foreach (var kvp in _positionsArrets)
            {
                var arretId = kvp.Key;
                var position = kvp.Value;

                if (arretsAvecBus.Contains(arretId))
                {
                    // Arrêt avec bus : utiliser la couleur de la ligne
                    var couleurBus = couleurParArret[arretId];
                    using var brush = new SolidBrush(couleurBus);
                    g.FillEllipse(brush, position.X - 6, position.Y - 6, 12, 12);
                    using var penBorder = new Pen(Color.White, 2);
                    g.DrawEllipse(penBorder, position.X - 6, position.Y - 6, 12, 12);
                }
                else
                {
                    // Arrêt normal : point noir
                    using var brush = new SolidBrush(Color.Black);
                    g.FillEllipse(brush, position.X - 3, position.Y - 3, 6, 6);
                }
            }
        }

        private void DessinerBusEnMouvement(Graphics g)
        {
            foreach (var bus in _busEnMouvement.Values)
            {
                if (!_checkBoxLignes.CheckedItems.Contains(bus.NomLigne)) continue;

                // Dessiner le bus comme une flèche
                var couleur = _simulation.ListeLignes
                    .FirstOrDefault(l => l.Nom == bus.NomLigne)?.Couleur ?? Color.Red;

                DessinerFlecheBus(g, bus.Position, bus.Direction, couleur);
            }
        }

        private void DessinerFlecheBus(Graphics g, PointF position, float angle, Color couleur)
        {
            const int tailleBus = 8;
            
            // Calculer les points de la flèche
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            
            var points = new PointF[]
            {
                new PointF(
                    position.X + (float)(tailleBus * cos), 
                    position.Y + (float)(tailleBus * sin)
                ), // Pointe
                new PointF(
                    position.X + (float)(-tailleBus/2 * cos - tailleBus/3 * sin), 
                    position.Y + (float)(-tailleBus/2 * sin + tailleBus/3 * cos)
                ), // Base gauche
                new PointF(
                    position.X + (float)(-tailleBus/2 * cos + tailleBus/3 * sin), 
                    position.Y + (float)(-tailleBus/2 * sin - tailleBus/3 * cos)
                )  // Base droite
            };

            using var brush = new SolidBrush(couleur);
            g.FillPolygon(brush, points);
            using var pen = new Pen(Color.Black, 1);
            g.DrawPolygon(pen, points);
        }

        private void DessinerNomsArrets(Graphics g)
        {
            using var font = new Font("Arial", 8);
            using var brush = new SolidBrush(Color.DarkBlue);

            var tousArrets = new List<Arret>();
            foreach (var ligne in _simulation.ListeLignes)
            {
                foreach (var arret in ligne.ListArret)
                {
                    if (!tousArrets.Any(a => a.Id == arret.Id))
                    {
                        tousArrets.Add(arret);
                    }
                }
            }

            foreach (var arret in tousArrets)
            {
                if (_positionsArrets.ContainsKey(arret.Id))
                {
                    var position = _positionsArrets[arret.Id];
                    var textSize = g.MeasureString(arret.Nom, font);
                    
                    // Fond blanc pour la lisibilité
                    using var whiteBrush = new SolidBrush(Color.FromArgb(200, Color.White));
                    g.FillRectangle(whiteBrush, 
                        position.X + 8, position.Y - textSize.Height / 2, 
                        textSize.Width, textSize.Height);
                    
                    // Texte
                    g.DrawString(arret.Nom, font, brush, position.X + 8, position.Y - textSize.Height / 2);
                }
            }
        }

        private void MettreAJourPositionsBus()
        {
            var simulateur = Simulateur.Instance;
            var heureActuelle = simulateur.Horloge.TempsActuel;

            // Effacer les anciens bus
            _busEnMouvement.Clear();

            foreach (var ligne in _simulation.ListeLignes)
            {
                foreach (var evenement in ligne.ListeEvenements)
                {
                    try
                    {
                        // Parser l'heure de départ (format HH:mm)
                        if (!DateTime.TryParse($"2024-01-01 {evenement.Heure}:00", out var heureDepart))
                            continue;

                        // Parser la durée (format HH:mm:ss)
                        var dureeString = evenement.Duree.Split(':');
                        if (dureeString.Length < 3) continue;
                        
                        var dureeMinutes = double.Parse(dureeString[0]) * 60 + 
                                         double.Parse(dureeString[1]) + 
                                         double.Parse(dureeString[2]) / 60.0;
                        
                        var heureArrivee = heureDepart.AddMinutes(dureeMinutes);

                        // Adapter l'heure actuelle au même format pour comparaison
                        var heureActuelleAdaptee = new DateTime(2024, 1, 1, 
                            heureActuelle.Hour, heureActuelle.Minute, heureActuelle.Second);

                        // Vérifier si le bus est en mouvement
                        if (heureActuelleAdaptee >= heureDepart && heureActuelleAdaptee <= heureArrivee)
                        {
                            // Calculer la position interpolée
                            var progression = (heureActuelleAdaptee - heureDepart).TotalMinutes / dureeMinutes;
                            var positionBus = InterpolerPosition(evenement.Depart, evenement.Arrivee, (float)progression);
                            var direction = CalculerDirection(evenement.Depart, evenement.Arrivee);

                            // Éviter les positions invalides
                            if (positionBus == PointF.Empty) continue;

                            var bus = new BusEnMouvement
                            {
                                Immatriculation = evenement.Bus,
                                NomLigne = ligne.Nom,
                                Position = positionBus,
                                Direction = direction,
                                ArretDepart = evenement.Depart,
                                ArretArrivee = evenement.Arrivee
                            };

                            _busEnMouvement[evenement.Bus] = bus;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log l'erreur mais continue avec les autres événements
                        Console.WriteLine($"Erreur lors du traitement de l'événement {evenement.Heure}: {ex.Message}");
                    }
                }
            }
        }

        private PointF InterpolerPosition(Arret depart, Arret arrivee, float progression)
        {
            if (!_positionsArrets.ContainsKey(depart.Id) || !_positionsArrets.ContainsKey(arrivee.Id))
                return PointF.Empty;

            var posDepart = _positionsArrets[depart.Id];
            var posArrivee = _positionsArrets[arrivee.Id];

            var x = posDepart.X + (posArrivee.X - posDepart.X) * progression;
            var y = posDepart.Y + (posArrivee.Y - posDepart.Y) * progression;

            return new PointF(x, y);
        }

        private float CalculerDirection(Arret depart, Arret arrivee)
        {
            if (!_positionsArrets.ContainsKey(depart.Id) || !_positionsArrets.ContainsKey(arrivee.Id))
                return 0;

            var posDepart = _positionsArrets[depart.Id];
            var posArrivee = _positionsArrets[arrivee.Id];

            var dx = posArrivee.X - posDepart.X;
            var dy = posArrivee.Y - posDepart.Y;

            return (float)Math.Atan2(dy, dx);
        }

        private string CalculerHeureAnterieure(string heure, int minutes)
        {
            var heureDateTime = DateTime.Parse($"2024-01-01 {heure}:00");
            var heureAnterieure = heureDateTime.AddMinutes(-minutes);
            return heureAnterieure.ToString("HH:mm");
        }

        private string CalculerHeureArrivee(string heureDepart, string duree)
        {
            var heureDateTime = DateTime.Parse($"2024-01-01 {heureDepart}:00");
            var dureeString = duree.Split(':');
            var dureeMinutes = int.Parse(dureeString[1]) + int.Parse(dureeString[2]) / 60.0;
            var heureArrivee = heureDateTime.AddMinutes(dureeMinutes);
            return heureArrivee.ToString("HH:mm");
        }

        private void TrackBarZoom_ValueChanged(object? sender, EventArgs e)
        {
            CalculerPositions();
            _panelDessin.Invalidate();
        }

        private void CheckBoxAfficherNoms_CheckedChanged(object? sender, EventArgs e)
        {
            _panelDessin.Invalidate();
        }

        private void CheckBoxLignes_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke(new Action(() => _panelDessin.Invalidate()));
        }

        public void MettreAJour()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(MettreAJour));
                return;
            }

            // Mettre à jour les positions des bus
            MettreAJourPositionsBus();

            // Redessiner
            _panelDessin.Invalidate();
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
            if (_positionsArrets != null)
            {
                CalculerPositions();
                _panelDessin?.Invalidate();
            }
        }
    }

    /// <summary>
    /// Représente un bus en mouvement sur la carte
    /// </summary>
    public class BusEnMouvement
    {
        public string Immatriculation { get; set; } = string.Empty;
        public string NomLigne { get; set; } = string.Empty;
        public PointF Position { get; set; }
        public float Direction { get; set; }
        public Arret ArretDepart { get; set; } = null!;
        public Arret ArretArrivee { get; set; } = null!;
    }
}