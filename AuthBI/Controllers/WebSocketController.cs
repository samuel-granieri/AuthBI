using AuthBI.Infrastructure.WsManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace AuthBI.Controllers
{
    [Route("ws")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly WorkerConnectionManager _manager;
        private readonly ILogger<WebSocketController> _logger;

        public WebSocketController(
            WorkerConnectionManager manager,
            ILogger<WebSocketController> logger)
        {
            _manager = manager;
            _logger = logger;
        }

        [Route("worker")]
        public async Task Worker()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            // ClientId vem no header — o Worker envia X-Client-Id
            var clientId = HttpContext.Request.Headers["X-Client-Id"].FirstOrDefault()
                           ?? HttpContext.Connection.RemoteIpAddress?.ToString()
                           ?? "unknown";

            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _manager.Register(clientId, ws);
            _logger.LogInformation("[WS] Worker '{ClientId}' conectado.", clientId);

            try
            {
                await ReceiveLoopAsync(ws, clientId);
            }
            finally
            {
                _manager.Unregister(clientId);
                _logger.LogInformation("[WS] Worker '{ClientId}' desconectado.", clientId);
            }
        }

        private async Task ReceiveLoopAsync(WebSocket ws, string clientId)
        {
            var buffer = new byte[128 * 1024];

            while (ws.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                try
                {
                    result = await ws.ReceiveAsync(buffer, HttpContext.RequestAborted);
                }
                catch (OperationCanceledException) { break; }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await ws.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    break;
                }

                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _manager.HandleIncoming(json);
            }
        }
    }
}
