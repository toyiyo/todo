﻿using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using toyiyo.todo.Authorization.Roles;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;
using Abp.Localization;

namespace toyiyo.todo.EntityFrameworkCore
{
    public class todoDbContext : AbpZeroDbContext<Tenant, Role, User, todoDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<Project> Projects { get; set; }

        public todoDbContext(DbContextOptions<todoDbContext> options)
            : base(options)
        {
        }
        // add these lines to override max length of property
        // we should set max length smaller than the PostgreSQL allowed size (10485760)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationLanguageText>()
                .Property(p => p.Value)
                .HasMaxLength(100); // any integer that is smaller than 10485760
        }
    }
}
