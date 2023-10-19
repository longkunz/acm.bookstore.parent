using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Acme.Parent;

[DependsOn(
    typeof(ParentApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class ParentHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(ParentApplicationContractsModule).Assembly,
            ParentRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ParentHttpApiClientModule>();
        });

    }
}
