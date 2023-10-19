using Localization.Resources.AbpUi;
using Acme.Parent.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Acme.Parent;

[DependsOn(
    typeof(ParentApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class ParentHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ParentHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ParentResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
