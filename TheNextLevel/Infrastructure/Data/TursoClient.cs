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

        // Retry with exponential backoff
        const int maxRetries = 3;
        var retryDelays = new[] {
            TimeSpan.FromMilliseconds(500),  // First retry after 500ms
            TimeSpan.FromMilliseconds(1000), // Second retry after 1s
            TimeSpan.FromMilliseconds(2000)  // Third retry after 2s
        };

        Exception? lastException = null;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            StringContent? content = null;
            try
            {
                content = new StringContent(json, Encoding.UTF8, "application/json");
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
            catch (HttpRequestException ex)
            {
                lastException = ex;

                if (attempt < maxRetries)
                {
                    Console.WriteLine($"Turso request failed (attempt {attempt + 1}/{maxRetries + 1}): {ex.Message}. Retrying in {retryDelays[attempt].TotalMilliseconds}ms...");
                    await Task.Delay(retryDelays[attempt]);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                lastException = ex;

                if (attempt < maxRetries)
                {
                    Console.WriteLine($"Turso request timed out (attempt {attempt + 1}/{maxRetries + 1}). Retrying in {retryDelays[attempt].TotalMilliseconds}ms...");
                    await Task.Delay(retryDelays[attempt]);
                }
            }
            finally
            {
                content.Dispose();
            }
        }

        throw new InvalidOperationException($"Failed to execute Turso request after {maxRetries + 1} attempts", lastException);
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

    [JsonPropertyName("result")]
    public TursoQueryResults? Result { get; set; }
}

public class TursoQueryResults
{
    [JsonPropertyName("cols")]
    public TursoColumn[] Cols { get; set; } = Array.Empty<TursoColumn>();

    [JsonPropertyName("rows")]
    public TursoValue[][] Rows { get; set; } = Array.Empty<TursoValue[]>();

    [JsonPropertyName("affected_row_count")]
    public int AffectedRowCount { get; set; }

    [JsonPropertyName("last_insert_rowid")]
    public string? LastInsertRowId { get; set; }
}

public class TursoColumn
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("decltype")]
    public string? DeclType { get; set; }
}

public class TursoValue
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    [JsonPropertyName("base64")]
    public string? Base64 { get; set; }

    public string GetStringValue()
    {
        if (Type == "null")
            return string.Empty;

        if (Type == "text" || Type == "integer")
            return Value?.GetString() ?? string.Empty;

        if (Type == "float")
            return Value?.GetDouble().ToString() ?? string.Empty;

        if (Type == "blob")
            return Base64 ?? string.Empty;

        return string.Empty;
    }

    public int GetInt32Value()
    {
        if (Type == "integer")
        {
            var strValue = Value?.GetString() ?? "0";
            return int.TryParse(strValue, out var result) ? result : 0;
        }

        if (Type == "float")
            return (int)(Value?.GetDouble() ?? 0);

        return 0;
    }

    public bool GetBoolValue()
    {
        if (Type == "integer")
        {
            var strValue = Value?.GetString() ?? "0";
            return strValue == "1";
        }

        return false;
    }

    public int? GetNullableInt32Value()
    {
        if (Type == "null")
            return null;

        return GetInt32Value();
    }
}

public class TursoError
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
