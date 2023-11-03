using Tradoctor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IAmazonService, AmazonService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

var configuration = app.Services.GetRequiredService<IConfiguration>();
var awsAccessKey = configuration["Credentials:AWSAccessKey"];
var awsSecretKey = configuration["Credentials:AWSSecretKey"];


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
