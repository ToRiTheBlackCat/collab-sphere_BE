using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace CollabSphere.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IAmazonS3 _s3Client;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAmazonS3 s3Client)
        {
            _logger = logger;
            _s3Client = s3Client;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("buckets")]
        public async Task<IActionResult> GetBuckets()
        {
            try
            {
                var response = await _s3Client.ListBucketsAsync();

                return Ok(response.Buckets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
