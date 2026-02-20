var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.Configure<MeteringOptions>(builder.Configuration.GetSection("Metering"));
builder.Services.AddEbpfMetering();
builder.Services.AddCoreServices();

var app = builder.Build();

app.UseRateLimiting();
app.UseDdosProtection();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();

app.Run();

