using System.Text.Json.Serialization;

namespace PrintHub.Core.Configuration;

/// <summary>
/// Configuration options for Bambu Cloud integration.
/// </summary>
public class BambuOptions
{
    public const string SectionName = "Bambu";

    /// <summary>
    /// Base URL for the Bambu Cloud API.
    /// </summary>
    [JsonPropertyName("apiBaseUrl")]
    public string ApiBaseUrl { get; set; } = "https://api.bambulab.com";

    /// <summary>
    /// MQTT broker hostname for real-time device updates.
    /// </summary>
    [JsonPropertyName("mqttBroker")]
    public string MqttBroker { get; set; } = "mqtt.bambulab.com";

    /// <summary>
    /// MQTT broker port (default: 8883 for TLS).
    /// </summary>
    [JsonPropertyName("mqttPort")]
    public int MqttPort { get; set; } = 8883;

    /// <summary>
    /// Request timeout in seconds for API calls.
    /// </summary>
    [JsonPropertyName("clientTimeoutSeconds")]
    public int ClientTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Whether to use TLS for MQTT connection.
    /// </summary>
    [JsonPropertyName("mqttUseTls")]
    public bool MqttUseTls { get; set; } = true;
}