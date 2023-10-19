using Acme.Parent.Dtos;
using Acme.Parent.Kafka;
using Acme.Parent.ServiceInterfaces;
using Bogus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.EventBus.Distributed;

namespace Acme.Parent.Services
{
    public class ProductService : ParentAppService, IProductService
    {
        private readonly IDistributedEventBus _distributedEventBus;
        private readonly ILogger<ProductService> _logger;
        public ProductService(IDistributedEventBus distributedEventBus, ILogger<ProductService> logger)
        {
            _distributedEventBus = distributedEventBus;
            _logger = logger;
        }
        /// <summary>
        ///   <para>
        /// Tạo fake data danh sách sản phẩm</para>
        /// </summary>
        /// <returns>
        ///   <br />
        /// </returns>
        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            try
            {
                var productFaker = new Faker<ProductDto>()
                    .RuleFor(p => p.Id, f => f.UniqueIndex)
                    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Price, f => f.Random.Double(100, 1000));

                var products = Enumerable.Range(0, 10).Select(x =>
                {
                    return productFaker.Generate();
                });

                return await Task.FromResult(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductService - GetProductsAsync - Error: {Error}", ex.Message);
                throw;
            }

        }

        public async Task PublicProduct2Kafka()
        {
            try
            {
                var productFaker = new Faker<ProductEto>()
                 .RuleFor(p => p.Id, f => f.UniqueIndex)
                 .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                 .RuleFor(p => p.Price, f => f.Random.Double(100, 1000));

                var request = productFaker.Generate();
                _logger.LogInformation("ProductService - PublicProduct2Kafka - Request: {Data}", System.Text.Json.JsonSerializer.Serialize(request));
                await _distributedEventBus.PublishAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProductService - PublicProduct2Kafka - Error: {Error}", ex.Message);
                throw;
            }
        }
    }
}
