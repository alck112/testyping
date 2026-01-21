using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ScjWin11UI.Models;

namespace ScjWin11UI.Services;

/// <summary>
/// Named pipe server for IME UI.
/// UI is the server, IME engine connects as client.
/// Messages are newline-delimited JSON (NDJSON).
/// </summary>
public sealed class ImePipeServer : IDisposable
{
    private readonly string _pipeName;
    private readonly Action<ImeToUiMessage> _onMessage;

    private CancellationTokenSource? _cts;
    private Task? _loopTask;

    private NamedPipeServerStream? _pipe;
    private StreamWriter? _writer;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public ImePipeServer(string pipeName, Action<ImeToUiMessage> onMessage)
    {
        _pipeName = pipeName;
        _onMessage = onMessage;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _loopTask = Task.Run(() => AcceptLoopAsync(_cts.Token));
    }

    public async Task SendAsync(UiToImeMessage msg)
    {
        try
        {
            if (_writer is null) return;
            var json = JsonSerializer.Serialize(msg, JsonOptions);
            await _writer.WriteLineAsync(json);
            await _writer.FlushAsync();
        }
        catch
        {
            // Ignore send failures (engine might not be connected).
        }
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    pipeName: _pipeName,
                    direction: PipeDirection.InOut,
                    maxNumberOfServerInstances: 1,
                    transmissionMode: PipeTransmissionMode.Byte,
                    options: PipeOptions.Asynchronous);

                _pipe = pipe;

                await pipe.WaitForConnectionAsync(ct);

                using var reader = new StreamReader(pipe);
                _writer = new StreamWriter(pipe) { AutoFlush = true };

                while (!ct.IsCancellationRequested && pipe.IsConnected)
                {
                    var line = await reader.ReadLineAsync();
                    if (line is null) break;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    ImeToUiMessage? msg = null;
                    try
                    {
                        msg = JsonSerializer.Deserialize<ImeToUiMessage>(line, JsonOptions);
                    }
                    catch
                    {
                        // Ignore malformed JSON.
                    }

                    if (msg is not null && !string.IsNullOrWhiteSpace(msg.Type))
                        _onMessage(msg);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Back-off briefly, then retry accept loop.
                await Task.Delay(200, ct);
            }
            finally
            {
                _writer = null;
                _pipe = null;
            }
        }
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _pipe?.Dispose(); } catch { }
        try { _cts?.Dispose(); } catch { }
    }
}
