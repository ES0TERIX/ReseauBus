using System.Drawing;
using ReseauBus.Core.Models;

namespace ReseauBus.Data
{
    /// <summary>
    /// Contient les données statiques du réseau d'Amiens - Conforme UML
    /// </summary>
    public static class DonneesAmiens
    {
        private static List<Arret>? _arrets;
        private static List<LigneBus>? _lignes;

        /// <summary>
        /// Retourne tous les arrêts d'Amiens
        /// </summary>
        public static List<Arret> ObtenirArrets()
        {
            if (_arrets == null)
            {
                InitialiserArrets();
            }
            return _arrets!;
        }

        /// <summary>
        /// Retourne toutes les lignes d'Amiens
        /// </summary>
        public static List<LigneBus> ObtenirLignes()
        {
            if (_lignes == null)
            {
                InitialiserLignes();
            }
            return _lignes!;
        }

        /// <summary>
        /// Trouve un arrêt par son ID
        /// </summary>
        public static Arret? TrouverArret(int id)
        {
            return ObtenirArrets().FirstOrDefault(a => a.Id == id);
        }

        private static void InitialiserArrets()
        {
            _arrets = new List<Arret>
            {
                new Arret(1, "Promenade", 24f, 0f),
                new Arret(2, "Centre commercial Nord", 22.5f, 1f),
                new Arret(3, "Romain Rolland", 22f, 2.5f),
                new Arret(4, "Espace santé", 22f, 3.5f),
                new Arret(5, "Pôle d'échanges Nord", 22f, 4.5f),
                new Arret(6, "Berlioz", 22f, 5.5f),
                new Arret(7, "Clémenceau", 21.5f, 6.5f),
                new Arret(8, "Citadelle François 1er", 21f, 8f),
                new Arret(9, "Citadelle Monstrescu", 20.5f, 9f),
                new Arret(10, "Vogel", 18.5f, 11.5f),
                new Arret(11, "Saint Leu", 20.5f, 12f),
                new Arret(12, "Place de Don", 22f, 13f),
                new Arret(13, "Alsace Lorraine", 24.5f, 14.5f),
                new Arret(14, "Gare du Nord", 23.5f, 17f),
                new Arret(15, "Otages", 21f, 18f),
                new Arret(16, "Cirque Jules Verne", 19f, 18f),
                new Arret(17, "Collège Saint Martin", 18f, 20.5f),
                new Arret(18, "Delpech", 17f, 22f),
                new Arret(19, "Cité - Providence", 19f, 23f),
                new Arret(20, "Charassin", 20.5f, 24.5f),
                new Arret(21, "Aquapôle", 20f, 26f),
                new Arret(22, "Pôle des cliniques", 19f, 25.5f),
                new Arret(23, "Georges Beauvais", 15.5f, 25f),
                new Arret(24, "Espagne", 13.5f, 26f),
                new Arret(25, "Grèce", 12.5f, 27f),
                new Arret(26, "IME", 10.5f, 29f),
                new Arret(27, "Centre Commercial Sud", 10f, 30.5f),
                new Arret(28, "Etouvie", 0f, 3.5f),
                new Arret(29, "Martinique", 1f, 2.5f),
                new Arret(30, "Collège Rosa Parks", 2f, 2f),
                new Arret(31, "Languedoc", 4f, 2.5f),
                new Arret(32, "Les Coursives", 3.5f, 3f),
                new Arret(33, "La Fontaine", 3.5f, 4.5f),
                new Arret(34, "Place La Barre", 4.5f, 5f),
                new Arret(35, "Sully", 6.5f, 6f),
                new Arret(36, "Espace Alliance", 9f, 7f),
                new Arret(37, "Faubourg de Hem", 10f, 7.5f),
                new Arret(38, "Eglise Saint Firmin", 11.5f, 8.5f),
                new Arret(39, "Zoo d'Amiens", 13f, 9.5f),
                new Arret(40, "Jean Jaures", 14.5f, 11f),
                new Arret(41, "Saint-Jacques", 15.5f, 12.5f),
                new Arret(42, "Maison de la Culture", 17f, 13.5f),
                new Arret(43, "Place du Marché", 17.5f, 13f),
                new Arret(44, "Beffroi", 19f, 13f),
                new Arret(45, "Dusevel", 20f, 14f),
                new Arret(46, "Palais de Justice", 21f, 15f),
                new Arret(47, "René Goblet", 21.5f, 16.5f),
                new Arret(48, "Caserne Dejean", 25.5f, 18f),
                new Arret(49, "Pinceau", 27f, 19f),
                new Arret(50, "Lycée de Luzarches", 28f, 20f),
                new Arret(51, "Eglise Saint Acheul", 29f, 21f),
                new Arret(52, "Mercey", 30.5f, 22f),
                new Arret(53, "Sobo", 31.5f, 22.5f),
                new Arret(54, "Pont de l'Avre", 33f, 23.5f),
                new Arret(55, "Cité du Château", 35f, 24.5f),
                new Arret(56, "La Rose Rouge", 35.5f, 25.5f),
                new Arret(57, "Poidevin", 36.5f, 26f),
                new Arret(58, "Mairie de Longueau", 37f, 26.5f),
                new Arret(59, "La Fournche", 38f, 27f),
                new Arret(60, "Croix de Fer", 40.5f, 27.5f),
                new Arret(61, "Centre Commercial Glisy", 42f, 29f),
                new Arret(62, "Capitaine Nemo", 43.5f, 30f),
                new Arret(63, "Pôle Jules Verne", 45f, 30.5f),
                new Arret(64, "La Paix", 19.5f, 4.5f),
                new Arret(65, "Chardin", 21f, 4.5f),
                new Arret(66, "Colvert", 24.5f, 4.5f),
                new Arret(67, "Nautilus", 26f, 5f),
                new Arret(68, "Balzac", 27.5f, 5f),
                new Arret(69, "Stendhal", 28f, 6.5f),
                new Arret(70, "Centre Saint Victor", 28f, 7.5f),
                new Arret(71, "Eloi Morel", 27f, 9f),
                new Arret(72, "Parc Saint Pierre", 26f, 11f),
                new Arret(73, "Hortillonnages", 25f, 13f),
                new Arret(75, "Nicole Fontaine", 16f, 16.5f),
                new Arret(76, "Simone Veil", 14.5f, 15f),
                new Arret(77, "Quatre Chênes", 12.5f, 18f),
                new Arret(78, "Libération", 11.5f, 19.5f),
                new Arret(79, "Quatre Lemaire", 11.5f, 20.5f),
                new Arret(80, "Ambroise Paré", 7f, 23f),
                new Arret(81, "Rotonde", 5.5f, 23.5f),
                new Arret(82, "CHU Amiens Picardie", 3.5f, 24f),
                new Arret(83, "Laënnec", 3.5f, 26f),
                new Arret(84, "Résidence du Thil", 5f, 26f),
                new Arret(85, "IUT", 7f, 26.5f),
                new Arret(86, "Pôle Licorne", 7.5f, 11f),
                new Arret(87, "Hyppodrome", 9.5f, 12f),
                new Arret(88, "Colbert", 11.5f, 14f),
                new Arret(89, "Lucien Fournier", 12.5f, 14f),
                new Arret(90, "Hôtel de Ville", 17.5f, 15f),
                new Arret(91, "Jacobins", 19.5f, 15.5f),
                new Arret(92, "Emile Zola", 21f, 16.5f),
                new Arret(93, "Mons", 27.5f, 21.5f),
                new Arret(94, "3ième DI", 26f, 22f),
                new Arret(95, "Hôtel des Impôts", 25.5f, 23f),
                new Arret(96, "Rollin", 26f, 24f),
                new Arret(97, "Ormale", 27f, 24.5f),
                new Arret(98, "Görlitz", 28f, 26f),
                new Arret(99, "Frédéric Mistral", 28.5f, 27f),
                new Arret(100, "Collège Guy Mareschal", 29.5f, 27f),
                new Arret(101, "IREAM", 31f, 26.5f),
                new Arret(102, "Bel Air", 31.5f, 28f),
                new Arret(103, "Wasse", 32f, 29f),
                new Arret(104, "Place de Cagny", 33f, 30.5f),
                new Arret(105, "Latapie", 33.5f, 29.5f),
                new Arret(106, "Longueau SNCF", 36f, 29.5f)
            };
        }

