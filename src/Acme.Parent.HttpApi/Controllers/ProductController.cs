using Acme.Parent.Dtos;
using Acme.Parent.ServiceInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Parent.Controllers
{
    [Route("[controller]")]
    public class ProductController : ParentController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("products")]
        public async Task<IEnumerable<ProductDto>> GetProductsAsync()
        {
            return await _productService.GetProductsAsync();
        }

        [HttpPost]
        [Route("public-product-event")]
        public async Task PublicProductAsync()
        {
            await _productService.PublicProduct2Kafka();
        }
    }
}
