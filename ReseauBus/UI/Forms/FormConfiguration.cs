﻿using ReseauBus.Data;

namespace ReseauBus.UI.Forms
{
    public partial class FormConfiguration : Form
    {
        public List<ConfigurationSimulation> Configurations { get; private set; }

        public FormConfiguration()
        {
            InitializeComponent();
            Configurations = new List<ConfigurationSimulation>();
        }

        private void InitializeComponent()
        {
            this.Text = "Configurer votre simulation";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            CreerControlesConfiguration();
        }

        private void CreerControlesConfiguration()
        {
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));

            var panelHautGauche = CreerPanelConfiguration("Que voulez-vous afficher en haut à gauche?", 0);
            mainPanel.Controls.Add(panelHautGauche, 0, 0);

            var panelHautDroite = CreerPanelConfiguration("Que voulez-vous afficher en haut à droite?", 1);
            mainPanel.Controls.Add(panelHautDroite, 1, 0);

            var panelBasGauche = CreerPanelConfiguration("Que voulez-vous afficher en bas à gauche?", 2);
            mainPanel.Controls.Add(panelBasGauche, 0, 1);

            var panelBasDroite = CreerPanelConfiguration("Que voulez-vous afficher en bas à droite?", 3);
            mainPanel.Controls.Add(panelBasDroite, 1, 1);

            var btnStart = new Button
            {
                Text = "Start",
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.LightCoral,
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };
            btnStart.Click += BtnStart_Click;
            mainPanel.Controls.Add(btnStart, 0, 2);
            mainPanel.SetColumnSpan(btnStart, 2);

            this.Controls.Add(mainPanel);
        }

        private GroupBox CreerPanelConfiguration(string titre, int index)
        {
            var groupBox = new GroupBox
            {
                Text = titre,
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var lblSimulation = new Label
            {
                Text = "Choisir une simulation",
                Location = new Point(10, 20),
                Size = new Size(150, 20)
            };

            var cmbSimulation = new ComboBox
            {
                Name = $"cmbSimulation_{index}",
                Location = new Point(10, 45),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cmbSimulation.Items.Add("Amiens semaine");
            cmbSimulation.Items.Add("Choisir nouveau pour créer une nouvelle simulation");
            cmbSimulation.SelectedIndex = 0;

            var lblDebut = new Label
            {
                Text = "Début simulation",
                Location = new Point(10, 80),
                Size = new Size(100, 20)
            };

            var dtpDebut = new DateTimePicker
            {
                Name = $"dtpDebut_{index}",
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new Point(120, 78),
                Size = new Size(80, 25),
                Value = new DateTime(2024, 1, 1, 6, 0, 0)
            };

            var lblFin = new Label
            {
                Text = "Fin simulation",
                Location = new Point(10, 110),
                Size = new Size(100, 20)
            };

            var dtpFin = new DateTimePicker
            {
                Name = $"dtpFin_{index}",
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "HH:mm",
                ShowUpDown = true,
                Location = new Point(120, 108),
                Size = new Size(80, 25),
                Value = new DateTime(2024, 1, 1, 23, 0, 0)
            };

            var lblType = new Label
            {
                Text = "Choisir le type de visualisation",
                Location = new Point(10, 140),
                Size = new Size(180, 20)
            };

            var cmbType = new ComboBox
            {
                Name = $"cmbType_{index}",
                Location = new Point(10, 165),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbType.Items.AddRange(new[] { "Textuelle", "Aucune" });
            cmbType.SelectedIndex = 0;

            panel.Controls.AddRange(new Control[] {
                lblSimulation, cmbSimulation,
                lblDebut, dtpDebut,
                lblFin, dtpFin,
                lblType, cmbType
            });

            groupBox.Controls.Add(panel);
            return groupBox;
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            CollecterConfigurations();

            if (Configurations.Count > 0)
            {
                var formSimulation = new FormSimulation(Configurations);
                this.Hide();
                formSimulation.ShowDialog();
                this.Show();
            }
            else
            {
                MessageBox.Show("Aucune configuration valide trouvée.", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollecterConfigurations()
        {
            Configurations.Clear();

            for (int i = 0; i < 4; i++)
            {
                var cmbSimulation = this.Controls.Find($"cmbSimulation_{i}", true).FirstOrDefault() as ComboBox;
                var dtpDebut = this.Controls.Find($"dtpDebut_{i}", true).FirstOrDefault() as DateTimePicker;
                var dtpFin = this.Controls.Find($"dtpFin_{i}", true).FirstOrDefault() as DateTimePicker;
                var cmbType = this.Controls.Find($"cmbType_{i}", true).FirstOrDefault() as ComboBox;

                if (cmbSimulation?.SelectedItem != null && 
                    cmbType?.SelectedItem?.ToString() != "Aucune" &&
                    dtpDebut != null && dtpFin != null)
                {
                    var aujourdhui = DateTime.Today;
                    var heureDebut = new DateTime(
                        aujourdhui.Year, 
                        aujourdhui.Month, 
                        aujourdhui.Day,
                        dtpDebut.Value.Hour,
                        dtpDebut.Value.Minute,
                        dtpDebut.Value.Second
                    );
                    
                    var heureFin = new DateTime(
                        aujourdhui.Year, 
                        aujourdhui.Month, 
                        aujourdhui.Day,
                        dtpFin.Value.Hour,
                        dtpFin.Value.Minute,
                        dtpFin.Value.Second
                    );

                    var config = new ConfigurationSimulation
                    {
                        Position = (PositionPanel)i,
                        NomSimulation = cmbSimulation.SelectedItem.ToString() ?? "Amiens semaine",
                        HeureDebut = heureDebut,
                        HeureFin = heureFin,
                        TypeVisualisation = Enum.Parse<TypeVisualisation>(cmbType.SelectedItem.ToString() ?? "Textuelle")
                    };

                    Console.WriteLine($"[CONFIG] Panel {i}: Début={config.HeureDebut:HH:mm}, Fin={config.HeureFin:HH:mm}");
                    Configurations.Add(config);
                }
            }
        }
    }

    public class ConfigurationSimulation
    {
        public PositionPanel Position { get; set; }
        public string NomSimulation { get; set; } = string.Empty;
        public DateTime HeureDebut { get; set; }
        public DateTime HeureFin { get; set; }
        public TypeVisualisation TypeVisualisation { get; set; }
    }

    public enum PositionPanel
    {
        HautGauche = 0,
        HautDroite = 1,
        BasGauche = 2,
        BasDroite = 3
    }

    public enum TypeVisualisation
    {
        Textuelle,
        Aucune
    }
}   