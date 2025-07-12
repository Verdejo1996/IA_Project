using UnityEngine;

public class AccionManager : MonoBehaviour
{
    public void EjecutarAbrirCajon()
    {
        GameStateManager.Instance.estado.habitacion.cajon = "abierto";
        GameStateManager.Instance.estado.jugador.accion_reciente = "abri� el caj�n";
        GameStateManager.Instance.AgregarConocimiento("El jugador ya abri� el caj�n.");
        Debug.Log("Caj�n abierto.");
    }

    public void EjecutarEncenderLuz()
    {
        GameStateManager.Instance.estado.habitacion.luz = "encendida";
        GameStateManager.Instance.estado.jugador.accion_reciente = "encendi� la luz";
    }
}

