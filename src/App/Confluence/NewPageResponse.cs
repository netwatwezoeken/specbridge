namespace App.Confluence;

public record NewPageResponse(
    string id,
    string type,
    string status,
    string title
);

public record BadRequest(
    int statusCode,
    Data data,
    string message
);

public record Data(
    bool authorized,
    bool valid,
    object[] errors,
    bool successful
);

