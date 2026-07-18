namespace InstagramExtraApi.Models;

/// <summary>Пометка «не интересует» для поста (клиент фильтрует ленту).</summary>
public class NotInterested
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Стикер из каталога.</summary>
public class Sticker
{
    public int Id { get; set; }
    public string Pack { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty; // картинка или эмодзи
}

/// <summary>
/// Сообщение расширенного чата (то, чего нет в основном API): поддержка
/// text/image/video/file/voice/gif/sticker + поиск и фильтр по типу медиа.
/// </summary>
public class ExtraMessage
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // text|image|video|file|voice|gif|sticker
    public string? Text { get; set; }
    public string? MediaUrl { get; set; }
    public string? FileName { get; set; }
    public int? DurationSec { get; set; } // для голосовых
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Эмодзи-реакция на сообщение (одна на пользователя+сообщение).</summary>
public class MessageReaction
{
    public int Id { get; set; }
    public int MessageId { get; set; } // id сообщения (основного API или ExtraMessage)
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>WebRTC-сигнал звонка (offer/answer/ICE-candidate) для реального P2P.</summary>
public class CallSignal
{
    public int Id { get; set; }
    public int CallId { get; set; }
    public string FromUserId { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty; // offer|answer|candidate
    public string Payload { get; set; } = string.Empty; // SDP или JSON ICE-кандидата
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Сторис (живёт 24 часа).</summary>
public class ExtraStory
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? UserImage { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string Type { get; set; } = "image"; // image|video
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
}

/// <summary>Событие сторис: просмотр или лайк.</summary>
public class StoryEvent
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Kind { get; set; } = "view"; // view|like
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Присутствие: когда пользователь последний раз пинговал (для «в сети»).</summary>
public class Presence
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
}

/// <summary>«Капсула времени»: пост скрыт до даты RevealAt.</summary>
public class TimeCapsule
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty; // владелец поста (кто поставил)
    public DateTime RevealAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Закреплённая музыка в профиле (30-сек превью-трек из iTunes).</summary>
public class ProfileMusic
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string TrackName { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty; // 30-сек аудио
    public string ArtworkUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
