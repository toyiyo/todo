using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using toyiyo.todo.Configuration;

namespace toyiyo.todo.Web.Host.Startup
{
    [DependsOn(
       typeof(todoWebCoreModule))]
    public class todoWebHostModule : AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public todoWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }
        public override void PreInitialize()
        {
            Configuration.Auditing.IsEnabled = false;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(todoWebHostModule).GetAssembly());
        }
    }
}
