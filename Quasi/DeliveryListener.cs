using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization;
using System.IO;
using System.Windows.Forms;

namespace Quasi
{
    public enum RequestResult { register, unregister, licInfo, callAnswer, callRefer, callBye, clientDisconnect, empty, unkown };
    internal class Command
    {
        public string CommandName { get; set; }
    }
    internal class Listener
    {
        public bool listen = true;
        public string forwardTo = "";
        TcpListener listener;
        public enum Logs { Main, Update }

        public void AddToLog(string message)
        {
            FileInfo fi = null;
            DirectoryInfo di = new DirectoryInfo(Application.StartupPath + "\\logs");
            if (!di.Exists)
                di.Create();

            fi = new FileInfo(Application.StartupPath + "\\logs\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            StreamWriter sw = null;
            try
            {
                if (fi.Exists)
                    sw = fi.AppendText();
                else
                    sw = fi.CreateText();

                string append = DateTime.Now.ToString("HH:mm:ss") + " " + message;

                sw.WriteLine(append);

            }
            catch
            { }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        public delegate void RegisterEvent(string username, string password, string server, int port);
        public delegate void GetLicenseInfoEvent();
        public delegate void ConnectClientEvent();
        public delegate void CallAnswerEvent();
        public delegate void CallReferEvent(string forwardTo);
        public delegate void CallByeEvent();
        public delegate void UnRegisterEvent();
        public delegate void ClientDisconnectEvent();
        public delegate void ListenerStoppedEvent();

        public event RegisterEvent onRegisterRequest;
        public event GetLicenseInfoEvent onLicenseRequest;
        public event ConnectClientEvent onClientConnect;
        public event CallAnswerEvent onCallAnswer;
        public event CallReferEvent onCallRefer;
        public event CallByeEvent onCallBye;
        public event UnRegisterEvent onUnRegister;
        public event ClientDisconnectEvent onClientDisconnect;
        public event ListenerStoppedEvent onListenerStopped;

        public TcpClient client = null;
        Thread trd;
        ManualResetEvent stop = new ManualResetEvent(false);
        public NetworkStream io;

        string _host;
        int _port;
        int _readTimeout;
        public void SendResponse(string message)
        {
            try
            {
                if (io != null)
                {
                    message = message + "\r\n";
                    AddToLog(message);
                    //ASCIIEncoding encoding = new ASCIIEncoding();
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] respBytes = encoding.GetBytes(message);
                    io.Write(respBytes, 0, respBytes.Length);
                }
            }
            catch (Exception ex)
            {
                AddToLog("Ошибка при вызове метода SendResponse: " + ex.Message);
            }
        }
        public Listener(string tsIP, int tsPort)
        {
            _host = tsIP;
            _port = tsPort;
            _readTimeout = 0;
        }
        private RequestResult CmdResult(string cmd)
        {
            cmd = cmd.Replace(" ", "");

            RequestResult result = RequestResult.unkown;

            try
            {
                string commandName = "";
                Match m = Regex.Match(cmd, "{\"Command\":\"(?<val>.*?)\"", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (m.Success)
                    commandName = m.Groups["val"].ToString();

                switch (commandName)
                {
                    case "Register":
                        m = Regex.Match(cmd, "{\"Command\":\"Register\",\"Server\":\"(?<server>.*?)\",\"Port\":(?<port>.*?),\"NatAdress\":\"(?<natAdr>.*?)\",\"Login\":\"(?<login>.*?)\",\"Password\":\"(?<password>.*?)\"}", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                        if (m.Success)
                        {
                            DeliveryRI.Login = m.Groups["login"].ToString();
                        }
                        result = RequestResult.register;
                        break;
                    case "LicInfo":
                        result = RequestResult.licInfo;
                        break;
                    case "CallAnswer":
                        result = RequestResult.callAnswer;
                        break;
                    case "CallRefer":
                        break;
                    case "CallBye":
                        result = RequestResult.callBye;
                        break;
                    case "UnRegister":
                        result = RequestResult.unregister;
                        break;
                    case "ClientDisconnect":
                        result = RequestResult.clientDisconnect;
                        break;
                    case "":
                        result = RequestResult.empty;
                        break;
                    default:
                        result = RequestResult.unkown;
                        break;
                }
            }
            catch (Exception ex)
            {
                AddToLog("Ошибка в CmdResult: " + ex.Message);
            }

            return result;
        }
        public void Run()
        {
            AddToLog("Старт SIP");

            try
            {
                ASCIIEncoding encoding = new ASCIIEncoding();
                IPAddress localAddress = IPAddress.Parse(_host);
                listener = new TcpListener(localAddress, _port);
                listener.Start();

                while (true)
                {
                    if (stop.WaitOne(500))
                    {
                        AddToLog("Brake!");
                        break;
                    }

                    if (client == null || !client.Connected)
                    {
                        client = listener.AcceptTcpClient();
                        client.ReceiveTimeout = _readTimeout;
                        io = client.GetStream();
                        AddToLog("Подключение клиента");
                    }

                    try
                    {
                        List<byte> _buf = new List<byte>();
                        byte[] buffer = new byte[128];

                        int cnt = 0;

                        do
                        {
                            cnt = io.Read(buffer, 0, buffer.Length);
                            _buf.AddRange(buffer);
                        }
                        while (cnt >= buffer.Length);

                        string cmd = encoding.GetString(_buf.ToArray());
                        cmd = cmd.Trim(new char[] { ' ', '\0' });

                        AddToLog(String.Format("Входящее сообщение: {0}", cmd));

                        switch (CmdResult(cmd))
                        {
                            case RequestResult.licInfo:
                                AddToLog(String.Format("onLicenseRequest"));
                                onLicenseRequest();
                                break;
                            case RequestResult.register:
                                AddToLog(String.Format("onRegisterRequest"));
                                onRegisterRequest(RegisterInfo.Login, "", "", 0);
                                break;
                            case RequestResult.callAnswer:
                                AddToLog(String.Format("onCallAnswer"));
                                onCallAnswer();
                                break;
                            case RequestResult.callRefer:
                                AddToLog(String.Format("onCallRefer"));
                                onCallRefer(forwardTo);
                                break;
                            case RequestResult.callBye:
                                AddToLog(String.Format("onCallBye"));
                                onCallBye();
                                break;
                            case RequestResult.unregister:
                                AddToLog(String.Format("onUnRegister"));
                                onUnRegister();
                                break;
                            case RequestResult.clientDisconnect:
                                AddToLog(String.Format("onClientDisconnect"));
                                onClientDisconnect();
                                break;
                            case RequestResult.empty:
                                AddToLog(String.Format("EMPTY"));
                                listen = false;
                                break;
                            case RequestResult.unkown:
                                AddToLog(String.Format("UNKOWN"));
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        var socketEx = ex as System.Net.Sockets.SocketException;

                        if (socketEx != null && socketEx.ErrorCode == 10004)
                            break;
                        else
                            continue;
                    }
                }

                if (client != null && client.Connected)
                    client.Close();
            }
            catch (Exception ex)
            {
                AddToLog(String.Format("[Listener {0}:{1}] Ошибка обработки {2}", _host, _port, ex));
                AddToLog(String.Format("Listener {0}:{1} остановлен", _host, _port));
            }
        }
        internal void Start()
        {
            trd = new Thread(Run);
            trd.Start();
        }
        internal void Stop()
        {
            stop.Set();

            listener.Stop();
        }
    }
}
