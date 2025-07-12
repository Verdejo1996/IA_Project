using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;

public class LLMInterface : MonoBehaviour
{
    public InputField inputField;       // Campo donde el jugador escribe
    public TextMeshProUGUI textoRespuesta;         // Texto donde se muestra la respuesta
    private List<string> historial = new();

    private void Start()
    {
        textoRespuesta.text = "IA: Hola, ¿en qué puedo ayudarte?";
    }

    public void EnviarPregunta()
    {
        Debug.Log("Se ejecutó EnviarPregunta()");
        string pregunta = inputField.text;
        GameStateManager.Instance.SetAccionJugador(pregunta); // Registrar acción

        historial.Add("Jugador: " + pregunta);

        string estadoJson = GameStateManager.Instance.GetGameStateAsJson();
        string historialTexto = string.Join("\n", historial);

        string conocimientoTexto = GameStateManager.Instance.GetConocimientosTexto();

        string prompt = $"Conocimientos sobre el jugador:\n{conocimientoTexto}\n" +
                        $"Estado actual:\n{estadoJson}\n" +
                        $"Historial:\n{historialTexto}\n" +
                        $"Jugador dijo: {pregunta}\n" +
                        $"Respondé como un asistente útil.";

        StartCoroutine(EnviarAIA(prompt));
    }

    IEnumerator EnviarAIA(string prompt)
    {
        // Usamos una clase para construir el JSON de forma segura
        LLMRequest requestData = new LLMRequest();
        requestData.prompt = prompt;

        string json = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest("http://localhost:11434/api/generate", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            string parsed = ExtraerRespuesta(responseText);
            historial.Add("IA: " + parsed);
            textoRespuesta.text = "IA: " + parsed;
        }
        else
        {
            textoRespuesta.text = "Error al conectar con la IA.";
            Debug.LogError("Código HTTP: " + request.responseCode + "\n" + request.downloadHandler.text);
        }
    }

    // Extraemos el contenido del campo "response"
    private string ExtraerRespuesta(string rawJson)
    {
        int responseIndex = rawJson.IndexOf("\"response\":\"") + 12;
        int endIndex = rawJson.IndexOf("\"", responseIndex);
        if (responseIndex < 12 || endIndex < 0 || endIndex <= responseIndex)
            return "No se pudo leer la respuesta.";

        string result = rawJson.Substring(responseIndex, endIndex - responseIndex);
        result = result.Replace("\\n", "\n").Replace("\\\"", "\"");
        return result;
    }
}
