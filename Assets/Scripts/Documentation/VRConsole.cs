using TMPro;
using UnityEngine;

public class VRConsole : MonoBehaviour
{
    public TextMeshProUGUI debugText;
    private string myLog = "";
    private readonly int maxLines = 15;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Colour code errors
        string color = (type == LogType.Error || type == LogType.Exception ? "red" : "white");

        string newEntry = $"<color={color}>{logString}</color>\n";
        myLog += newEntry;

        // Keep to max lines
        string[] lines = myLog.Split('\n');
        if (lines.Length > maxLines)
        {
            myLog = string.Join("\n", lines, lines.Length - maxLines, maxLines);
        }

        debugText.text = myLog;
    }

}
