using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Validation;
using toyiyo.todo.Validation;

namespace toyiyo.todo.Web.Models.Account
{
    public class RegisterCompanyAdminViewModel : ICustomValidate
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        [RegularExpression(AbpTenantBase.TenancyNameRegex)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(AbpTenantBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

        public bool IsExternalLogin { get; set; }

        public string ExternalLoginAuthSchema { get; set; }

        public void AddValidationErrors(CustomValidationContext context)
        {
            if (!EmailAddress.Contains(TenancyName))
            {
                context.Results.Add(new ValidationResult("Email must match the domain registered"));
            }
        }
    }
}
