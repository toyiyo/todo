using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using toyiyo.todo.Configuration;

namespace toyiyo.todo.Web.Startup
{
    [DependsOn(typeof(todoWebCoreModule))]
    public class todoWebMvcModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public todoWebMvcModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabled = false;
            Configuration.Navigation.Providers.Add<todoNavigationProvider>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(todoWebMvcModule).GetAssembly());
        }
    }
}
