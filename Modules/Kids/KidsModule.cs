using KidsMealApi.DataAccess;
using KidsMealApi.DataAccess.Models;
using KidsMealApi.Modules.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace KidsMealApi.Modules.Kids
{
    public class KidsModule : IModule
    {
        public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/kid", [Authorize] (KidsMealDbContext db) => {
                return Results.Ok(db.Kids.ToList()); 
            });

            endpoints.MapGet("/kid/{kidID}", [Authorize] async (Guid kidID, KidsMealDbContext db) => {
                return await db.Kids.FindAsync(kidID) is Kid kid ? Results.Ok(kid) : Results.NotFound();
            });

            endpoints.MapPost("/kid", [Authorize] async (Kid kid, KidsMealDbContext db) => {
                await db.Kids.AddAsync(kid);
                await db.SaveChangesAsync();
                return Results.Created($"/kid/{kid.Id}", kid);
            });

            endpoints.MapPut("/kid", [Authorize] async (Kid kid, KidsMealDbContext db) => {
                if (kid == null)
                    return Results.BadRequest();
                
                var existingKid = await db.Kids.FindAsync(kid.Id);
                if (existingKid == null)
                    return Results.NotFound();

                db.Kids.Update(kid);
                await db.SaveChangesAsync();
                return Results.Ok();
            });

            endpoints.MapDelete("/kid/{kidID}", [Authorize] async(Guid kidID, KidsMealDbContext db) => {
                
                var existingKid =  await db.Kids.FindAsync(kidID);
                if (existingKid == null)
                    return Results.NotFound();
                
                db.Kids.Remove(existingKid);
                await db.SaveChangesAsync();
                return Results.NoContent();
            });
            
            return endpoints;
        }

        public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}