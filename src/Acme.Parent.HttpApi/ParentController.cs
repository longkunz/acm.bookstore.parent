using Acme.Parent.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Acme.Parent;

public abstract class ParentController : AbpControllerBase
{
    protected ParentController()
    {
        LocalizationResource = typeof(ParentResource);
    }
}
