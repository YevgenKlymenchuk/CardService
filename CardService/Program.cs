using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CardService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHealthChecks();
            builder.Services.AddControllers()
                .AddJsonOptions(options => 
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
            builder.Services.AddSingleton<CardService.src.CardActions.Api.Services.CardService>();



            var app = builder.Build();

            app.MapHealthChecks("/health");
            app.MapControllers();
            app.MapGet("/", () => Results.Ok(new 
            { 
                name = "CardService API",
                status = "running"
            }));

            app.Run();
        }
    }
}
