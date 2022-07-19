using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Threading;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Buffers;

namespace PowerOverlay.IPC;

public class NamedPipeServer : IDisposable
{
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public static string PipeName => $"PowerOverlay.{System.Diagnostics.Process.GetCurrentProcess().SessionId}";


    public NamedPipeServer()
    {
        Task.Factory.StartNew(Listen, cancellationTokenSource.Token);
    }

    private async void Listen()
    {
        while (true)
        {
            if (cancellationTokenSource.IsCancellationRequested) return;
            using var server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Message,
                PipeOptions.CurrentUserOnly);

            await server.WaitForConnectionAsync(cancellationTokenSource.Token);
            if (cancellationTokenSource.IsCancellationRequested) return;

            if (!server.IsConnected) continue;
            try
            {
                ProcessMessage(server);
            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling IPC message: {e.Message}");
            }
        }
    }

    private void ProcessMessage(NamedPipeServerStream server)
    {
        Span<byte> buffer = stackalloc byte[2048];

        int readLength = server.Read(buffer);

        if (!server.IsMessageComplete) return; // invalid message

        var textBuf = buffer.Slice(0, readLength);

        var msg = Message.FromJson(textBuf);

        App.Current.Dispatcher.Invoke(() => ((PowerOverlay.App)App.Current).HandleIPC(msg));
    }

    public void Dispose()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }
}

public class NamedPipeClient
{
    private const int ConnectTimeoutMilliseconds = 50;

    public static bool IsServerAvailable()
    {
        using var client = new NamedPipeClientStream(".", NamedPipeServer.PipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly, System.Security.Principal.TokenImpersonationLevel.Identification);
        try
        {
            client.Connect(ConnectTimeoutMilliseconds);
            return true;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to connect to named pipe: {e.Message}");
            return false;
        }
    }

    public static bool SendMessage(Message msg)
    {
        using var client = new NamedPipeClientStream(".", NamedPipeServer.PipeName, PipeDirection.Out, PipeOptions.CurrentUserOnly, System.Security.Principal.TokenImpersonationLevel.Identification);
        try
        {
            client.Connect(ConnectTimeoutMilliseconds);
            client.Write(msg.WriteJson());
            client.WaitForPipeDrain();
            return true;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send message to named pipe: {e.Message}");
            return false;
        }
    }
}

public enum MessageAction
{
    Activate = 0,
    ActivateMenu = 1,
}
public class Message
{
    public MessageAction Action = MessageAction.Activate;
    public string TargetMenu = String.Empty;

    public ReadOnlySpan<byte> WriteJson()
    {
        JsonObject o = new JsonObject();
        o[nameof(Action)] = Action.ToString();
        switch (Action)
        {
            case MessageAction.Activate: break;
            case MessageAction.ActivateMenu:
                o[nameof(TargetMenu)] = TargetMenu;
                break;
        }
        var buffer = new ArrayBufferWriter<byte>(2048);
        using var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions() { Indented = false });
        o.WriteTo(writer);
        writer.Flush();
        return buffer.WrittenSpan;
    }

    public static Message FromJson(Span<byte> rawData)
    {
        var reader = new Utf8JsonReader(rawData);
        var n = JsonNode.Parse(ref reader, new JsonNodeOptions() { PropertyNameCaseInsensitive = true });
        var o = n.AsObject();

        var result = new Message();
        result.Action = Enum.Parse<MessageAction>(o[nameof(Action)]!.AsValue().GetValue<string>());
        switch (result.Action)
        {
            case MessageAction.Activate: break;
            case MessageAction.ActivateMenu:
                {
                    result.TargetMenu = o[nameof(TargetMenu)]!.AsValue().GetValue<string>();
                }
                break;
        }
        return result;
    }
}