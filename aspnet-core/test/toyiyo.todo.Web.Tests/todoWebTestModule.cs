using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using toyiyo.todo.EntityFrameworkCore;
using toyiyo.todo.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Abp.Configuration.Startup;
using Abp.Net.Mail;
using Abp.Dependency;

namespace toyiyo.todo.Web.Tests
{
    [DependsOn(
        typeof(todoWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class todoWebTestModule : AbpModule
    {
        public todoWebTestModule(todoEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
            Configuration.ReplaceService<IEmailSender, NullEmailSender>(DependencyLifeStyle.Transient);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(todoWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(todoWebMvcModule).Assembly);
        }
    }
}