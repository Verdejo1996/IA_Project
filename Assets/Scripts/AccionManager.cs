using UnityEngine;

public class AccionManager : MonoBehaviour
{
    public void EjecutarAccion(string accionId)
    {
        switch (accionId)
        {
            case "explorar":
                GameStateManager.Instance.estado.jugador.accion_reciente = "exploró la habitación";
                GameStateManager.Instance.AgregarItem("llave_cajon");
                GameStateManager.Instance.RegistrarAccionEjecutada("explorar");
                break;

            case "usar_llave_cajon":
                if (GameStateManager.Instance.estado.jugador.inventario.Contains("llave_cajon"))
                {
                    GameStateManager.Instance.estado.habitacion.cajon = "desbloqueado";
                    GameStateManager.Instance.estado.jugador.accion_reciente = "desbloqueó el cajón";
                    GameStateManager.Instance.RegistrarAccionEjecutada("usar_llave_cajon");
                }
                break;

            case "abrir_cajon":
                if (GameStateManager.Instance.estado.habitacion.cajon == "desbloqueado")
                {
                    GameStateManager.Instance.estado.jugador.accion_reciente = "abrió el cajón";
                    GameStateManager.Instance.AgregarItem("llave_puerta");
                    GameStateManager.Instance.RegistrarAccionEjecutada("abrir_cajon");
                }
                break;

            case "usar_llave_puerta":
                if (GameStateManager.Instance.estado.jugador.inventario.Contains("llave_puerta"))
                {
                    GameStateManager.Instance.estado.habitacion.puerta = "abierta";
                    GameStateManager.Instance.estado.jugador.accion_reciente = "usó la llave para abrir la puerta";
                    GameStateManager.Instance.RegistrarAccionEjecutada("usar_llave_puerta");
                }
                break;
        }

        GameStateManager.Instance.SincronizarConocimientos();
        Debug.Log("Acción ejecutada: " + accionId);
    }
}