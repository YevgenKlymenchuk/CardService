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
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() { Title = "Card Actions API", Version = "v1" });
            });
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
            builder.Services.AddSingleton<ICardRepository, CardRepository>();
            builder.Services.AddSingleton<IAllowedActionsEngine, AllowedActionsEngine>();


            var app = builder.Build();
            app.UseExceptionHandler();
            app.UseStatusCodePages();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapGet("/", () => Results.Redirect("/swagger"));
            }
            app.MapHealthChecks("/health");
            app.MapControllers();

            app.Run();
        }
    }
}
