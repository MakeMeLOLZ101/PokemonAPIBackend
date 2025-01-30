using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PokemonAPIBackend.Models;
using System.Net.Http;
using System.Text.Json;

namespace PokemonAPIBackend.Service
{
    public class PokemonService
    {
         private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://pokeapi.co/api/v2/";
        private const int MaxGeneration = 5;
        private readonly Random _random = new Random();

        public PokemonService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<PokemonModel> GetPokemonByNameOrId(string nameOrId)
        {
            try
            {
                // Convert input to lowercase for case-insensitive search
                var response = await _httpClient.GetAsync($"pokemon/{nameOrId.ToLower()}");
                response.EnsureSuccessStatusCode();
                
                var pokemonData = await JsonSerializer.DeserializeAsync<JsonElement>
                    (await response.Content.ReadAsStreamAsync());
                
                // Only process if Pokemon is from first 5 generations (ID <= 649)
                var pokemonId = pokemonData.GetProperty("id").GetInt32();
                if (pokemonId > 649) return null;

                return await CreatePokemonObject(pokemonData);
            }
            catch
            {
                return null;
            }
        }

        public async Task<PokemonModel> GetRandomPokemon()
        {
            // Generate random ID between 1 and 649 (Gen 1-5)
            var randomId = _random.Next(1, 650);
            return await GetPokemonByNameOrId(randomId.ToString());
        }

        private async Task<PokemonModel> CreatePokemonObject(JsonElement pokemonData)
        {
            var pokemon = new PokemonModel
            {
                Id = pokemonData.GetProperty("id").GetInt32(),
                Name = pokemonData.GetProperty("name").GetString(),
                Types = ExtractTypes(pokemonData),
                Abilities = ExtractAbilities(pokemonData),
                Moves = ExtractMoves(pokemonData),
                Sprites = ExtractSprites(pokemonData)
            };

            // Get locations
            pokemon.Locations = await GetLocations(pokemon.Id);
            
            // Get evolution chain
            pokemon.Evolution = await GetEvolutionChain(pokemon.Id);

            return pokemon;
        }

        private List<PokemonType> ExtractTypes(JsonElement pokemonData)
        {
            var types = new List<PokemonType>();
            var typesArray = pokemonData.GetProperty("types");

            foreach (var type in typesArray.EnumerateArray())
            {
                types.Add(new PokemonType
                {
                    Name = type.GetProperty("type").GetProperty("name").GetString()
                });
            }

            return types;
        }

        private List<Ability> ExtractAbilities(JsonElement pokemonData)
        {
            var abilities = new List<Ability>();
            var abilitiesArray = pokemonData.GetProperty("abilities");

            foreach (var ability in abilitiesArray.EnumerateArray())
            {
                abilities.Add(new Ability
                {
                    Name = ability.GetProperty("ability").GetProperty("name").GetString(),
                    IsHidden = ability.GetProperty("is_hidden").GetBoolean()
                });
            }

            return abilities;
        }

        private List<Move> ExtractMoves(JsonElement pokemonData)
        {
            var moves = new List<Move>();
            var movesArray = pokemonData.GetProperty("moves");

            foreach (var move in movesArray.EnumerateArray())
            {
                moves.Add(new Move
                {
                    Name = move.GetProperty("move").GetProperty("name").GetString()
                });
            }

            return moves;
        }

        private Sprites ExtractSprites(JsonElement pokemonData)
        {
            var spritesData = pokemonData.GetProperty("sprites");
            return new Sprites
            {
                Default = spritesData.GetProperty("front_default").GetString(),
                Shiny = spritesData.GetProperty("front_shiny").GetString()
            };
        }

        private async Task<List<Location>> GetLocations(int pokemonId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"pokemon/{pokemonId}/encounters");
                response.EnsureSuccessStatusCode();

                var locationsData = await JsonSerializer.DeserializeAsync<JsonElement[]>
                    (await response.Content.ReadAsStreamAsync());

                var validLocations = new List<Location>();
                foreach (var locationData in locationsData)
                {
                    var locationArea = locationData.GetProperty("location_area")
                        .GetProperty("name").GetString();
                    
                    // Only add locations from first 5 generations
                    var versionDetails = locationData.GetProperty("version_details");
                    foreach (var version in versionDetails.EnumerateArray())
                    {
                        var generationId = GetGenerationFromVersion(
                            version.GetProperty("version").GetProperty("name").GetString());
                        
                        if (generationId <= MaxGeneration)
                        {
                            validLocations.Add(new Location
                            {
                                Name = locationArea,
                                GenerationId = generationId
                            });
                            break;
                        }
                    }
                }

                if (validLocations.Count > 0)
        {
            var randomIndex = _random.Next(0, validLocations.Count);
            return new List<Location> { validLocations[randomIndex] };
        }

        return new List<Location>();
    }
    catch
    {
        return new List<Location>();
    }
        }

        private async Task<Evolution> GetEvolutionChain(int pokemonId)
        {
            try
            {
                // First get species data to get evolution chain URL
                var speciesResponse = await _httpClient.GetAsync($"pokemon-species/{pokemonId}");
                speciesResponse.EnsureSuccessStatusCode();
                
                var speciesData = await JsonSerializer.DeserializeAsync<JsonElement>
                    (await speciesResponse.Content.ReadAsStreamAsync());
                
                var evolutionUrl = speciesData.GetProperty("evolution_chain")
                    .GetProperty("url").GetString();

                // Get evolution chain data
                var evolutionResponse = await _httpClient.GetAsync(evolutionUrl);
                evolutionResponse.EnsureSuccessStatusCode();
                
                var evolutionData = await JsonSerializer.DeserializeAsync<JsonElement>
                    (await evolutionResponse.Content.ReadAsStreamAsync());

                return ExtractEvolutionChain(evolutionData.GetProperty("chain"));
            }
            catch
            {
                return new Evolution { Chain = new List<EvolutionDetail>() };
            }
        }

        private Evolution ExtractEvolutionChain(JsonElement chain)
        {
            var evolution = new Evolution { Chain = new List<EvolutionDetail>() };
            
            // Extract evolution chain recursively
            ExtractEvolutionDetails(chain, evolution.Chain);
            
            return evolution;
        }

        private void ExtractEvolutionDetails(JsonElement chain, List<EvolutionDetail> details)
        {
            var species = chain.GetProperty("species");
            details.Add(new EvolutionDetail
            {
                PokemonName = species.GetProperty("name").GetString(),
                PokemonId = ExtractIdFromUrl(species.GetProperty("url").GetString())
            });

            var evolvesTo = chain.GetProperty("evolves_to");
            foreach (var evolution in evolvesTo.EnumerateArray())
            {
                ExtractEvolutionDetails(evolution, details);
            }
        }

        private int ExtractIdFromUrl(string url)
        {
            var segments = url.Split('/');
            return int.Parse(segments[segments.Length - 2]);
        }

        private int GetGenerationFromVersion(string version)
        {
            // Map version names to generation numbers
            return version switch
            {
                var v when new[] {"red", "blue", "yellow"}.Contains(v) => 1,
                var v when new[] {"gold", "silver", "crystal"}.Contains(v) => 2,
                var v when new[] {"ruby", "sapphire", "emerald", "firered", "leafgreen"}.Contains(v) => 3,
                var v when new[] {"diamond", "pearl", "platinum", "heartgold", "soulsilver"}.Contains(v) => 4,
                var v when new[] {"black", "white", "black-2", "white-2"}.Contains(v) => 5,
                _ => 999 // Return high number for newer generations
            };
        }
    }
}