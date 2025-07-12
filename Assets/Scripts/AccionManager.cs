using UnityEngine;

public class AccionManager : MonoBehaviour
{
    public void EjecutarAbrirCajon()
    {
        GameStateManager.Instance.estado.habitacion.cajon = "abierto";
        GameStateManager.Instance.estado.jugador.accion_reciente = "abrió el cajón";
        GameStateManager.Instance.AgregarConocimiento("El jugador ya abrió el cajón.");
        Debug.Log("Cajón abierto.");
    }

    public void EjecutarEncenderLuz()
    {
        GameStateManager.Instance.estado.habitacion.luz = "encendida";
        GameStateManager.Instance.estado.jugador.accion_reciente = "encendió la luz";
    }
}

