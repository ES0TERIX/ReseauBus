namespace ReseauBus.Core.Interfaces
{
    /// <summary>
    /// Interface Observer - Conforme au diagramme UML
    /// </summary>
    public interface IObserver
    {
        void Actualiser();
    }

    /// <summary>
    /// Interface pour les utilisateurs de l'interface
    /// </summary>
    public interface IInterfaceUtilisateur : IObserver
    {
        void Actualiser();
    }
}