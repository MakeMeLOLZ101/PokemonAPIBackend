using PokemonAPIBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace PokemonAPIBackend.Context
{
    public class DataContext : DbContext 
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<PokemonModel> Users {get; set;}
    }
}