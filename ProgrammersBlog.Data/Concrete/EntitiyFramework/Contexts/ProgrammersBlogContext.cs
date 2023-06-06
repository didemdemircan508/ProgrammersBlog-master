//using Microsoft.AspNetCore.Identity.EntityFramework;

using Microsoft.EntityFrameworkCore;
using ProgrammersBlog.Data.Concrete.EntitiyFramework.Mappings;
using ProgrammersBlog.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ProgrammersBlog.Data.Concrete.EntitiyFramework.Contexts
{
    public class ProgrammersBlogContext: IdentityDbContext<User, Role, int,UserClaim,UserRole,UserLogin,RoleClaim,UserToken>
    {

        public DbSet<Article> Articles { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Comment> Comments { get; set; }

       


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(connectionString: @"Server=IAS-DDEMIRCAN\SQLEXPRESS;DATABASE=master;Trusted_Connection=True;TrustServerCertificate=True");
            optionsBuilder.UseSqlServer(connectionString: @"Server=LAPTOP-6KVAH9H9\SQLEXPRESS;DATABASE=ProgrammersBlog;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;MultipleActiveResultSets=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ArticleMap());
            modelBuilder.ApplyConfiguration(new CategoryMap());
            modelBuilder.ApplyConfiguration(new CommentMap());
            modelBuilder.ApplyConfiguration(new UserMap());
            modelBuilder.ApplyConfiguration(new RoleClaimMap());
            modelBuilder.ApplyConfiguration(new UserClaimMap());
            modelBuilder.ApplyConfiguration(new UserLoginMap());
            modelBuilder.ApplyConfiguration(new UserRoleMap());
            modelBuilder.ApplyConfiguration(new UserTokenMap());
           



        }
    }
}
