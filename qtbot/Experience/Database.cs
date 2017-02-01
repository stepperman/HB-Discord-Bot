using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace qtbot.Experience
{
    class Database
    {

    }

    public class ExperienceContext : DbContext
    {
        public DbSet<ExperienceUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=./LocalFiles/experience.db");
        }
    }

    [Table("users")]
    public class ExperienceUser
    {
        [Key]
        public int PrimaryKey { get; set; }

        public ulong UserID { get; set; }
        public ulong ServerID { get; set; }

        public DateTime LastMessage { get; set; }
        public DateTime LastResettedXP { get; set; }
        public int DisplayXP { get; set; }
        public int FullXP { get; set; }
    }
}
