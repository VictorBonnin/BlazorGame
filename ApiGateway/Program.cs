var builder = WebApplication.CreateBuilder(args);

// 1. Ajouter YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// 2. Ajouter CORS (Pour que le Blazor puisse appeler la Gateway)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5000") // L'adresse de ton Blazor
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// 3. Activer le proxy
app.MapReverseProxy();

app.Run();