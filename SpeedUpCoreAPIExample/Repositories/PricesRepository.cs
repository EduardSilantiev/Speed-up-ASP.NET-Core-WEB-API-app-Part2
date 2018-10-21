using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SpeedUpCoreAPIExample.Contexts;
using SpeedUpCoreAPIExample.Interfaces;
using SpeedUpCoreAPIExample.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace SpeedUpCoreAPIExample.Repositories
{
    public class PricesRepository : IPricesRepository
    {
        private readonly Settings _settings;
        private readonly DefaultContext _context;
        private readonly IDistributedCache _distributedCache;

        public PricesRepository(DefaultContext context, IConfiguration configuration, IDistributedCache distributedCache)
        {
            _settings = new Settings(configuration);

            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<IEnumerable<Price>> GetPricesAsync(int productId)
        {
            IEnumerable<Price> prices = null;

            string cacheKey = "Prices: " + productId;

            var pricesTemp = await _distributedCache.GetStringAsync(cacheKey);
            if (pricesTemp != null)
            {
                //Deserialize
                prices = JsonConvert.DeserializeObject<IEnumerable<Price>>(pricesTemp);
            }
            else
            {
                prices = await _context.Prices.AsNoTracking().FromSql("[dbo].GetPricesByProductId @productId = {0}", productId).ToListAsync();

                //cache prices for PricesExpirationPeriod minutes
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.PricesExpirationPeriod));
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(prices), cacheOptions);
            }

            return prices;
        }

        public async Task PreparePricesAsync(int productId)
        {
            IEnumerable<Price> prices = null;

            string cacheKey = "Prices: " + productId;

            var pricesTemp = await _distributedCache.GetStringAsync(cacheKey);
            if (pricesTemp != null)
            {
                //already cached
                return;
            }
            else
            {
                prices = await _context.Prices.AsNoTracking().FromSql("[dbo].GetPricesByProductId @productId = {0}", productId).ToListAsync();

                //cache prices for PricesExpirationPeriod minutes
                DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions()
                                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_settings.PricesExpirationPeriod));
                await _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(prices), cacheOptions);
            }
            return;
        }

        private class Settings
        {
            public int PricesExpirationPeriod = 15;       //15 minutes by default

            public Settings(IConfiguration configuration)
            {
                int pricesExpirationPeriod;
                if (Int32.TryParse(configuration["Caching:PricesExpirationPeriod"], NumberStyles.Any,
                                    NumberFormatInfo.InvariantInfo, out pricesExpirationPeriod))
                {
                    PricesExpirationPeriod = pricesExpirationPeriod;
                }
            }
        }
    }
}