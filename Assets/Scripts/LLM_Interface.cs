using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LLMInterface : MonoBehaviour
{
    public InputField inputField;
    public TextMeshProUGUI textoRespuesta;
    public Transform contenedorAcciones;
    public GameObject botonAccionPrefab;
    public AccionManager accionManager;
    public LLMConfig config;

    private List<string> historial = new List<string>();

    private void Start()
    {
        GameStateManager.Instance.CargarConocimientosIniciales();
    }
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
                        $"Respondé como si estuvieras en una conversación real. Que tu respuesta sea breve. No expliques todo. Si proponés acciones usá el formato [ACTION:accion_id] al final.";

        historial.Add("Jugador: " + pregunta);
        Debug.Log("Mensaje enviado");
        StartCoroutine(EnviarAIA(prompt));
    }

    IEnumerator EnviarAIA(string prompt)
    {
        /*        var request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
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
                Debug.Log("Mensaje leido");
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
                }*/

        string modeloUsado = config != null ? config.modelo : "mistral";

        string jsonBody = JsonUtility.ToJson(new LLMRequest
        {
            model = modeloUsado,
            prompt = prompt,
            stream = true
        });

        var request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Activamos streaming
        request.SendWebRequest();

        string respuestaAcumulada = "";
        string respuestaTotal = "";
        textoRespuesta.text = "IA: ";

        while (!request.isDone)
        {
            if (request.downloadHandler != null)
            {
                string data = request.downloadHandler.text;

                // Solo leer lo nuevo
                if (data.Length > respuestaTotal.Length)
                {
                    string nuevo = data.Substring(respuestaTotal.Length);
                    respuestaTotal = data;

                    // Separar por líneas
                    using (StringReader reader = new StringReader(nuevo))
                    {
                        string linea;
                        while ((linea = reader.ReadLine()) != null)
                        {
                            if (!string.IsNullOrWhiteSpace(linea))
                            {
                                try
                                {
                                    var frag = JsonUtility.FromJson<RespuestaIA>(linea);
                                    respuestaAcumulada += frag.response;
                                    textoRespuesta.text += frag.response;                                    
                                }
                                catch { }
                            }
                        }
                    }
                }
            }

            yield return null;
        }

        historial.Add("IA: " + respuestaAcumulada);

        List<AccionSugerida> acciones = DetectarAccionesEn(respuestaAcumulada);
        MostrarBotonesDeAccion(acciones);
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
                string tag = texto.Substring(index + 8, fin - index - 8).Trim();
                string textoAccion = char.ToUpper(tag.Replace("_", " ")[0]) + tag.Replace("_", " ").Substring(1);
                if (!GameStateManager.Instance.AccionYaEjecutada(tag))
                {
                    acciones.Add(new AccionSugerida(tag, textoAccion));
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

