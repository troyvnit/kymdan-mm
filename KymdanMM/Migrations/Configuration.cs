using System.Web.Security;
using WebMatrix.WebData;

namespace KymdanMM.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<KymdanMM.Models.UsersContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(KymdanMM.Models.UsersContext context)
        {
            WebSecurity.InitializeDatabaseConnection("KymdanMMEntities", "UserProfile", "UserId", "UserName", autoCreateTables: true);
            if (!WebSecurity.UserExists("kymdanadmin"))
            {
                WebSecurity.CreateUserAndAccount("kymdanadmin", "Kymdan@123",
                    new { FirstName = "Kymdan", LastName = "Admin", DepartmentId = 0 });
            }
            if (!Roles.RoleExists("Admin"))
            {
                Roles.CreateRole("Admin");
            }
            if (!Roles.RoleExists("Department Manager"))
            {
                Roles.CreateRole("Department Manager");
            }
            if (!Roles.RoleExists("Member"))
            {
                Roles.CreateRole("Member");
            }
            if (Membership.GetUser("kymdanadmin") != null)
            {
                if (!Roles.IsUserInRole("kymdanadmin", "Admin"))
                {
                    Roles.AddUserToRole("kymdanadmin", "Admin");
                }
            }
        }
    }
}
