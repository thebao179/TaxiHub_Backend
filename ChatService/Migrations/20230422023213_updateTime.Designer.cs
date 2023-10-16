﻿// <auto-generated />
using System;
using ChatService.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatService.Migrations
{
    [DbContext(typeof(ChatDbContext))]
    [Migration("20230422023213_updateTime")]
    partial class updateTime
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ChatService.Models.Chat", b =>
                {
                    b.Property<Guid>("TripId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("DriverId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PassengerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("TripCreatedTime")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("TripId");

                    b.ToTable("Chat", "dbo");
                });

            modelBuilder.Entity("ChatService.Models.ChatMessage", b =>
                {
                    b.Property<Guid>("ChatMessageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("SendTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<Guid>("SenderId")
                        .HasColumnType("uuid");

                    b.Property<string>("SenderName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("TripId")
                        .HasColumnType("uuid");

                    b.HasKey("ChatMessageId");

                    b.ToTable("ChatMessage", "dbo");
                });
#pragma warning restore 612, 618
        }
    }
}
