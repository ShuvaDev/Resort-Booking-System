using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon.Application.Common.Interfaces;
using WhiteLagoon.Application.Common.Utility;

namespace WhiteLagoon.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        public DbInitializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager) 
        { 
            _db = db;
            _roleManager = roleManager;
        }
        public async Task InitializeAsync()
        {
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
                if (!_roleManager.RoleExistsAsync(SD.Admin).GetAwaiter().GetResult())
                {
                    await _roleManager.CreateAsync(new IdentityRole(SD.Admin));
                    await _roleManager.CreateAsync(new IdentityRole(SD.User));
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
