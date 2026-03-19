using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AuthBI.Infrastructure.WsManager
{
    public class WorkerConnectionManager
    {
        // ClientId → conexão ativa
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

        // QueryId → TaskCompletionSource aguardando resposta
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonElement>> _pending = new();

        // ── Gerenciamento de conexões ─────────────────────────────────────────

        public void Register(string clientId, WebSocket ws)
            => _connections[clientId] = ws;

        public void Unregister(string clientId)
            => _connections.TryRemove(clientId, out _);

        public bool IsConnected(string clientId)
            => _connections.TryGetValue(clientId, out var ws)
               && ws.State == WebSocketState.Open;

        public IEnumerable<string> ConnectedClients
            => _connections.Keys;

        // ── Envio de query e espera do resultado ─────────────────────────────

        public async Task<JsonElement> ExecuteQueryAsync(
            string clientId,
            string sql,
            TimeSpan? timeout = null)
        {
            if (!_connections.TryGetValue(clientId, out var ws) || ws.State != WebSocketState.Open)
                throw new InvalidOperationException($"Worker '{clientId}' não está conectado.");

            var queryId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<JsonElement>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            _pending[queryId] = tcs;

            try
            {
                var payload = JsonSerializer.Serialize(new { query_id = queryId, sql });
                var bytes = Encoding.UTF8.GetBytes(payload);
                await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

                using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(30));
                cts.Token.Register(() => tcs.TrySetCanceled());

                return await tcs.Task;
            }
            finally
            {
                _pending.TryRemove(queryId, out _);
            }
        }

        // ── Chamado pelo WebSocketController ao receber mensagem do Worker ────

        public void HandleIncoming(string rawJson)
        {
            using var doc = JsonDocument.Parse(rawJson);
            var root = doc.RootElement;

            // Ignora pings
            if (root.TryGetProperty("type", out var type) && type.GetString() == "ping")
                return;

            if (!root.TryGetProperty("query_id", out var queryIdEl))
                return;

            var queryId = queryIdEl.GetString()!;

            if (_pending.TryGetValue(queryId, out var tcs))
                tcs.TrySetResult(root.Clone()); // .Clone() porque o doc vai ser descartado
        }
    }
}
