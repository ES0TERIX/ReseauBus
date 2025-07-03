namespace ReseauBus.UI.Panels
{
    /// <summary>
    /// Interface commune pour tous les panneaux de simulation
    /// </summary>
    public interface IPanneauSimulation
    {
        void MettreAJour();
        void Demarrer();
        void Arreter();
    }
}