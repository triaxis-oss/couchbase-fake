namespace Couchbase.Fake.Controllers;

public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;

    public QueryController(ILogger<QueryController> logger)
    {
        _logger = logger;
    }

    [HttpPost("/query")]
    public async Task<IActionResult> Index([FromBody] QueryDto query)
    {
        _logger.LogWarning("Query: [{Client}] {Statement}", query.ClientContextId, query.Statement);
        await Task.Delay(10);
        return Ok(new QueryResultDto<object>
        {
            Status = "success",
            ClientContextId = query.ClientContextId,
            Results = Enumerable.Empty<object>(),
        });
    }
}
