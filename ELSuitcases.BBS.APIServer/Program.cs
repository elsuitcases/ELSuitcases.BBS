using ELSuitcases.BBS.Library;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseAuthorization();
//app.UseCors();
//app.UseHealthChecks("/api");
app.UseHttpLogging();
//app.UseHttpsRedirection();
/*app.UseSession(new SessionOptions()
{
    IdleTimeout = Constants.DEFAULT_VALUE_API_CLIENT_TIMEOUT
});*/
app.UseSwagger();
app.UseSwaggerUI();
app.UseWebSockets();
app.UseWelcomePage(new WelcomePageOptions()
{
    Path = "/"
});

app.MapControllers();

app.Run();

