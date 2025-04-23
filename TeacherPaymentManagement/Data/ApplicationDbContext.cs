using Microsoft.EntityFrameworkCore;
using TeacherPaymentManagement.Models;

namespace TeacherPaymentManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<PaymentSetting> PaymentSettings { get; set; }
        public DbSet<TeacherEntry> TeacherEntries { get; set; }
    }
}
