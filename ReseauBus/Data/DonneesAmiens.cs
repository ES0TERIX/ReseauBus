using System.Drawing;
using ReseauBus.Core.Models;

namespace ReseauBus.Data
{
    /// <summary>
    /// Contient les données statiques du réseau d'Amiens - Version simplifiée
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
                new Arret(1, "Promenade"),
                new Arret(2, "Centre commercial Nord"),
                new Arret(3, "Romain Rolland"),
                new Arret(4, "Espace santé"),
                new Arret(5, "Pôle d'échanges Nord"),
                new Arret(6, "Berlioz"),
                new Arret(7, "Clémenceau"),
                new Arret(8, "Citadelle François 1er"),
                new Arret(9, "Citadelle Monstrescu"),
                new Arret(10, "Vogel"),
                new Arret(11, "Saint Leu"),
                new Arret(12, "Place de Don"),
                new Arret(13, "Alsace Lorraine"),
                new Arret(14, "Gare du Nord"),
                new Arret(15, "Otages"),
                new Arret(16, "Cirque Jules Verne"),
                new Arret(17, "Collège Saint Martin"),
                new Arret(18, "Delpech"),
                new Arret(19, "Cité - Providence"),
                new Arret(20, "Charassin"),
                new Arret(21, "Aquapôle"),
                new Arret(22, "Pôle des cliniques"),
                new Arret(23, "Georges Beauvais"),
                new Arret(24, "Espagne"),
                new Arret(25, "Grèce"),
                new Arret(26, "IME"),
                new Arret(27, "Centre Commercial Sud"),
                new Arret(28, "Etouvie"),
                new Arret(29, "Martinique"),
                new Arret(30, "Collège Rosa Parks"),
                new Arret(31, "Languedoc"),
                new Arret(32, "Les Coursives"),
                new Arret(33, "La Fontaine"),
                new Arret(34, "Place La Barre"),
                new Arret(35, "Sully"),
                new Arret(36, "Espace Alliance"),
                new Arret(37, "Faubourg de Hem"),
                new Arret(38, "Eglise Saint Firmin"),
                new Arret(39, "Zoo d'Amiens"),
                new Arret(40, "Jean Jaures"),
                new Arret(41, "Saint-Jacques"),
                new Arret(42, "Maison de la Culture"),
                new Arret(43, "Place du Marché"),
                new Arret(44, "Beffroi"),
                new Arret(45, "Dusevel"),
                new Arret(46, "Palais de Justice"),
                new Arret(47, "René Goblet"),
                new Arret(48, "Caserne Dejean"),
                new Arret(49, "Pinceau"),
                new Arret(50, "Lycée de Luzarches"),
                new Arret(51, "Eglise Saint Acheul"),
                new Arret(52, "Mercey"),
                new Arret(53, "Sobo"),
                new Arret(54, "Pont de l'Avre"),
                new Arret(55, "Cité du Château"),
                new Arret(56, "La Rose Rouge"),
                new Arret(57, "Poidevin"),
                new Arret(58, "Mairie de Longueau"),
                new Arret(59, "La Fournche"),
                new Arret(60, "Croix de Fer"),
                new Arret(61, "Centre Commercial Glisy"),
                new Arret(62, "Capitaine Nemo"),
                new Arret(63, "Pôle Jules Verne"),
                new Arret(64, "La Paix"),
                new Arret(65, "Chardin"),
                new Arret(66, "Colvert"),
                new Arret(67, "Nautilus"),
                new Arret(68, "Balzac"),
                new Arret(69, "Stendhal"),
                new Arret(70, "Centre Saint Victor"),
                new Arret(71, "Eloi Morel"),
                new Arret(72, "Parc Saint Pierre"),
                new Arret(73, "Hortillonnages"),
                new Arret(75, "Nicole Fontaine"),
                new Arret(76, "Simone Veil"),
                new Arret(77, "Quatre Chênes"),
                new Arret(78, "Libération"),
                new Arret(79, "Quatre Lemaire"),
                new Arret(80, "Ambroise Paré"),
                new Arret(81, "Rotonde"),
                new Arret(82, "CHU Amiens Picardie"),
                new Arret(83, "Laënnec"),
                new Arret(84, "Résidence du Thil"),
                new Arret(85, "IUT"),
                new Arret(86, "Pôle Licorne"),
                new Arret(87, "Hyppodrome"),
                new Arret(88, "Colbert"),
                new Arret(89, "Lucien Fournier"),
                new Arret(90, "Hôtel de Ville"),
                new Arret(91, "Jacobins"),
                new Arret(92, "Emile Zola"),
                new Arret(93, "Mons"),
                new Arret(94, "3ième DI"),
                new Arret(95, "Hôtel des Impôts"),
                new Arret(96, "Rollin"),
                new Arret(97, "Ormale"),
                new Arret(98, "Görlitz"),
                new Arret(99, "Frédéric Mistral"),
                new Arret(100, "Collège Guy Mareschal"),
                new Arret(101, "IREAM"),
                new Arret(102, "Bel Air"),
                new Arret(103, "Wasse"),
                new Arret(104, "Place de Cagny"),
                new Arret(105, "Latapie"),
                new Arret(106, "Longueau SNCF")
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