using DotNetGarbage.Services;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        var app = CreateApp(args);

        var mem = app.MapGroup("/mem");
        mem.MapGet("/allocate", (IHeavyService svc) =>
        {
            svc.Allocate();
            return Results.Ok("done");
        });

        app.Run();
    }

    public static WebApplication CreateApp(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.AddScoped<IHeavyService, HeavyService>();

        return builder.Build();
    }
}

