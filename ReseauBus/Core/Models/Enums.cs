namespace ReseauBus.Core.Models
{
    /// <summary>
    /// Statuts possibles d'un bus - Version consolidée
    /// </summary>
    public enum StatutBus
    {
        AArret,
        EnCirculation
    }

    /// <summary>
    /// Types d'événements détaillés pour les notifications
    /// </summary>
    public enum TypeEvenementBus
    {
        AArret,     // Bus à l'arrêt (temps d'arrêt en cours)
        EnRoute,    // Bus en circulation entre deux arrêts
        Arrivee,    // Bus vient d'arriver à un arrêt
        Depart      // Bus vient de partir d'un arrêt
    }
}