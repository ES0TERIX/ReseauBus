using ReseauBus.UI.Forms;

namespace ReseauBus
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormConfiguration());
        }
    }
}