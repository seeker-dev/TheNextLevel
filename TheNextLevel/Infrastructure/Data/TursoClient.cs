using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheNextLevel.Infrastructure.Configuration;

namespace TheNextLevel.Infrastructure.Data;

public class TursoClient
{
    private readonly HttpClient _httpClient;
    private readonly string _databaseUrl;
    private readonly string _authToken;

    public TursoClient(TursoConfiguration config)
    {
        _databaseUrl = config.DatabaseUrl.TrimEnd('/');
        _authToken = config.AuthToken;

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_databaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _authToken);
    }

    public async Task<TursoResponse> ExecuteAsync(string sql, params object[] parameters)
    {
        return await ExecuteBatchAsync(new[] { new TursoStatement(sql, parameters) });
    }

    public async Task<TursoResponse> ExecuteBatchAsync(IEnumerable<TursoStatement> statements)
    {
        var requestItems = new List<TursoPipelineRequestItem>();

        // Add execute request for each statement
        foreach (var statement in statements)
        {
            requestItems.Add(new TursoPipelineRequestItem
            {
                Type = "execute",
                Stmt = statement.ToApiStatement()
            });
        }

        // Add close request at the end
        requestItems.Add(new TursoPipelineRequestItem
        {
            Type = "close"
        });

        var request = new TursoPipelineRequest
        {
            Requests = requestItems.ToArray()
        };

        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/v2/pipeline", content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var pipelineResponse = JsonSerializer.Deserialize<TursoPipelineResponse>(responseJson, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        if (pipelineResponse?.Results == null || pipelineResponse.Results.Length == 0)
        {
            throw new InvalidOperationException("No results returned from Turso");
        }

        var executeResult = pipelineResponse.Results[0];

        if (executeResult.Type == "error")
        {
            throw new InvalidOperationException($"Turso error: {executeResult.Error?.Message ?? "Unknown error"}");
        }

        return executeResult.Response ?? new TursoResponse();
    }

    public async Task<TursoResponse> QueryAsync(string sql, params object[] parameters)
    {
        return await ExecuteAsync(sql, parameters);
    }
}

public class TursoStatement
{
    public string Sql { get; set; }
    public object[] Parameters { get; set; }

    public TursoStatement(string sql, params object[] parameters)
    {
        Sql = sql;
        Parameters = parameters ?? Array.Empty<object>();
    }

    public TursoApiStatement ToApiStatement()
    {
        return new TursoApiStatement
        {
            Sql = Sql,
            Args = Parameters.Length > 0 ? Parameters.Select(p => new TursoParameter
            {
                Type = GetParameterType(p),
                Value = p?.ToString()
            }).ToArray() : null
        };
    }

    private static string GetParameterType(object? param)
    {
        return param switch
        {
            null => "null",
            int or long or short or byte => "integer",
            float or double or decimal => "float",
            bool => "integer",
            byte[] => "blob",
            _ => "text"
        };
    }
}

// Request Models
public class TursoPipelineRequest
{
    [JsonPropertyName("requests")]
    public TursoPipelineRequestItem[] Requests { get; set; } = Array.Empty<TursoPipelineRequestItem>();
}

public class TursoPipelineRequestItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("stmt")]
    public TursoApiStatement? Stmt { get; set; }
}

public class TursoApiStatement
{
    [JsonPropertyName("sql")]
    public string Sql { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public TursoParameter[]? Args { get; set; }
}

public class TursoParameter
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

// Response Models
public class TursoPipelineResponse
{
    [JsonPropertyName("results")]
    public TursoPipelineResult[] Results { get; set; } = Array.Empty<TursoPipelineResult>();
}

public class TursoPipelineResult
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("response")]
    public TursoResponse? Response { get; set; }

    [JsonPropertyName("error")]
    public TursoError? Error { get; set; }
}

public class TursoResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("results")]
    public TursoQueryResults? Results { get; set; }
}

public class TursoQueryResults
{
    [JsonPropertyName("columns")]
    public string[] Columns { get; set; } = Array.Empty<string>();

    [JsonPropertyName("rows")]
    public JsonElement[][] Rows { get; set; } = Array.Empty<JsonElement[]>();

    [JsonPropertyName("affected_row_count")]
    public int AffectedRowCount { get; set; }

    [JsonPropertyName("last_insert_rowid")]
    public string? LastInsertRowId { get; set; }
}

public class TursoError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
