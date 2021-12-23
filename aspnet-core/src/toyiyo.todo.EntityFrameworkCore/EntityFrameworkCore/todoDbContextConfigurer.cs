using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace toyiyo.todo.EntityFrameworkCore
{
    public static class todoDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<todoDbContext> builder, string connectionString)
        {
            builder.UseNpgsql(connectionString);
        }

        public static void Configure(DbContextOptionsBuilder<todoDbContext> builder, DbConnection connection)
        {
            builder.UseNpgsql(connection);
        }
    }
}
