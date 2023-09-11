using ColorCodeHackathon2023;

var builder = WebApplication.CreateBuilder(args);
//builder.WebHost.UseStartup<Startup>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.ConfigureAuthorization(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

