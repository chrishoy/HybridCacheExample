﻿using Microsoft.Extensions.Caching.Hybrid;
using System.Text.Json;

namespace HybridCacheExample;

internal class WeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HybridCache _hybridCache;

    public WeatherService(IHttpClientFactory httpClientFactory, HybridCache hybridCache)
    {
        _httpClientFactory = httpClientFactory;
        _hybridCache = hybridCache;
    }


    public async Task<WeatherResponse?> GetCurrentWeatherAsync(string cityOrCountry, string apiKey)
    {
        var cacheKey = $"WeatherService.GetCurrentWeatherAsync({cityOrCountry})";

        // Either lookup weather from cache or fetch it from the API - Tag all entries with "weather"
        return await _hybridCache.GetOrCreateAsync(cacheKey, 
            async _ => await GetWeatherFromApiAsync(cityOrCountry, apiKey), tags: ["weather"]);
    }

    private async Task<WeatherResponse?> GetWeatherFromApiAsync(string cityOrCountry, string apiKey)
    {
        var client = _httpClientFactory.CreateClient("OpenWeather");
        var response = await client.GetAsync($"weather?q={cityOrCountry}&appid={apiKey}&units=metric");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WeatherResponse>(content);
        }

        return null;
    }
}
