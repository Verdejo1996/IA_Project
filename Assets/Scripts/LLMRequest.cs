[System.Serializable]
public class LLMRequest
{
    public string model = "mistral";
    public string prompt;
    public bool stream = false;
}

