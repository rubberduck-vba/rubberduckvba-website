using Microsoft.Extensions.Options;
using rubberduckvba.Server.Model;

namespace rubberduckvba.Server.Data;

public class HangfireJobStateRepository : QueryableRepository<HangfireJobState>
{
    public HangfireJobStateRepository(IOptions<ConnectionSettings> settings)
        : base(settings) { }

    protected override string SelectSql => "SELECT [JobName],[LastJobId],[CreatedAt],[NextExecution],[StateName],[StateTimestamp] FROM [dbo].[HangfireJobState];";
}
