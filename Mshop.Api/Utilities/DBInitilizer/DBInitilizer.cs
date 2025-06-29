
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Mshop.Api.Data;
using Mshop.Api.Data.models;

namespace Mshop.Api.Utilities.DBInitilizer
{
    public class DBInitilizer : IDBInitilizer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration configuration;

        public DBInitilizer(ApplicationDbContext context,RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager,IConfiguration configuration)
        {
            _context = context;
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.configuration = configuration;
        }
        public async Task Initilize()
        {
            try
            {  
                await _context.Database.MigrateAsync();
            } catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }

            if (!roleManager.Roles.Any()) {
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.SuperAdmin));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Admin));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Customer));
                await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Company));

                var applicationUser = new ApplicationUser()
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    UserName = "super_admin",
                    Email = "mohammadsof72@gmail.com",
                    Gender = ApplicationUserGender.MALE,
                    Address = "#####",
                    City = "#####",
                    BirthDate = new DateTime(1990, 1, 1),
                    EmailConfirmed = true,
                };
                var result = await userManager.CreateAsync(applicationUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(applicationUser, ApplicationRoles.SuperAdmin);
                }
                else
                {
                    foreach (var e in result.Errors) {
                        Console.WriteLine(e.Description);

                    }
                }
            }
        }
    }
}
