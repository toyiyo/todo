using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using toyiyo.todo.Authorization.Roles;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.EntityFrameworkCore
{
    public class todoDbContext : AbpZeroDbContext<Tenant, Role, User, todoDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public todoDbContext(DbContextOptions<todoDbContext> options)
            : base(options)
        {
        }
    }
}
