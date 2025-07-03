using ReseauBus.Core.Interfaces;

namespace ReseauBus
{
    /// <summary>
    /// Classe de test pour observer les simulations - Implémente IObserver
    /// </summary>
    public class ObservateurTest : IObserver
    {
        public string Nom { get; set; }
        
        public ObservateurTest(string nom)
        {
            Nom = nom;
        }
        
        public void Actualiser()
        {
            Console.WriteLine($"[{Nom}] Notification reçue - Simulation mise à jour !");
        }
    }
}