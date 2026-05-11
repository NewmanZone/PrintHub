namespace PrintHub.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Bambu Cloud integration.
/// </summary>
public class BambuOptions
{
    public const string SectionName = "Bambu";

    /// <summary>
    /// Base URL for the Bambu Cloud API. Defaults to the global endpoint.
    /// </summary>
    public string ApiBaseUrl { get; set; } = "https://api.bambulab.com";

    /// <summary>
    /// MQTT broker host for real-time printer telemetry.
    /// </summary>
    public string MqttBrokerHost { get; set; } = "us.mqtt.bambulab.com";

    /// <summary>
    /// MQTT broker port (TLS).
    /// </summary>
    public int MqttBrokerPort { get; set; } = 8883;

    /// <summary>
    /// Application identifier used when registering as an integration partner.
    /// Optional until official partner program access is granted.
    /// </summary>
    public string? AppKey { get; set; }

    /// <summary>
    /// Application secret for partner program authentication.
    /// Optional until official partner program access is granted.
    /// </summary>
    public string? AppSecret { get; set; }

    /// <summary>
    /// Maximum number of status poll requests per minute per device.
    /// Bambu Cloud enforces ~10 req/min; we default slightly lower.
    /// </summary>
    public int MaxStatusPollsPerMinute { get; set; } = 8;

    /// <summary>
    /// Timeout for HTTP requests to Bambu Cloud (seconds).
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient failures (429, 502, 503).
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Delay between retries (seconds).
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 2;
}
