﻿// <auto-generated />

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SampleApi.V1.Data.Migrations
{
    [DbContext(typeof(SampleDbContext))]
    partial class SampleDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SampleApi.V1.Data.Model.Address", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Building")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<long>("PersonId");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("Country");

                    b.HasIndex("PersonId");

                    b.ToTable("Addresses");

                    b.HasData(
                        new { Id = 1L, Building = "Golden Sands Tower", City = "Sharja", Country = "United Arab Emirates", PersonId = 1L, Street = "Wehda Street", Type = 0 }
                    );
                });

            modelBuilder.Entity("SampleApi.V1.Data.Model.Person", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("People");

                    b.HasData(
                        new { Id = 1L, Name = "Hamza Althunibat" }
                    );
                });

            modelBuilder.Entity("SampleApi.V1.Data.Model.Address", b =>
                {
                    b.HasOne("SampleApi.V1.Data.Model.Person", "Person")
                        .WithMany("Addresses")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
