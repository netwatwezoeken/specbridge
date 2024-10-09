namespace App.Confluence;

public record Results(
    string id,
    string status,
    string title,
    string spaceId,
    int childPosition
);