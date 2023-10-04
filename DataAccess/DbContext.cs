using Microsoft.EntityFrameworkCore;
using KidsMealApi.DataAccess.Models;

namespace KidsMealApi.DataAccess
{
    public class KidsMealDbContext : DbContext 
    {
        public KidsMealDbContext(DbContextOptions<KidsMealDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Kid> Kids => Set<Kid>();
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
        public DbSet<MealPreference> MealPreferences => Set<MealPreference>();
        public DbSet<MealHistory> MealHistories => Set<MealHistory>();
        public DbSet<MealSuggestion> MealSuggestions => Set<MealSuggestion>();
        public DbSet<RefreshTokenHistory> RefreshTokenHistories => Set<RefreshTokenHistory>();
        public DbSet<KidAssociation> KidAssociations => Set<KidAssociation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<MealType>();
            
            modelBuilder.Entity<KidAssociation>()
                        .HasOne(ka => ka.User)
                        .WithMany(u => u.KidAssociations);

            modelBuilder.Entity<User>()
                        .HasMany(u => u.KidAssociations)
                        .WithOne(ka => ka.User);
            
            modelBuilder.Entity<KidAssociation>()
                        .HasOne(ka => ka.Kid)
                        .WithMany(k => k.KidAssociations);

            modelBuilder.Entity<Kid>()
                        .HasMany(k => k.KidAssociations)
                        .WithOne(ka => ka.Kid);

            modelBuilder.Entity<Kid>()
                        .Property(k => k.Gender)
                        .HasConversion(
                            v => v.ToString(),
                            v => (Gender)Enum.Parse(typeof(Gender), v));

            modelBuilder.Entity<MealPreference>()
                        .HasOne(mp => mp.Meal);

            modelBuilder.Entity<MealHistory>()
                        .HasOne(mp => mp.MealSuggestion);
            
            /*modelBuilder.Entity<MealPreference>()
                        .Property(mp => mp.MealType)
                        .HasConversion(
                                        mp => mp.ToString(),
                                        mp => (MealType)Enum.Parse(typeof(MealType), mp));*/

        }
    }
}