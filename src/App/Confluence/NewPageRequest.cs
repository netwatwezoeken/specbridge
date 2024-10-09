namespace App.Confluence;

public record NewPageRequest(
    Ancestors[] ancestors,
    NewBody body,
    Space space,
    string title,
    string type
);