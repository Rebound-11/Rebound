using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using Microsoft.UI.Xaml;

#nullable enable

namespace Rebound.Helpers.Services;

public class SingleInstanceLaunchEventArgs(string arguments, bool isFirstLaunch) : EventArgs
{
    public string Arguments { get; private set; } = arguments;
    public bool IsFirstLaunch { get; private set; } = isFirstLaunch;
}

public partial class SingleInstanceAppService(string appId) : IDisposable
{
    private readonly string _mutexName = "MUTEX_" + appId;
    private readonly string _pipeName = "PIPE_" + appId;
    private readonly object _namedPiperServerThreadLock = new();

    private bool _isDisposed = false;
    private bool _isFirstInstance;

    private Mutex? _mutexApplication;
    private NamedPipeServerStream? _namedPipeServerStream;

    public event EventHandler<SingleInstanceLaunchEventArgs>? Launched;

    public void Launch(string arguments)
    {
        if (string.IsNullOrEmpty(arguments))
        {
            // The arguments from LaunchActivatedEventArgs can be empty, when
            // the user specified arguments (e.g. when using an execution alias). For this reason we
            // alternatively check for arguments using a different API.
            var argList = System.Environment.GetCommandLineArgs();
            if (argList.Length > 1)
            {
                arguments = string.Join(' ', argList.Skip(1));
            }
        }

        if (IsFirstApplicationInstance())
        {
            CreateNamedPipeServer();
            Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(arguments, isFirstLaunch: true));
        }
        else
        {
            SendArgumentsToRunningInstance(arguments);

            Application.Current.Exit();
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _namedPipeServerStream?.Dispose();
        _mutexApplication?.Dispose();
    }

    private bool IsFirstApplicationInstance()
    {
        // Allow for multiple runs but only try and get the mutex once
        _mutexApplication ??= new Mutex(true, _mutexName, out _isFirstInstance);

        return _isFirstInstance;
    }

    /// <summary>
    /// Starts a new pipe server if one isn't already active.
    /// </summary>
    private void CreateNamedPipeServer()
    {
        _namedPipeServerStream = new NamedPipeServerStream(
            _pipeName, PipeDirection.In,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            inBufferSize: 0,
            outBufferSize: 0);

        _ = _namedPipeServerStream.BeginWaitForConnection(OnNamedPipeServerConnected, _namedPipeServerStream);
    }

    private void SendArgumentsToRunningInstance(string arguments)
    {
        try
        {
            using var namedPipeClientStream = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
            namedPipeClientStream.Connect(3000); // Maximum wait 3 seconds
            using var sw = new StreamWriter(namedPipeClientStream);
            sw.Write(arguments);
            sw.Flush();
        }
        catch (Exception)
        {
            // Error connecting or sending
        }
    }

    private void OnNamedPipeServerConnected(IAsyncResult asyncResult)
    {
        try
        {
            if (_namedPipeServerStream == null)
            {
                return;
            }

            _namedPipeServerStream.EndWaitForConnection(asyncResult);

            // Read the arguments from the pipe
            lock (_namedPiperServerThreadLock)
            {
                using var sr = new StreamReader(_namedPipeServerStream);
                var args = sr.ReadToEnd();
                Launched?.Invoke(this, new SingleInstanceLaunchEventArgs(args, isFirstLaunch: false));
            }
        }
        catch (ObjectDisposedException)
        {
            // EndWaitForConnection will throw when the pipe closes before there is a connection.
            // In that case, we don't create more pipes and just return.
            // This will happen when the app is closed and therefor the pipe is closed as well.
            return;
        }
        catch (Exception)
        {
            // ignored
        }
        finally
        {
            // Close the original pipe (we will create a new one each time)
            _namedPipeServerStream?.Dispose();
        }

        // Create a new pipe for next connection
        CreateNamedPipeServer();
    }
}
