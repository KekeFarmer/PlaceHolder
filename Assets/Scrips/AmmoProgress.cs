using UnityEngine;

/// <summary>
/// Shared ammo state that survives scene loads: bullets loaded in the gun and spare casings
/// picked up. Casings are what the player spends to reload.
/// </summary>
public static class AmmoProgress
{
    public static bool Initialized { get; private set; }
    public static int Loaded { get; private set; }
    public static int Reserve { get; private set; }

    /// <summary>Called by the shooter on Awake. Only the first call (first scene) sets the starting values.</summary>
    public static void Init(int magazineSize, int startingCasings)
    {
        if (Initialized) return;

        Initialized = true;
        Loaded = magazineSize;
        Reserve = startingCasings;
    }

    public static void UseRound()
    {
        if (Loaded > 0) Loaded--;
    }

    public static void AddCasings(int amount)
    {
        Reserve += amount;
    }

    public static bool CanReload(int magazineSize)
    {
        return Loaded < magazineSize && Reserve > 0;
    }

    /// <summary>Moves casings from the reserve into the gun, up to the magazine size.</summary>
    public static void Reload(int magazineSize)
    {
        int moved = Mathf.Max(0, Mathf.Min(magazineSize - Loaded, Reserve));
        Loaded += moved;
        Reserve -= moved;
    }

    public static string HudText(int magazineSize, bool isReloading)
    {
        string text = $"Balas: {Loaded}/{magazineSize}   Casquillos: {Reserve}";

        if (isReloading) return text + "   Recargando...";
        if (Loaded == 0) return text + (Reserve > 0 ? "   Pulsa R para recargar" : "   Sin balas: busca casquillos");
        return text;
    }
}
