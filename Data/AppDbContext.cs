using InstagramExtraApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InstagramExtraApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Call> Calls => Set<Call>();
    public DbSet<Reaction> Reactions => Set<Reaction>();
    public DbSet<NotInterested> NotInterested => Set<NotInterested>();
    public DbSet<Sticker> Stickers => Set<Sticker>();
    public DbSet<ExtraMessage> ExtraMessages => Set<ExtraMessage>();
    public DbSet<MessageReaction> MessageReactions => Set<MessageReaction>();
    public DbSet<CallSignal> CallSignals => Set<CallSignal>();
    public DbSet<ExtraStory> ExtraStories => Set<ExtraStory>();
    public DbSet<StoryEvent> StoryEvents => Set<StoryEvent>();
    public DbSet<Repost> Reposts => Set<Repost>();
    public DbSet<StoryReaction> StoryReactions => Set<StoryReaction>();
    public DbSet<StoryReply> StoryReplies => Set<StoryReply>();
    public DbSet<StorySeen> StorySeen => Set<StorySeen>();
    public DbSet<AccountLink> AccountLinks => Set<AccountLink>();
    public DbSet<Block> Blocks => Set<Block>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Highlight> Highlights => Set<Highlight>();
    public DbSet<HighlightItem> HighlightItems => Set<HighlightItem>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionItem> CollectionItems => Set<CollectionItem>();
    public DbSet<CommentReply> CommentReplies => Set<CommentReply>();
    public DbSet<Privacy> Privacies => Set<Privacy>();
    public DbSet<FollowRequest> FollowRequests => Set<FollowRequest>();
    public DbSet<Presence> Presences => Set<Presence>();
    public DbSet<TimeCapsule> TimeCapsules => Set<TimeCapsule>();
    public DbSet<ProfileMusic> ProfileMusics => Set<ProfileMusic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Стартовые данные, чтобы после деплоя список не был пустым.
        modelBuilder.Entity<Location>().HasData(
            new Location { LocationId = 1, City = "Dushanbe", State = "Dushanbe", ZipCode = "734000", Country = "Tajikistan" },
            new Location { LocationId = 2, City = "New York", State = "New York", ZipCode = "10001", Country = "USA" },
            new Location { LocationId = 3, City = "Moscow", State = "Moscow", ZipCode = "101000", Country = "Russia" }
        );

        // Базовый набор стикеров (emoji-стикеры).
        var emojis = new[] { "😍", "🔥", "😂", "😎", "🥰", "😭", "👍", "🙏", "🎉", "💯", "👏", "❤️", "😮", "😢", "🤝", "👀" };
        var stickers = new List<Sticker>();
        for (var i = 0; i < emojis.Length; i++)
            stickers.Add(new Sticker { Id = i + 1, Pack = "classic", Name = emojis[i], Url = emojis[i] });
        modelBuilder.Entity<Sticker>().HasData(stickers);
    }
}
