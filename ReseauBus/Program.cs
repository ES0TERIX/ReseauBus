using ReseauBus.UI.Forms;

namespace ReseauBus
{
    internal static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application de simulation de bus
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormConfiguration());
        }
    }
}