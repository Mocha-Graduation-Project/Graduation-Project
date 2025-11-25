using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/LudiscanConfig")]
public class LudiscanConfig : ScriptableObject
{
    [SerializeField] private string apiBaseUrl = "https://ludiscan.net/api";
    [SerializeField] private string xapiKey = "";
    [SerializeField] private int timeoutSeconds = 10;
    [SerializeField] private bool enableLogging = true;

    public string ApiBaseUrl => apiBaseUrl;
    public string XapiKey => xapiKey;
    public int TimeoutSeconds => timeoutSeconds;
    public bool EnableLogging => enableLogging;
}
