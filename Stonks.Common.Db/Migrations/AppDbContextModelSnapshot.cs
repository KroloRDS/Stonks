﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Stonks.Common.Db;

#nullable disable

namespace Stonks.Common.Db.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.AvgPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(8,2)");

                    b.Property<decimal>("SharesTraded")
                        .HasColumnType("decimal(20,0)");

                    b.Property<Guid>("StockId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.ToTable("AvgPrice");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Log", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ClassName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Exception")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Message")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ObjectDump")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Log");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Share", b =>
                {
                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.HasKey("OwnerId", "StockId");

                    b.HasIndex("StockId");

                    b.ToTable("Share");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Stock", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("BankruptDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.HasKey("Id");

                    b.HasIndex("Ticker")
                        .IsUnique();

                    b.ToTable("Stock");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.TradeOffer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(8,2)");

                    b.Property<Guid>("StockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<Guid?>("WriterId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("StockId");

                    b.HasIndex("WriterId");

                    b.ToTable("TradeOffer");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Amount")
                        .HasColumnType("int");

                    b.Property<Guid>("BuyerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(8,2)");

                    b.Property<Guid?>("SellerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StockId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("BuyerId");

                    b.HasIndex("SellerId");

                    b.HasIndex("StockId");

                    b.HasIndex("Timestamp");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Funds")
                        .HasColumnType("decimal(11,2)");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varbinary(32)");

                    b.Property<string>("Roles")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<short>("Salt")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.AvgPrice", b =>
                {
                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Share", b =>
                {
                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("Stock");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.TradeOffer", b =>
                {
                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.User", "Writer")
                        .WithMany()
                        .HasForeignKey("WriterId");

                    b.Navigation("Stock");

                    b.Navigation("Writer");
                });

            modelBuilder.Entity("Stonks.Common.Db.EntityFrameworkModels.Transaction", b =>
                {
                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.User", "Buyer")
                        .WithMany()
                        .HasForeignKey("BuyerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.User", "Seller")
                        .WithMany()
                        .HasForeignKey("SellerId");

                    b.HasOne("Stonks.Common.Db.EntityFrameworkModels.Stock", "Stock")
                        .WithMany()
                        .HasForeignKey("StockId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Buyer");

                    b.Navigation("Seller");

                    b.Navigation("Stock");
                });
#pragma warning restore 612, 618
        }
    }
}
