using System.Text.Json;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using WeatherVideoApp.Server.Models;

namespace WeatherVideoApp.Server.Services;

public class GreeterService : IssLocationService.IssLocationServiceBase
{   
    private ILogger<GreeterService> _logger;
    private static readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://api.open-notify.org/"), };
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Greeter Service Started");
    }

    
    async Task<IssNowResponse?> GetAsync()
    {   
        _logger.LogInformation("getasync url Started");
        
        using HttpResponseMessage response = await _httpClient.GetAsync("iss-now.json");
    
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var issData = JsonSerializer.Deserialize<IssNowResponse>(jsonResponse);

        if (issData is null)
        {
            _logger.LogWarning("Deserializacija ISS podatkov je vrnila null");
            return issData;
        }

        _logger.LogInformation("ISS lokacija: lat={Lat}, lon={Lon}", issData.IssPosition.Latitude, issData.IssPosition.Longitude);
    
        
        return issData;
        
    }
    public override async Task SubscribeToIssLocation(IssLocationUpdateRequest request, IServerStreamWriter<IssLocationUpdateReply> responseStream, ServerCallContext context)
    {   
        _logger.LogInformation("{Name} Has Subscribed to IssLocationUpdate", request.Name);

        while (!context.CancellationToken.IsCancellationRequested)
        {
            try
            {
                var issData = await GetAsync();

                if (issData is not null)
                {
                    var update = new IssLocationUpdateReply
                    {
                        Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(issData.Timestamp)),
                        Message = issData.Message,
                        Latitude = float.Parse(issData.IssPosition.Latitude,
                            System.Globalization.CultureInfo.InvariantCulture),
                        Longitude = float.Parse(issData.IssPosition.Longitude,
                            System.Globalization.CultureInfo.InvariantCulture),
                    };
                    await responseStream.WriteAsync(update);
                }

                

                await Task.Delay(1000, context.CancellationToken);
            }
            catch (OperationCanceledException)
            {   
                _logger.LogInformation("Client closed the connection");
            }


        }
    }
}
