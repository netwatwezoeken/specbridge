namespace App.Confluence;

public record PageResponse(
    object parentType,
    string authorId,
    string createdAt,
    PageVersion version,
    object position,
    string title,
    string status,
    object lastOwnerId,
    PageBody body,
    object parentId,
    string spaceId,
    string ownerId,
    string id
);