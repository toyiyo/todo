using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Roles;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Authorization.Roles
{
    [Index(nameof(TenantId))]
    public class Role : AbpRole<User>
    {
        public const int MaxDescriptionLength = 5000;

        public Role()
        {
        }

        public Role(int? tenantId, string displayName)
            : base(tenantId, displayName)
        {
        }

        public Role(int? tenantId, string name, string displayName)
            : base(tenantId, name, displayName)
        {
        }

        [StringLength(MaxDescriptionLength)]
        public string Description {get; set;}
    }
}