        private static void InitialiserLignes()
        {
            var arrets = ObtenirArrets();
            _lignes = new List<LigneBus>();

            // Ligne Nemo 1 (Rouge)
            var nemo1 = new LigneBus("Nemo 1", Color.Red);
            var idsNemo1 = new[] { 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 14, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63 };
            foreach (var id in idsNemo1)
            {
                var arret = TrouverArret(id);
                if (arret != null) nemo1.AjouterArret(arret);
            }
            _lignes.Add(nemo1);

            // Ligne Nemo 2 (Bleu)
            var nemo2 = new LigneBus("Nemo 2", Color.Blue);
            var idsNemo2 = new[] { 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 13, 14, 15, 16, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85 };
            foreach (var id in idsNemo2)
            {
                var arret = TrouverArret(id);
                if (arret != null) nemo2.AjouterArret(arret);
            }
            _lignes.Add(nemo2);

            // Ligne Nemo 3 (Vert)
            var nemo3 = new LigneBus("Nemo 3", Color.Green);
            var idsNemo3 = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27 };
            foreach (var id in idsNemo3)
            {
                var arret = TrouverArret(id);
                if (arret != null) nemo3.AjouterArret(arret);
            }
            _lignes.Add(nemo3);

            // Ligne Nemo 4 (Orange)
            var nemo4 = new LigneBus("Nemo 4", Color.Orange);
            var idsNemo4 = new[] { 86, 87, 88, 89, 76, 75, 90, 91, 92, 14, 48, 49, 50, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106 };
            foreach (var id in idsNemo4)
            {
                var arret = TrouverArret(id);
                if (arret != null) nemo4.AjouterArret(arret);
            }
            _lignes.Add(nemo4);
        }
    }
}