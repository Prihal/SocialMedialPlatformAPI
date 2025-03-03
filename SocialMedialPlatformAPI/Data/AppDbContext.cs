using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.Data
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Chat> Chats { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<Like> Likes { get; set; }

        public virtual DbSet<MediaType> MediaTypes { get; set; }

        public virtual DbSet<Message> Messages { get; set; }

        public virtual DbSet<Notification> Notifications { get; set; }

        public virtual DbSet<Post> Posts { get; set; }

        public virtual DbSet<PostMapping> PostMappings { get; set; }

        public virtual DbSet<Request> Requests { get; set; }

        public virtual DbSet<Story> Stories { get; set; }

        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chat>(entity =>
            {
                entity.HasKey(e => e.ChatId);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()")
                    .HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.ChatFromUsers)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.ChatToUsers)
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId);

                entity.Property(e => e.CommentText).HasColumnType("text");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Like>(entity =>
            {
                entity.HasKey(e => e.LikeId);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<MediaType>(entity =>
            {
                entity.HasKey(e => e.MediaTypeId);

                entity.Property(e => e.MediaType1)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("MediaType");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.MessageId);

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETDATE()")
                    .HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.MessageFromUsers)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.MessageToUsers)
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Comment)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.CommentId);

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.NotificationFromUsers)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Like)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.LikeId);

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.PostId);

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.RequestId);

                entity.HasOne(d => d.Story)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.StoryId);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.NotificationToUsers)
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.PostId);

                entity.Property(e => e.Caption).HasColumnType("text");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Location)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.PostType)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.PostTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<PostMapping>(entity =>
            {
                entity.HasKey(e => e.PostMappingId);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.MediaName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.MediaUrl)
                    .IsUnicode(false)
                    .HasColumnName("MediaURL");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.MediaType)
                    .WithMany(p => p.PostMappings)
                    .HasForeignKey(d => d.MediaTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.PostMappings)
                    .HasForeignKey(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasKey(e => e.RequestId);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.FromUser)
                    .WithMany(p => p.RequestFromUsers)
                    .HasForeignKey(d => d.FromUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.ToUser)
                    .WithMany(p => p.RequestToUsers)
                    .HasForeignKey(d => d.ToUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Story>(entity =>
            {
                entity.HasKey(e => e.StoryId);

                entity.Property(e => e.Caption)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
                entity.Property(e => e.StoryName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.StoryUrl)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("StoryURL");

                entity.HasOne(d => d.StoryType)
                    .WithMany(p => p.Stories)
                    .HasForeignKey(d => d.StoryTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Stories)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Bio).HasColumnType("text");
                entity.Property(e => e.CreatedDate).HasColumnType("datetime");
                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);
                entity.Property(e => e.IsPrivate).HasDefaultValue(false);
                entity.Property(e => e.IsVerified).HasDefaultValue(false);
                entity.Property(e => e.Link)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.LoginType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.ProfilePictureName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
