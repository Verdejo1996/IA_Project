using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Habitacion
{
    public string puerta = "cerrada";
    public bool llave_encontrada = false;
    public string cajon = "bloqueado";
    public string luz = "apagada";
}

[System.Serializable]
public class Jugador
{
    public string accion_reciente = "observó la habitación";
    public List<string> inventario = new List<string>() { "linterna" };
}

[System.Serializable]
public class GameState
{
    public Habitacion habitacion = new Habitacion();
    public Jugador jugador = new Jugador();
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;
    public GameState estado = new GameState();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public string GetGameStateAsJson()
    {
        return JsonUtility.ToJson(estado, true);
    }

    public void SetAccionJugador(string accion)
    {
        estado.jugador.accion_reciente = accion;
    }

    public void AgregarItem(string item)
    {
        if (!estado.jugador.inventario.Contains(item))
            estado.jugador.inventario.Add(item);
    }

    private Dictionary<string, string> conocimientos = new Dictionary<string, string>();

    public void SetConocimiento(string clave, string descripcion)
    {
        conocimientos[clave] = descripcion;
    }

    public bool TieneConocimiento(string clave)
    {
        return conocimientos.ContainsKey(clave);
    }

    public string GetConocimientosTexto()
    {
        return string.Join("\n", conocimientos.Values);
    }

    public void SincronizarConocimientos()
    {
        SetConocimiento("puerta", $"La puerta está {estado.habitacion.puerta}.");
        SetConocimiento("luz", $"La luz está {estado.habitacion.luz}.");
        SetConocimiento("cajon", $"El cajón está {estado.habitacion.cajon}.");

        for (int i = 0; i < estado.jugador.inventario.Count; i++)
        {
            string item = estado.jugador.inventario[i];
            SetConocimiento($"item_{item}", $"El jugador tiene una {item}.");
        }

        if (!string.IsNullOrEmpty(estado.jugador.accion_reciente))
            SetConocimiento("accion", $"La última acción del jugador fue: {estado.jugador.accion_reciente}.");
    }

    public HashSet<string> accionesEjecutadas = new HashSet<string>();

    public void RegistrarAccionEjecutada(string accionId)
    {
        accionesEjecutadas.Add(accionId);
    }

    public bool AccionYaEjecutada(string accionId)
    {
        return accionesEjecutadas.Contains(accionId);
    }
}