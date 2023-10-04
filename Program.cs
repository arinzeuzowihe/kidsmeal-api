using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.DataAccess.Utilities;
using KidsMealApi.Modules.Extensions;
using KidsMealApi.Modules.Interfaces;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.NameTranslation;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static void Main(string[] args)
    {
        //This is a varition of domain-driven that is referred to as "Module-Driven" approach: 
        //https://timdeschryver.dev/blog/maybe-its-time-to-rethink-our-project-structure-with-dot-net-6#a-domain-driven-api

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.RegisterModules(builder.Configuration); //Automatically discover and register all modules
        //builder.Services.AddDbContext<KidsMealDbContext>(opt => opt.UseInMemoryDatabase("KidMeals"));
        builder.Services.AddDbContext<KidsMealDbContext>(opt => opt.UseNpgsql(builder.Configuration["DbConnectionString"] ?? "")
                                                                   .UseSnakeCaseNamingConvention());
        NpgsqlConnection.GlobalTypeMapper.MapEnum<MealType>("mealtype", new NpgsqlNullNameTranslator());
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        var corsPolicyName = "corsPolicy";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: corsPolicyName,
                            policy =>
                            {
                                policy.WithOrigins("http://localhost:3000")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowAnyOrigin();
                            }
            );
        });

        var app = builder.Build();
        app.MapEndpoints();

        app.UseCors(corsPolicyName);

        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            //Seed the database
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KidsMealDbContext>();
            SampleDataGenerator.SeedDatabase(dbContext);
        }
        else
        {
            //Global Exception Handling
            app.UseExceptionHandler(exceptionHandlerApp =>
            {
                exceptionHandlerApp.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = Text.Plain;

                    await context.Response.WriteAsync("An exception was thrown");

                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                    //Generate various responses based on error generated by the original request
                    if (exceptionHandlerPathFeature?.Error is FileNotFoundException)
                    {
                        await context.Response.WriteAsync("The file was not found");
                    }

                    if (exceptionHandlerPathFeature?.Path == "/")
                    {
                        await context.Response.WriteAsync("Page: Home");
                    }

                });
            });
        }


        app.Run();
    }
}