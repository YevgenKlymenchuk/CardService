using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace CardService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHealthChecks();

            builder.Services.AddControllers();

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
