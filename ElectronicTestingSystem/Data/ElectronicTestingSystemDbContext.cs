using ElectronicTestingSystem.Models;
using ElectronicTestingSystem.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ElectronicTestingSystem.Data
{
    public class ElectronicTestingSystemDbContext : IdentityDbContext
    {
        public ElectronicTestingSystemDbContext(DbContextOptions<ElectronicTestingSystemDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<MappedExamsAndQuestions> MappedExamsAndQuestions { get; set; }
        public DbSet<RequestedExams> RequestedExams { get; set; }
    }
}