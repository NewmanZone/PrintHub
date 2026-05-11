using FluentAssertions;
using PrintHub.Core.Configuration;

namespace PrintHub.Tests;

public class BambuOptionsTests
{
    [Fact]
    public void BambuOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new BambuOptions();

        // Assert
        options.ApiBaseUrl.Should().Be("https://api.bambulab.com");
        options.MqttBroker.Should().Be("mqtt.bambulab.com");
        options.MqttPort.Should().Be(8883);
        options.ClientTimeoutSeconds.Should().Be(30);
        options.MqttUseTls.Should().BeTrue();
    }

    [Fact]
    public void BambuOptions_ShouldAllowCustomValues()
    {
        // Arrange & Act
        var options = new BambuOptions
        {
            ApiBaseUrl = "https://custom.bambu.com",
            MqttBroker = "custom.mqtt.bambu.com",
            MqttPort = 1883,
            ClientTimeoutSeconds = 60,
            MqttUseTls = false
        };

        // Assert
        options.ApiBaseUrl.Should().Be("https://custom.bambu.com");
        options.MqttBroker.Should().Be("custom.mqtt.bambu.com");
        options.MqttPort.Should().Be(1883);
        options.ClientTimeoutSeconds.Should().Be(60);
        options.MqttUseTls.Should().BeFalse();
    }
}
