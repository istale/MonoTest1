using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations.History;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace MonoTest1.Models
{
    // 您可以在 ApplicationUser 類別新增更多屬性，為使用者新增設定檔資料，請造訪 http://go.microsoft.com/fwlink/?LinkID=317594 以深入了解。
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // 注意 authenticationType 必須符合 CookieAuthenticationOptions.AuthenticationType 中定義的項目
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在這裡新增自訂使用者宣告
            return userIdentity;
        }
    }

    [DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        static ApplicationDbContext()
        {
            Database.SetInitializer(new MySqlInitializer());
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
        
    }

    public class MySqlHistoryContext : HistoryContext
    {
        public MySqlHistoryContext(
          DbConnection existingConnection,
          string defaultSchema)
        : base(existingConnection, defaultSchema)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<HistoryRow>().Property(h => h.MigrationId).HasMaxLength(100).IsRequired();
            modelBuilder.Entity<HistoryRow>().Property(h => h.ContextKey).HasMaxLength(200).IsRequired();
        }
    }

    public class MySqlConfiguration : DbConfiguration
    {
        public MySqlConfiguration()
        {
            SetHistoryContext(
            "MySql.Data.MySqlClient", (conn, schema) => new MySqlHistoryContext(conn, schema));
        }
    }

    public class MySqlInitializer : IDatabaseInitializer<ApplicationDbContext>
    {
        public void InitializeDatabase(ApplicationDbContext context)
        {
            if (!context.Database.Exists())
            {
                // if database did not exist before - create it
                context.Database.Create();
            }
            else
            {
                // query to check if MigrationHistory table is present in the database 
                var migrationHistoryTableExists = ((IObjectContextAdapter)context).ObjectContext.ExecuteStoreQuery<int>(
                string.Format(
                  "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '{0}' AND table_name = '__MigrationHistory'",
                  "[Insert your database schema here - such as 'users']"));

                // if MigrationHistory table is not there (which is the case first time we run) - create it
                if (migrationHistoryTableExists.FirstOrDefault() == 0)
                {
                    context.Database.Delete();
                    context.Database.Create();
                }
            }
        }
    }
}