using System;

public static class GameEvents
{
    public static Action<float> TimerChanged; // segundos restantes
    public static Action GameOver;

    public static Action<bool> EnemyKilled; // disparado por cada enemigo al morir, bool = wasHeadshot

    public static Action<int, int> AmmoChanged; // (actual, máximo)
    public static Action<bool> ReloadAlert; // true = mostrar "Reload!" cuando sin balas
    public static Action<bool> ReloadingStatus; // true = mostrar "Reloading..." durante recarga
    public static Action<float, float> PlayerHealthChanged; // (actual, máximo)
    
    // Evento para notificar cuando se está viajando entre zonas
    public static Action<bool> TravellingBetweenZones; // true = viajando, false = en zona
}
