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
    public List<string> conocimientos = new();

    public GameState estado = new GameState();

    private void Awake()
    {
        Instance = this;
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
    public void AgregarConocimiento(string info)
    {
        if (!conocimientos.Contains(info))
            conocimientos.Add(info);
    }
    public string GetConocimientosTexto()
    {
        return string.Join("\n", conocimientos);
    }
}

