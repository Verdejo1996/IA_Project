using UnityEngine;

public class AccionManager : MonoBehaviour
{
    public void EjecutarAccion(string accionId)
    {
        switch (accionId)
        {
            case "abrir_cajon":
                GameStateManager.Instance.estado.habitacion.cajon = "abierto";
                GameStateManager.Instance.estado.jugador.accion_reciente = "abrió el cajón";
                GameStateManager.Instance.RegistrarAccionEjecutada("abrir_cajon");
                break;

            case "encender_luz":
                GameStateManager.Instance.estado.habitacion.luz = "encendida";
                GameStateManager.Instance.estado.jugador.accion_reciente = "encendió la luz";
                GameStateManager.Instance.RegistrarAccionEjecutada("encender_luz");
                break;

                // Agregá más acciones aquí...
        }

        GameStateManager.Instance.SincronizarConocimientos();
        Debug.Log("Acción ejecutada: " + accionId);
    }
}