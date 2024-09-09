using DonationWall.Entities;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data.Entity;

namespace DonationWall.Database
{
    public class DSContext : IdentityDbContext<User>, IDisposable
    {
        public DSContext() : base("DWConnectionStrings")
        {
        }

        public static DSContext Create()
        {
            return new DSContext();
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<PostImages> PostImages { get; set; }
        public DbSet<PostVideos> PostVideos { get; set; }
        public DbSet<PostHidden> PostHiddens { get; set; }
        public DbSet<DonationHistory> DonationHistories { get; set; }
    }
}
