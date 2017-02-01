using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using qtbot.Experience;

namespace qtbot.Migrations
{
    [DbContext(typeof(ExperienceContext))]
    [Migration("20170201224421_XPUser")]
    partial class XPUser
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("qtbot.Experience.ExperienceUser", b =>
                {
                    b.Property<int>("PrimaryKey")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DisplayXP");

                    b.Property<int>("FullXP");

                    b.Property<DateTime>("LastMessage");

                    b.Property<DateTime>("LastResettedXP");

                    b.Property<ulong>("ServerID");

                    b.Property<ulong>("UserID");

                    b.HasKey("PrimaryKey");

                    b.ToTable("users");
                });
        }
    }
}
