using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpeedUpCoreAPIExample.Interfaces;
using SpeedUpCoreAPIExample.Models;
using SpeedUpCoreAPIExample.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedUpCoreAPIExample.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IProductsRepository _productsRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly string _apiUrl;

        public ProductsService(IProductsRepository productsRepository, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _productsRepository = productsRepository;
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;

            _apiUrl = GetFullyQualifiedApiUrl("/api/prices/prepare/");
        }

        private string GetFullyQualifiedApiUrl(string apiRout)
        {
            string apiUrl = string.Format("{0}://{1}{2}",
                            _httpContextAccessor.HttpContext.Request.Scheme,
                            _httpContextAccessor.HttpContext.Request.Host,
                            apiRout);

            return apiUrl;
        }

        public async Task<IActionResult> FindProductsAsync(string sku)
        {
            try
            {
                IEnumerable<Product> products = await _productsRepository.FindProductsAsync(sku);

                if (products != null)
                {

                    if (products.Count() == 1)
                    {
                        //only one record found - prepare prices beforehand
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            PreparePricesAsync(products.FirstOrDefault().ProductId);
                        });
                    };

                    return new OkObjectResult(products.Select(p => new ProductViewModel()
                    {
                        Id = p.ProductId,
                        Sku = p.Sku,
                        Name = p.Name
                    }
                    ));
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> GetAllProductsAsync()
        {
            try
            {
                IEnumerable<Product> products = await _productsRepository.GetAllProductsAsync();

                if (products != null)
                {
                    return new OkObjectResult(products.Select(p => new ProductViewModel()
                    {
                        Id = p.ProductId,
                        Sku = p.Sku,
                        Name = p.Name
                    }
                    ));
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> GetProductAsync(int productId)
        {
            try
            {
                Product product = await _productsRepository.GetProductAsync(productId);

                if (product != null)
                {

                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        PreparePricesAsync(productId);
                    });

                    return new OkObjectResult(new ProductViewModel()
                    {
                        Id = product.ProductId,
                        Sku = product.Sku,
                        Name = product.Name
                    });
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch
            {
                return new ConflictResult();
            }
        }

        public async Task<IActionResult> DeleteProductAsync(int productId)
        {
            try
            {
                Product product = await _productsRepository.DeleteProductAsync(productId);

                if (product != null)
                {
                    return new OkObjectResult(new ProductViewModel()
                    {
                        Id = product.ProductId,
                        Sku = product.Sku,
                        Name = product.Name
                    });
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch
            {
                return new ConflictResult();
            }
        }

        private async void PreparePricesAsync(int productId)
        {

            var parameters = new Dictionary<string, string>();
            var encodedContent = new FormUrlEncodedContent(parameters);

            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                var result = await client.PostAsync(_apiUrl + productId, encodedContent).ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }
}