using Acme.Parent.Dtos;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.Parent.ServiceInterfaces
{
    public interface IProductService : IApplicationService
    {
        Task<IEnumerable<ProductDto>> GetProductsAsync();
        Task PublicProduct2Kafka();
    }
}
