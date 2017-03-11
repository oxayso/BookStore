using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OfficialBookStore.Models.Data
{
    public class Db : DbContext
    {
        public DbSet<PageDTO> Pages { get; set; }
        public DbSet<SidebarDTO> Sidebar { get; set; }
        public DbSet<CategoryDTO> Categories { get; set; }
        public DbSet<ProductDTO> Product { get; set; }
        public DbSet<UserDTO> User { get; set; }
        public DbSet<RoleDTO> Role { get; set; }
        public DbSet<UserRoleDTO> UserRole { get; set; }

    }
}