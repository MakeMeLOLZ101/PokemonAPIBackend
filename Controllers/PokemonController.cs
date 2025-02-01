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
        private readonly ILogger<PokemonController> _logger;

        public PokemonController(PokemonService pokemonService, ILogger<PokemonController> logger)
        {
            _pokemonService = pokemonService;
            _logger = logger;
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
    try 
    {
        var pokemon = await _pokemonService.GetRandomPokemon();
        if (pokemon == null)
            return StatusCode(500, "Failed to fetch random Pokemon");
            
        return Ok(pokemon);
    }
    catch (Exception ex)
    {
        return StatusCode(500, "An error occurred while fetching random Pokemon");
    }
}
    }
}