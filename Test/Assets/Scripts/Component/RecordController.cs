using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using OBSWebsocketDotNet;

namespace Component
{
    public static class RecordController
    {
        private static readonly OBSWebsocket _websocket = new();
        private static bool _isInitialize;

        private static string _host;
        private static string _port;
        private static string _password;

        private static CancellationTokenSource _cancellationTokenSource = new();

        public static bool Initialize(string host, string port, string password)
        {
            if (_isInitialize) return false;
            _host = host;
            _port = port;
            _password = password;
            _isInitialize = true;
            return true;
        }

        public static string Host
        {
            get { return _host; }
        }
        public static string Port
        {
            get { return _port; }
        }
        public static string Password
        {
            get { return _password; }
        }

        async public static UniTask OBSConnect(CancellationToken token)
        {
            if (!_isInitialize) return;
            _websocket.ConnectAsync("ws://" + Host + ":" + Port, Password);
            var source = CancellationTokenSource.CreateLinkedTokenSource(token, _cancellationTokenSource.Token);
            var ct = source.Token;
            Timeout(ct);
            var canceled = await UniTask.WaitUntil(() => _websocket.IsConnected, PlayerLoopTiming.Update, ct).SuppressCancellationThrow();
            if (canceled) return;
        }

        public static void OBSDisconnect()
        {
            if (_websocket.IsConnected) _websocket.Disconnect();
        }

        public static bool OBSIsConnected()
        {
            return _websocket.IsConnected;
        }

        public static void OBSRecordStart()
        {
            if (_websocket.IsConnected) _websocket.StartRecord();
        }

        public static string OBSRecordStop()
        {
            try
            {
                if (_websocket.IsConnected) return _websocket.StopRecord();
            }
            catch (ErrorResponseException)
            {

            }
            return "録画データなし";
        }

        async static void Timeout(CancellationToken token)
        {
            var canceled = await UniTask.Delay(TimeSpan.FromSeconds(10), true, PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (canceled) return;
            _cancellationTokenSource.Cancel();
        }

        public static void OBSSetCurrentProgromScene(string scene)
        {
            if (!_websocket.IsConnected) return;
            if (_websocket.GetCurrentProgramScene() == scene) return;

            _websocket.SetCurrentProgramScene(scene);
        }
    }
}