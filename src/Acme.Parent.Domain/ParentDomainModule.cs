using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Acme.Parent;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ParentDomainSharedModule)
)]
public class ParentDomainModule : AbpModule
{

}
