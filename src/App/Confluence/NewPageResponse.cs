namespace App.Confluence;

public record NewPageResponse(
    string id,
    string type,
    string status,
    string title
);