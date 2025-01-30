using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonAPIBackend.Models
{
    public class PokemonModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<PokemonType> Types { get; set; }
        public List<Ability> Abilities { get; set; }
        public List<Move> Moves { get; set; }
        public List<Location> Locations { get; set; }
        public Evolution Evolution { get; set; }
        public Sprites Sprites { get; set; }
    }

    public class PokemonType
    {
        public string Name { get; set; }
    }

    public class Ability
    {
        public string Name { get; set; }
        public bool IsHidden { get; set; }
    }

    public class Move
    {
        public string Name { get; set; }
    }

    public class Location
    {
        public string Name { get; set; }
        public int GenerationId { get; set; }
    }

    public class Evolution
    {
        public List<EvolutionDetail> Chain { get; set; }
    }

    public class EvolutionDetail
    {
        public string PokemonName { get; set; }
        public int PokemonId { get; set; }
    }

    public class Sprites
    {
        public string Default { get; set; }
        public string Shiny { get; set; }
    }

}