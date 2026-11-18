using System;
using System.Collections.Generic;

// Permite que janelas sobrepostas (card de elemento, Legenda) tratem o Esc
// antes do PauseManager. O handler retorna true se consumiu a tecla.
public static class OverlayEscape
{
    private static readonly List<Func<bool>> handlers =
        new List<Func<bool>>();

    public static void Register(Func<bool> handler)
    {
        if (!handlers.Contains(handler))
            handlers.Add(handler);
    }

    public static void Unregister(Func<bool> handler)
    {
        handlers.Remove(handler);
    }

    // Chama o handler mais recente primeiro
    public static bool TryConsume()
    {
        for (int i = handlers.Count - 1; i >= 0; i--)
        {
            if (handlers[i]())
                return true;
        }

        return false;
    }
}
