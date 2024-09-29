﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransactionUploadService.Data;

#nullable disable

namespace TransactionUploadService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.20")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TransactionUploadService.Models.Entities.Transaction", b =>
                {
                    b.Property<string>("ReferenceNumber")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderSide")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OrderStatus")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Quantity")
                        .HasColumnType("bigint");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("ReferenceNumber");

                    b.ToTable("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
