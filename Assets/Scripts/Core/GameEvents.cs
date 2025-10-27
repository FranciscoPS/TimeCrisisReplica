using System;

public static class GameEvents
{
    public static Action<float> TimerChanged; // segundos restantes
    public static Action GameOver;

    public static Action EnemyKilled; // disparado por cada enemigo al morir (+20s)

    public static Action<int, int> AmmoChanged; // (actual, máximo)
    public static Action<bool> ReloadAlert; // true = mostrar "Reload!"
    public static Action<float, float> PlayerHealthChanged; // (actual, máximo)
}
