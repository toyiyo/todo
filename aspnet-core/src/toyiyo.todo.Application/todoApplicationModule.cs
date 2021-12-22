using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using toyiyo.todo.Authorization;

namespace toyiyo.todo
{
    [DependsOn(
        typeof(todoCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class todoApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<todoAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(todoApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
