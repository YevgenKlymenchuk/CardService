using System.Text.Json;
using System.Text.Json.Serialization;
using CardService.CardActions.Api.ErrorHandling;
using CardService.CardActions.Api.Services;

namespace CardService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    if (context.ProblemDetails.Instance is null)
                        context.ProblemDetails.Instance = context.HttpContext.Request.Path;

                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                };
            });
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddHealthChecks();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
            builder.Services.AddSingleton<ICardRepository, CardRepository>();
            builder.Services.AddSingleton<IAllowedActionsEngine, AllowedActionsEngine>();



            var app = builder.Build();
            app.UseExceptionHandler();
            app.UseStatusCodePages();
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
