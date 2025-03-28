﻿using System;
using Abp.AspNetCore.MultiTenancy;
using Abp.Configuration.Startup;
using Abp.Localization;
using Abp.Modules;
using Abp.Net.Mail;
using Abp.Reflection.Extensions;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.Zero;
using Abp.Zero.Configuration;
using toyiyo.todo.Authorization.Roles;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Configuration;
using toyiyo.todo.Email;
using toyiyo.todo.Localization;
using toyiyo.todo.MultiTenancy;
using toyiyo.todo.Notifications;
using toyiyo.todo.Timing;

namespace toyiyo.todo
{
    [DependsOn(typeof(AbpZeroCoreModule))]
    public class todoCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.ReplaceService<IEmailSender, SendGridEmailSender>();
            Configuration.Notifications.Providers.Add<TodoNotificationProvider>();
            Configuration.Auditing.IsEnabledForAnonymousUsers = true;

            // Declare entity types
            Configuration.Modules.Zero().EntityTypes.Tenant = typeof(Tenant);
            Configuration.Modules.Zero().EntityTypes.Role = typeof(Role);
            Configuration.Modules.Zero().EntityTypes.User = typeof(User);

            todoLocalizationConfigurer.Configure(Configuration.Localization);

            // Enable this line to create a multi-tenant application.
            Configuration.MultiTenancy.IsEnabled = todoConsts.MultiTenancyEnabled;
            // Register custom tenant resolver
            Configuration.MultiTenancy.Resolvers.Add<UrlParameterTenantResolveContributor>();


            // Configure roles
            AppRoleConfig.Configure(Configuration.Modules.Zero().RoleManagement);

            Configuration.Settings.Providers.Add<AppSettingProvider>();
            
            Configuration.Localization.Languages.Add(new LanguageInfo("fa", "فارسی", "famfamfam-flags ir"));
            
            Configuration.Settings.SettingEncryptionConfiguration.DefaultPassPhrase = todoConsts.DefaultPassPhrase;
            SimpleStringCipher.DefaultPassPhrase = todoConsts.DefaultPassPhrase;
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(todoCoreModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }

    }
}
