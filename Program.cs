using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Database;
using RaceDay.Database;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//Entity Framework Core (uses the same connection string as the SqlClient classes)
builder.Services.AddDbContext<RaceDayContext>(options => options.UseSqlServer(db_connection.connection_string));

//register the database classes so the controllers receive them in their constructors
//(this is also what lets the unit tests swap in fake databases)
builder.Services.AddScoped<users_database>();
builder.Services.AddScoped<events_database>();
builder.Services.AddScoped<categories_database>();
builder.Services.AddScoped<routes_database>();
builder.Services.AddScoped<enrolments_database>();
builder.Services.AddScoped<results_database>();

//session management: remembers who is logged in and their role
//the session data lives on the server, the browser only holds a session cookie
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);   // logged out after 60 minutes of no activity
    options.Cookie.HttpOnly = true;                   // JavaScript cannot read the cookie
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllers();

//Swagger: generates the interactive page where every endpoint can be tested in the browser
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "Register and log in first (POST /api/auth/register, then /api/auth/login). " +
                      "The session cookie is then used for every other request."
    });

    //shows the /// comments of the controllers in Swagger
    string xml_file = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xml_file));
});

var app = builder.Build();

//Code-First: creates RaceDayDB and applies the migrations if needed
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<RaceDayContext>().Database.Migrate();
}

//any unexpected error (e.g. database down) becomes a clean JSON 500 response
app.UseExceptionHandler(error_app => error_app.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    await context.Response.WriteAsJsonAsync(new { message = error?.Message ?? "Unexpected server error." });
}));

//Swagger is only switched on while developing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseSession();            // must be before MapControllers
app.UseAuthorization();
app.MapControllers();

app.Run();