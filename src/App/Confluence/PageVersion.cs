namespace App.Confluence;

public record PageVersion(
    int number,
    string message,
    bool minorEdit,
    string authorId,
    string createdAt
);