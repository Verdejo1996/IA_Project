using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class LLMInterface : MonoBehaviour
{
    public InputField inputField;
    public TextMeshProUGUI textoRespuesta;
    public Transform contenedorAcciones;
    public GameObject botonAccionPrefab;
    public AccionManager accionManager;
    public LLMConfig config;

    private List<string> historial = new List<string>();

    public void EnviarPregunta()
    {
        string pregunta = inputField.text;
        if (string.IsNullOrWhiteSpace(pregunta)) return;

        GameStateManager.Instance.SetAccionJugador(pregunta);
        GameStateManager.Instance.SincronizarConocimientos();

        string conocimientoTexto = GameStateManager.Instance.GetConocimientosTexto();
        string historialTexto = string.Join("\n", historial);

        string prompt = $"Conocimientos sobre el jugador:\n{conocimientoTexto}\n" +
                        $"Historial:\n{historialTexto}\n" +
                        $"Jugador dijo: {pregunta}\n" +
                        $"Respondé como un asistente útil y realista, si proponés acciones usá el formato [ACTION:accion_id] al final.";

        historial.Add("Jugador: " + pregunta);
        StartCoroutine(EnviarAIA(prompt));
    }

    IEnumerator EnviarAIA(string prompt)
    {
        var request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
        string modeloUsado = config != null ? config.modelo : "mistral";
        string jsonBody = JsonUtility.ToJson(new LLMRequest
        {
            model = modeloUsado,
            prompt = prompt,
            stream = false
        });

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            string parsed = ExtraerRespuesta(responseText);
            textoRespuesta.text = "IA: " + parsed;
            historial.Add("IA: " + parsed);

            List<AccionSugerida> acciones = DetectarAccionesEn(parsed);
            MostrarBotonesDeAccion(acciones);
        }
        else
        {
            Debug.LogError("Error en la solicitud: " + request.error);
        }
    }

    string ExtraerRespuesta(string json)
    {
        var wrapper = JsonUtility.FromJson<RespuestaIA>(json);
        return wrapper.response.Trim();
    }

    List<AccionSugerida> DetectarAccionesEn(string texto)
    {
        List<AccionSugerida> acciones = new List<AccionSugerida>();
        int index = 0;
        while ((index = texto.IndexOf("[ACTION:", index)) != -1)
        {
            int fin = texto.IndexOf("]", index);
            if (fin != -1)
            {
                //string tag = texto.Substring(index + 8, fin - index - 8).Trim();
                string tag = char.ToUpper(texto.Replace("_", " ")[0]) + texto.Replace("_", " ")[1..];
                if (!GameStateManager.Instance.AccionYaEjecutada(tag))
                {
                    acciones.Add(new AccionSugerida(tag, texto));
                }
                index = fin + 1;
            }
            else break;
        }
        return acciones;
    }

    void MostrarBotonesDeAccion(List<AccionSugerida> acciones)
    {
        foreach (Transform child in contenedorAcciones)
            Destroy(child.gameObject);

        foreach (var acc in acciones)
        {
            var boton = Instantiate(botonAccionPrefab, contenedorAcciones);
            var tmp = boton.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = acc.texto;
            else
            {
                var legacy = boton.GetComponentInChildren<Text>();
                if (legacy != null) legacy.text = acc.texto;
            }

            boton.GetComponent<Button>().onClick.AddListener(() =>
            {
                accionManager.EjecutarAccion(acc.accionId);
                textoRespuesta.text += $"\n(Ejecutaste: {acc.texto})";
            });
        }
    }

    [System.Serializable]
    public class RespuestaIA
    {
        public string response;
    }
}

