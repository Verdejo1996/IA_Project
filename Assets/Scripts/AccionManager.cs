using UnityEngine;

public class AccionManager : MonoBehaviour
{
    public void EjecutarAccion(string accionId)
    {
        switch (accionId)
        {
            case "abrir_cajon":
                GameStateManager.Instance.estado.habitacion.cajon = "abierto";
                GameStateManager.Instance.estado.jugador.accion_reciente = "abri� el caj�n";
                GameStateManager.Instance.RegistrarAccionEjecutada("abrir_cajon");
                break;

            case "encender_luz":
                GameStateManager.Instance.estado.habitacion.luz = "encendida";
                GameStateManager.Instance.estado.jugador.accion_reciente = "encendi� la luz";
                GameStateManager.Instance.RegistrarAccionEjecutada("encender_luz");
                break;

                // Agreg� m�s acciones aqu�...
        }

        GameStateManager.Instance.SincronizarConocimientos();
        Debug.Log("Acci�n ejecutada: " + accionId);
    }
}