using Acme.Parent.Localization;
using Volo.Abp.Application.Services;

namespace Acme.Parent;

public abstract class ParentAppService : ApplicationService
{
    protected ParentAppService()
    {
        LocalizationResource = typeof(ParentResource);
        ObjectMapperContext = typeof(ParentApplicationModule);
    }
}
