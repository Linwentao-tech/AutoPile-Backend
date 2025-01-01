using AutoPile.DATA.Configurations;
using AutoPile.DOMAIN.Models.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoPile.DATA.Data
{
    public class AutoPileMongoDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public static AutoPileMongoDbContext Create(IMongoDatabase database) =>
            new(new DbContextOptionsBuilder<AutoPileMongoDbContext>()
                .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
                .Options);

        public AutoPileMongoDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ProductConfigurations());
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>().ToCollection("products");
            modelBuilder.Entity<Review>().ToCollection("reviews");
        }
    }
}