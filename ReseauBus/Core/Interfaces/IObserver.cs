namespace ReseauBus.Core.Interfaces
{
    public interface IObserver
    {
        void Actualiser();
    }

    public interface IInterfaceUtilisateur : IObserver
    {
        void Actualiser();
    }
}