using System.Text.Json;
using AuthBI.Exceptions;
using AuthBI.Infrastructure.WsManager;

namespace AuthBI.Services
{
    public class QueryService
    {
        private readonly WorkerConnectionManager _manager;

        public QueryService(WorkerConnectionManager manager)
            => _manager = manager;

        /// <summary>
        /// Executa um SQL no banco do cliente e retorna as linhas como lista de dicionários.
        /// </summary>
        public async Task<List<Dictionary<string, object?>>> ExecuteAsync(
            string clientId,
            string sql,
            TimeSpan? timeout = null)
        {
            var result = await _manager.ExecuteQueryAsync(clientId, sql, timeout);

            if (result.TryGetProperty("status", out var status) && status.GetString() == "error")
            {
                var msg = result.GetProperty("message").GetString();
                throw new WorkerQueryException($"Erro no Worker: {msg}");
            }

            var columns = result.GetProperty("columns")
                .EnumerateArray()
                .Select(c => c.GetString()!)
                .ToList();

            var rows = new List<Dictionary<string, object?>>();

            foreach (var rowEl in result.GetProperty("rows").EnumerateArray())
            {
                var row = new Dictionary<string, object?>();
                foreach (var col in columns)
                {
                    if (rowEl.TryGetProperty(col, out var val))
                    {
                        row[col] = val.ValueKind switch
                        {
                            JsonValueKind.String => val.GetString(),
                            JsonValueKind.Number => val.TryGetInt64(out var i) ? i : val.GetDouble(),
                            JsonValueKind.True => true,
                            JsonValueKind.False => false,
                            JsonValueKind.Null => null,
                            _ => val.ToString()
                        };
                    }
                }
                rows.Add(row);
            }

            return rows;
        }
    }
}