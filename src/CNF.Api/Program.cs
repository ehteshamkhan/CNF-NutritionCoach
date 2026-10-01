using CNF.Api.Data;
using CNF.Api.Services;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddSingleton<CnfDatabase>();

builder.Services.AddScoped<FoodService>();

builder.Services.AddEndpointsApiExplorer();

var app =
    builder.Build();

app.UseCors("AngularDev");


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



