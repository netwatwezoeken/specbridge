namespace App.Confluence;

public record UpdatePageRequest(
    string id,
    string status,
    string title,
    Body body,
    Version version
);