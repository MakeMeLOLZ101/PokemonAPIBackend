using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PokemonAPIBackend.Models;
using PokemonAPIBackend.Service;

namespace PokemonAPIBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PokemonController : ControllerBase
    {
        private readonly PokemonService _pokemonService;

        public PokemonController(PokemonService pokemonService)
        {
            _pokemonService = pokemonService;
        }

        [HttpGet("{nameOrId}")]
        public async Task<ActionResult<PokemonModel>> GetPokemon(string nameOrId)
        {
            var pokemon = await _pokemonService.GetPokemonByNameOrId(nameOrId);
            if (pokemon == null)
                return NotFound();
                
            return Ok(pokemon);
        }

        [HttpGet("random")]
        public async Task<ActionResult<PokemonModel>> GetRandomPokemon()
        {
            var pokemon = await _pokemonService.GetRandomPokemon();
            if (pokemon == null)
                return NotFound();
                
            return Ok(pokemon);
        }
    }
}