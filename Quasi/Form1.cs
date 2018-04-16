using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Diagnostics;

using Sipek.Common;
using Sipek.Common.CallControl;
using Sipek.Sip;

using iLLi.VOIP;
using System.Security.Principal;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;

namespace Quasi
{
    public partial class Form1 : Form
    {
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;
        // Needed to update the form with messages from another thread
        private delegate void UpdateLogCallback(string strMessage);
        // Needed to set the form to a "disconnected" state from another thread
        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool applicationClosing = false;
        private bool _connected;
        private bool Connected
        {
            get 
            {
                return _connected;
            }
            set 
            {
                _connected = value;

                if (!applicationClosing && !_connected)
                {
                    InitializeConnection();
                }
            }
        }

        int metkaop = 1; //метка для проверки запущен ли оператор
        bool oncall = false;

        System.Media.SoundPlayer player2 = new System.Media.SoundPlayer(); //плеер для звука входящего звонка

        bool callResultFormshowed = false;

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(HandleRef hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr handle, int cmdShow);
        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr handle);

        #region SIPModule
        Listener listener = new Listener("127.0.0.1", 4505);
        #endregion

        #region properties
        CCallManager CallManager
        {
            get
            {
                return CCallManager.Instance;
            }
        }

        private rc_PhoneCfg v_hPhoneCfg = new rc_PhoneCfg();
        internal rc_PhoneCfg Config
        {
            get
            {
                return v_hPhoneCfg;
            }
        }

        private IStateMachine v_hCall = null;
        #endregion

        private List<CallInfo> CallList = new List<CallInfo>();
        private List<CallInfo> ActiveCallList = new List<CallInfo>();
        public Form1()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();

            CallManager.CallStateRefresh += new DCallStateRefresh(CallManager_CallStateRefresh);
            CallManager.IncomingCallNotification += new DIncomingCallNotification(CallManager_IncomingCallNotification);
            pjsipRegistrar.Instance.AccountStateChanged += new DAccountStateChanged(Instance_AccountStateChanged);

            CallManager.StackProxy = pjsipStackProxy.Instance;
            Config.Accounts.Clear();
            List<string> codecs = Config.CodecList;

            listener.onLicenseRequest += new Listener.GetLicenseInfoEvent(GetLicenseInfo);
            listener.onRegisterRequest += new Listener.RegisterEvent(Register);
            listener.onCallAnswer += new Listener.CallAnswerEvent(CallAnswer);
            listener.onCallBye += new Listener.CallByeEvent(CallBye);
        }

        public void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                // Closes the connections, streams, etc.
                applicationClosing = true;
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        private void InitializeConnection()
        {
            // Parse the IP address from the TextBox into an IPAddress object
            ipAddr = IPAddress.Parse(System.Configuration.ConfigurationSettings.AppSettings["QuasiServer"]);
            // Start a new TCP connections to the chat server
            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr, Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["QuasiServerPort"]));
            Connected = true;
            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(RegisterInfo.ADLogin);
            swSender.Flush();

            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        public class Message
        {
            public string To { get; set; }
            public string Phone { get; set; }
        }

        private void UpdateLog(string strMessage)
        {
            try
            {
                var msg = (Message)new JavaScriptSerializer().Deserialize(strMessage, typeof(Message));

                if (!String.IsNullOrEmpty(msg.Phone))
                {
                    comboBox1.Text = msg.Phone.Trim();
                    callButton_Click(this, new EventArgs());
                }
            }
            catch
            { 
                // Ничего не делаем
            }
        }

        private void ReceiveMessages()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string ConResponse = srReceiver.ReadLine();

            if (ConResponse[0] == '1')
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Успешно подключено!" });
            }
            else
            {
                string Reason = "Не подлючено: ";
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                return;
            }

            while (Connected)
            {
                try
                {
                    this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });
                }
                catch 
                { 
                    // Тут тоже нихуя не делаем
                    Connected = false;
                }
            }
        }

        private void CloseConnection(string Reason)
        {
            Connected = false;
            swSender.Close();
            srReceiver.Close();
            tcpServer.Close();
        }

        public delegate void StatusChange(string status);
        public delegate void RejectButtonEnabled(bool enabled);
        public delegate void CallState();
        public delegate void Journal();
        public delegate void DAnswer();
        public void ChangeStatus(string status)
        {
            statusStrip1.Items["pStatusLabel"].Text = status;
        }
        public void EnabledRejectButton(bool enabled)
        {
            //метка
            if (ActiveCallList.Count > 0)
                enabled = true;
            else
                enabled = false;

            rejectButton.Enabled = enabled;
            CallStateStatusChange();
            HoldButtonEnabled(enabled);
        }
        public void CallStateStatusChange()
        {
            for (int i = 0; i < ActiveCallList.Count; i++)
            {
                try
                {
                    //задаёт таймер для звонка???
                    IStateMachine sm = CallManager.getCall(ActiveCallList[i].SessionID);
                    int t;
                    if(sm != null && !sm.IsNull && sm.StateId == EStateId.ACTIVE)
                        //задать таймер, когда звонок принят
                        curCallInfoDG.Rows[i].Cells[1].Value = sm.RuntimeDuration.ToString(@"hh\:mm\:ss");
                    else
                        //задать таймер, когда звонок ЕЩЁ НЕ принят
                        curCallInfoDG.Rows[i].Cells[1].Value = ActiveCallList[i].Duration.ToString(@"hh\:mm\:ss");                       
                }
                catch { }
            }

            if (ActiveCallList.Count == 0)
                timer1.Enabled = false;
            else
                timer1.Enabled = true;

            curCallInfoDG.Refresh();
        }
        private void HoldButtonEnabled(bool enabled)
        {
            holdButton.Enabled = enabled;
        }
        public void AddJournal()
        {
            dataGridView2.Rows.Clear();

            //for (int i = CallList.Count - 1; i >= 0; i--)
            //{
            //}
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            LoginForm login = new LoginForm();
            login.ShowDialog();
             */

            RegisterInfo.Login = System.Configuration.ConfigurationSettings.AppSettings["SipUser"];
            RegisterInfo.Password = System.Configuration.ConfigurationSettings.AppSettings["SipUserPsw"];
            RegisterInfo.DisplayName = "Имя для отображения";

            if (RegisterInfo.Login == "")
            {
                Close();
                return;
            }

            if (!string.IsNullOrEmpty(RegisterInfo.ADLogin))
            {
                try
                {
                    InitializeConnection();
                }
                catch
                { 
                    //тут нужно добавить, что если не получилось подключиться, то жопа и проинформировать об этом пользователя в статусбаре
                    Connected = false;
                }
            }

            this.Text = "КвазиФон 1.7 - " + RegisterInfo.DisplayName + "(" + RegisterInfo.Login + ")";

            //Инициализация СИП-библиотеки и подключение к серверу
            rc_AccountCfg config = new rc_AccountCfg();
            config.AAFlag = false;
            config.AccountName = RegisterInfo.Login;
            config.UserName = RegisterInfo.Login;
            config.Id = RegisterInfo.Login;
            config.DisplayName = RegisterInfo.DisplayName;
            config.HostName = RegisterInfo.SIPServerIP + ":" + RegisterInfo.SIPServerPort.ToString();
            config.Password = RegisterInfo.Password;
            config.ProxyAddress = "";
            config.Enabled = true;

            Config.Accounts.Add(config);
            CallManager.Config = Config;
            pjsipStackProxy.Instance.Config = Config;
            pjsipRegistrar.Instance.Config = Config;
            CallManager.Initialize();

            pjsipRegistrar.Instance.registerAccounts();

            //speakerVolumeRegulator.Maximum = 255;
            //micVolumeRegulator.Maximum = 255;

            listener.Start();

            connectSoundToolStripMenuItem.Checked = Global.ConnectSound;
            disconnectSoundToolStripMenuItem.Checked = Global.DisconnectSound;

            if (Global.Skin == 0)
                phonerLiteToolStripMenuItem.Checked = true;
            else
                portGoToolStripMenuItem.Checked = true;
        }
        #region SIPModule
        public void GetLicenseInfo()
        {
            string message = "{\"Event\":\"LicInfo\",\"LicStatus\":\"licensed\",\"LicCompany\":\"\"Fusion Group\" Ltd.\",\"LicExpires\":\"never\",\"LicHW\":\"92A4-CD2D-C7CD-9B92-EEE5-41B3\"}";
            listener.AddToLog(message);
            listener.SendResponse(message);
        }
        public void CallBye()
        {
            
        }
        public void CallRefer(string forwardTo)
        {
        }
        public void CallAnswer()
        {
            if (InvokeRequired)
                this.BeginInvoke(new DAnswer(Answer), new Object[] { });
            else
                Answer();
        }

        private void Register(string username, string password, string server, int port)
        {
            try
            {
                string message = "{\"Event\":\"Register\",\"Status\":\"OK\",\"Call-ID\":\"1895892098-1771922985-403535780\"}";
                listener.AddToLog(message);
                listener.SendResponse(message);
            }
            catch (Exception ex)
            {
                listener.AddToLog("Ошибка при регистрации" + ex.Message);
            }
        }
        #endregion
        private void Call()
        {
            //метка
            if (comboBox1.Items.IndexOf(comboBox1.Text) < 0)
                comboBox1.Items.Add(comboBox1.Text);

            if (v_hCall != null && v_hCall.StateId == EStateId.ACTIVE)
                CallManager.onUserHoldRetrieve(v_hCall.Session);

            v_hCall = CallManager.createOutboundCall(comboBox1.Text);
            OnStateUpdate(v_hCall.Session);
            comboBox1.Text = "";
        }
        private void Answer()
        {
            //завершаем проигрывание звука поступившего звонка, когда трубка поднята из Operatora
            player2.Stop();

            CallManager.onUserAnswer(v_hCall.Session);
            string message = "{\"Event\":\"CallAnswered\",\"Call-ID\":\"" + v_hCall.Session.ToString() + "@1.1.1.1:5060\",\"From\":\"sip:" + v_hCall.CallingNumber + "@1.1.1.1\",\"To\":\"sip:" + DeliveryRI.Login + "@1.1.1.1:8780\"}";
            listener.SendResponse(message);
            
        }
        private void Reject()
        {
            Invoke(new RejectButtonEnabled(EnabledRejectButton), new object[] { false });
        }
        private void HangUp()
        {
            if (v_hCall != null && !v_hCall.IsNull)
                CallManager.onUserRelease(v_hCall.Session);
            else
                MessageBox.Show("Пусто!!!");

        }
        private void Cancel()
        {
            Invoke(new RejectButtonEnabled(EnabledRejectButton), new object[] { false });
        }

        #region ControlsInteractive

        private void dndButton_Click(object sender, EventArgs e)
        {
            CallManager.Config.DNDFlag = dndButton.Checked;
        }
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            dataGridView3.Rows.Clear();
            DB.GetContacts();

            for (int i = 0; i < Global.Contacts.Count; i++)
            {
                dataGridView3.Rows.Add(Global.Contacts[i].phone, Global.Contacts[i].DisplayName, Global.Contacts[i].comment);
            }
        }
        private void SetCBvalue(string value)
        {
            comboBox1.Text += value;
            comboBox1.Focus();
            comboBox1.SelectionStart = comboBox1.Text.Length;
        }

        //кнопки набора номера
        #region numers-buttons
        private void button1_Click(object sender, EventArgs e)
        {
            SetCBvalue("1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetCBvalue("2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetCBvalue("3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SetCBvalue("4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetCBvalue("5");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SetCBvalue("6");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetCBvalue("7");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetCBvalue("8");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SetCBvalue("9");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SetCBvalue("0");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SetCBvalue("*");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            SetCBvalue("#");
        }
        #endregion
        #endregion

        //кнопка вызова/поднятия трубки
        private void callButton_Click(object sender, EventArgs e)
        {
            player2.Stop();

            if (comboBox1.Text == "" && v_hCall != null && !v_hCall.IsNull && v_hCall.Incoming && v_hCall.StateId == EStateId.INCOMING)
            {
                Answer();
                oncall = true; //метка для блокировки вызовов, когда уже есть активный
                
            }               
            else
                Call();
            //для проблемы с 2-мя звонками сразу
            if (oncall)
            {                
                //restToolStripMenuItem.PerformClick();
                //CallManager.createOutboundCall("*78");
                //DB.SetStatus("REST");
                
                //System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                //player.SoundLocation = "Sounds\\CallWaiting.wav";
                //player.Load();
                //player.Play();
                //player.Stop();
                //onlinebutton.Visible = true;
                //offlinebutton.Visible = false;

                //oncall = false;
            }
            
        }

        //нажатие кнопки завершения вызова
        private void rejectButton_Click(object sender, EventArgs e)
        {
            player2.Stop();

            HangUp();

            //прячем кнопки перевода звонка
            //forwardBtn.Enabled = false;
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < ActiveCallList.Count; i++)
            {
                IStateMachine sm = CallManager.getCall(ActiveCallList[i].SessionID);

                if(sm.StateId == EStateId.ACTIVE)
                    ActiveCallList[i].Duration = sm.RuntimeDuration;
                else
                    ActiveCallList[i].Duration = DateTime.Now - ActiveCallList[i].CallDT;

                Invoke(new CallState(CallStateStatusChange), new object[] { });
            }
        }
        //набор номера выбранного из телефонной книги по двойному клику
        private void dataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                comboBox1.Text = dataGridView3.Rows[e.RowIndex].Cells[0].Value.ToString();
                comboBox1.Items.Add(comboBox1.Text);
                Call();
            }
        }

        private void holdButton_CheckStateChanged(object sender, EventArgs e)
        {
        }

        //набор номера выбранного из журнала по двойному клику 
        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                comboBox1.Text = dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString();
                comboBox1.Items.Add(comboBox1.Text);
                Call();
            }
        }
        
        private void curCallInfoDG_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (curCallInfoDG.SelectedRows.Count > 0)
            {
                
                v_hCall = CallManager.getCall(ActiveCallList[curCallInfoDG.SelectedRows[0].Index].SessionID);

                if (v_hCall != null && !v_hCall.IsNull)
                {
                    if (v_hCall.StateId == EStateId.HOLDING)
                        CallManager.onUserHoldRetrieve(v_hCall.Session);
                    else
                        holdButton.Checked = false;
                }
            }
        }

        public static class IncomingCall
        {
            public static int SessionID;
            public static string Phone;
            public static string DisplayName;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
                forwardBtn.Enabled = true;
        }
        //перевод звонка
        private void forwardBtn_Click(object sender, EventArgs e)
        {
                /*if(v_hCall != null)
                {*/
                    HOMEforwardBtn.Visible = true;
                    GAIDforwardBtn.Visible = true;
                    KAWAIforwardBtn.Visible = true;
                    MAUROforwardBtn.Visible = true;
                    KIOTOforwardBtn.Visible = true;
                    NAHODforwardBtn.Visible = true;
                    USSURforwardBtn.Visible = true;
                    BONUSforwardBtn.Visible = true;
                //}
        }

        //выпадающий список с последними 13-и набранными номерами
        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 13)
            {
                if(comboBox1.Text != "")
                    Call();
            }
        }


        #region callbacks
        void Instance_AccountStateChanged(Int32 iAccountId, Int32 iAccState)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DAccountStateChanged(OnRegistrationUpdate123), new Object[] { iAccountId, iAccState });
            else
                OnRegistrationUpdate123(iAccountId, iAccState);
        }

        void CallManager_CallStateRefresh(Int32 iSessionId)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DCallStateRefresh(OnStateUpdate), new Object[] { iSessionId });
            else
                OnStateUpdate(iSessionId);
        }

        void CallManager_IncomingCallNotification(Int32 iSessionId, String szNumber, String szInfo)
        {
            if (InvokeRequired)
                this.BeginInvoke(new DIncomingCallNotification(OnIncomingCallNotification), new Object[] { iSessionId, szNumber, szInfo });
            else
                OnIncomingCallNotification(iSessionId, szNumber, szInfo);
        }
        #endregion
        #region synchronized callbacks
        private void OnRegistrationUpdate123(Int32 iAccountId, Int32 iAccState)
        {
            statusStrip1.Items["pStatusLabel"].Text = iAccState.ToString();

            if (iAccState == 200)
            {
                Image statusImg = Image.FromFile("Resources\\online.png");
                statusStrip1.Items["pStatusLabel"].Image = statusImg;
                DB.SetStatus("REGISTER");
                DB.SetStatus("ONLINE");
            }
            else
            {
                Image statusImg = Image.FromFile("Resources\\offline.png");
                statusStrip1.Items["pStatusLabel"].Image = statusImg;
            }
        }

        private void OnStateUpdate(Int32 iSessionId)
        {
            IStateMachine sm = CallManager.getCall(iSessionId);
            v_hCall = sm;

            pCallStatusLabel.Text = sm.CallingNumber + " : " + sm.StateId.ToString();

            if (sm.StateId == EStateId.IDLE || sm.StateId == EStateId.NULL)
            {
                if (Global.DisconnectSound)
                {
                    //проигрывание звука окончания звонка
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.SoundLocation = "Sounds\\disconnect.wav";
                    player.Load();
                    player.Play();
                }

                //если в списке есть хотя бы 1 звонок - не принимать звонки
                //метка
                if (curCallInfoDG.SelectedRows.Count > 0)
                    {
                        v_hCall = CallManager.getCall(ActiveCallList[curCallInfoDG.SelectedRows[0].Index].SessionID);                       
                    }

                CallInfo ci = ActiveCallList.Find(p => p.SessionID == iSessionId);

                if (ci != null)
                {
                    if (ci.isIncoming)
                    {
                        listener.SendResponse("{\"Event\":\"Cancel\",\"Call-ID\":\"" + ci.SessionID.ToString() + "@1.1.1.1:5060\",\"From\":\"sip:" + ci.Phone + "@1.1.1.1\",\"To\":\"sip:" + DeliveryRI.Login + "@1.1.1.1:8780\"}");
                    }
                    //добавление звонка в журнал звонков
                    dataGridView2.Rows.Add(ci.Phone, (DateTime.Now - sm.Duration).ToString("dd.MM.yyyy HH:mm:ss"), sm.Duration.ToString(@"hh\:mm\:ss"), "ОК");                    
                    
                    int ind = ActiveCallList.IndexOf(ci);

                    ActiveCallList.Remove(ci);

                    if(ind >= 0)
                        curCallInfoDG.Rows.RemoveAt(ind);
                }                
                //вывод формы о цели звонка
                /*
                if (!callResultFormshowed)
                {           
                    if (ci!=null)
                    {
                        Form3 frm3 = new Form3();
                        frm3.number = ci.Phone;
                        frm3.isIncoming = ci.isIncoming;
                        frm3.duration = (int)ci.Duration.TotalSeconds;
                        frm3.Show();
                        callResultFormshowed = true;
                    }
                  
                }*/
            }
            else
                callResultFormshowed = false;

            if (sm.StateId == EStateId.INCOMING || sm.StateId == EStateId.CONNECTING)
            {
                CallInfo ci = new CallInfo();
                ci.Phone = sm.CallingNumber;
                ci.CallDT = DateTime.Now;
                ci.dt = DateTime.Now;
                ci.SessionID = sm.Session;
                ci.isIncoming = sm.Incoming;
                ActiveCallList.Add(ci);
                curCallInfoDG.Rows.Add(ci.Phone, ci.Duration);

                if (sm.StateId == EStateId.ALERTING)
                    curCallInfoDG.Rows[curCallInfoDG.Rows.Count - 1].Selected = true;
            }
            
            if(sm.StateId == EStateId.ACTIVE)
            {
                if(Global.ConnectSound)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.SoundLocation = "Sounds\\aa.wav";
                    player.Load();
                    player.Play();
                }
            }

            if (sm.IsHeld)
                rejectButton.Checked = true;
            else
                rejectButton.Checked = false;

            //поступил звонок
            if (ActiveCallList.Count > 0)
            {
                rejectButton.Enabled = true;
                holdButton.Enabled = true;
                timer1.Enabled = true;

                
                //метка
                //player2.Stop();
                //if (oncall)
                //{
                //    MessageBox.Show("Active call");
                //    oncall = false;
                //    restToolStripMenuItem.PerformClick();
                //}
                

            }
            else
            {
                rejectButton.Enabled = false;
                holdButton.Enabled = false;
                timer1.Enabled = false;                             
            }

        }
        
        //поступление звонка/входящий звонок
        private void OnIncomingCallNotification(Int32 iSessionId, String szNumber, String szInfo)
        {
            Process[] p = Process.GetProcessesByName("Operator");

            for (int i = 0; i < p.Length; i++)
                try
                {
                    ShowWindow(p[i].MainWindowHandle, 1);
                }
                catch
                { }

            IStateMachine sm = CallManager.getCall(iSessionId);
            sm.CallingNumber = "+7" + sm.CallingNumber;
            string message = "{\"Event\":\"Invite\",\"Call-ID\":\"" + iSessionId.ToString() + "@1.1.1.1:5060\",\"From\":\"sip:" + sm.CallingNumber + "@1.1.1.1\",\"To\":\"sip:" + DeliveryRI.Login + "@1.1.1.1:8780\"}";
            listener.SendResponse(message);

            //звук поступившего звонка           
            player2.SoundLocation = "Sounds\\RingIn.wav";
            player2.Load();
            player2.PlayLooping();           
        }
        #endregion

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(CallManager.IsInitialized)
                CallManager.Shutdown();

            try
            {
                listener.Stop();
            }
            catch { }
        }

        private void autoAnswerToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            CallManager.Config.AAFlag = autoAnswerToolStripMenuItem.Checked;
           
        }

        private void holdButton_Click(object sender, EventArgs e)
        {
            if (v_hCall != null && !v_hCall.IsNull)
            {
                CallManager.onUserHoldRetrieve(v_hCall.Session);
                
            }
            
        }

        //Изменение статуса
        #region status
        private void readyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (readyToolStripMenuItem.Checked)
            {
                dndButton.Checked = false;
                CallManager.Config.DNDFlag = !readyToolStripMenuItem.Checked;
                restToolStripMenuItem.Checked = !readyToolStripMenuItem.Checked;
                internetToolStripMenuItem.Checked = !readyToolStripMenuItem.Checked;
                studyToolStripMenuItem.Checked = !readyToolStripMenuItem.Checked;
                DB.SetStatus("ONLINE");
                System.Threading.Thread.Sleep(1000);
                CallManager.createOutboundCall("*79");

                onlinebutton.Visible = true;
                offlinebutton.Visible = false;

            }
            else
            {
                readyToolStripMenuItem.Checked = true;
            }
            
        }

        private void restToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (restToolStripMenuItem.Checked)
            {
                dndButton.Checked = true;
                CallManager.Config.DNDFlag = restToolStripMenuItem.Checked;
                studyToolStripMenuItem.Checked = !restToolStripMenuItem.Checked;
                internetToolStripMenuItem.Checked = !restToolStripMenuItem.Checked;
                readyToolStripMenuItem.Checked = !restToolStripMenuItem.Checked;
                CallManager.createOutboundCall("*78");
                DB.SetStatus("REST");

                onlinebutton.Visible = false;
                offlinebutton.Visible = true;
            }
            else
            {
                restToolStripMenuItem.Checked = true;
                
            }
            
        }

        private void internetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (internetToolStripMenuItem.Checked)
            {
                dndButton.Checked = true;
                CallManager.Config.DNDFlag = true;
                restToolStripMenuItem.Checked = !internetToolStripMenuItem.Checked;
                readyToolStripMenuItem.Checked = !internetToolStripMenuItem.Checked;
                studyToolStripMenuItem.Checked = !internetToolStripMenuItem.Checked;
                CallManager.createOutboundCall("*78");
                DB.SetStatus("INTERNET");

                onlinebutton.Visible = false;
                offlinebutton.Visible = true;
            }
            else
            {
                internetToolStripMenuItem.Checked = true;
            }
        }

        private void studyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (studyToolStripMenuItem.Checked)
            {
                dndButton.Checked = true;
                CallManager.Config.DNDFlag = studyToolStripMenuItem.Checked;
                restToolStripMenuItem.Checked = !studyToolStripMenuItem.Checked;
                internetToolStripMenuItem.Checked = !studyToolStripMenuItem.Checked;
                readyToolStripMenuItem.Checked = !studyToolStripMenuItem.Checked;
                CallManager.createOutboundCall("*78");
                DB.SetStatus("TRAINING");

                onlinebutton.Visible = false;
                offlinebutton.Visible = true;
            }
            else
            {
                studyToolStripMenuItem.Checked = true;
            }
        }
        #endregion
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Упс...");
        }

        private void curCallInfoDG_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                string opclose = "Operator"; //процесс самого оператора
                string dispclose = "SDispather"; //диспетчера
                string cashclose = "DOSCASH"; //кэш-станция                
				Process[] allproc = Process.GetProcesses(); //список всех запущенных процессов
				for (int i = 0; i < allproc.Length; i++)
				{
					//перебираем все запущенные процессы на наличие opertorа
					if (allproc[i].ProcessName == opclose)
					{
						//убиваем тот процесс на котором остановился цикл (искомый процесс)
						Process target_proc = allproc[i];
						try
						{
							target_proc.Kill();
						}
						catch
						{
							MessageBox.Show("Упс...", "Оператору плохо");
						}

					}
					//перебираем все запущенные процессы на наличие диспетчера
					if (allproc[i].ProcessName == dispclose)
					{
						//убиваем тот процесс на котором остановился цикл (искомый процесс)
						Process target_proc2 = allproc[i];
						try
						{
							target_proc2.Kill();
						}
						catch
						{
							MessageBox.Show("Упс...", "Диспетчеру плохо");
						}
					}
					//перебираем все запущенные процессы на наличие doscash
					if (allproc[i].ProcessName == cashclose)
					{
						//убиваем тот процесс на котором остановился цикл (искомый процесс)
						Process target_proc3 = allproc[i];
						try
						{
							target_proc3.Kill();
						}
						catch
						{
							MessageBox.Show("Упс...", "Cash-станции плохо");
						}

					}
				}

                
            }

            if(RegisterInfo.Login != "")
                DB.SetStatus("OFFLINE");

            Global.SaveSettings();
            //CallManager.createOutboundCall("*45");
        }

        private void connectSoundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Global.ConnectSound = connectSoundToolStripMenuItem.Checked;
        }

        private void disconnectSoundToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Global.DisconnectSound = disconnectSoundToolStripMenuItem.Checked;
        }

        private void portGoToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (portGoToolStripMenuItem.Checked)
                Global.Skin = 1;
        }

        private void phonerLiteToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            if (phonerLiteToolStripMenuItem.Checked)
                Global.Skin = 0;
        }

        private void portGoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Он недоделан и страшный очень. Звонить не умеет еще. Вы точно в этом уверены?", "Вы уверены????!!", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                MessageBox.Show("Вы сами выбрали ЭТО... Звонить не сможете пока. Перезапустите программу для применения изменений");

                if (portGoToolStripMenuItem.Checked)
                    phonerLiteToolStripMenuItem.Checked = false;
            }
        }

        private void phonerLiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (phonerLiteToolStripMenuItem.Checked)
                portGoToolStripMenuItem.Checked = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Process[] p = Process.GetProcessesByName("Operator");
            for (int i = 0; i < p.Length; i++)
                try
                {
                    ShowWindow(p[i].MainWindowHandle, 1);
                }
                catch
                { }
        }
        //перевод звонка на конкретные рестораны VL + Находка + Уссурийск
        #region call forwarding
        private void HOMEforwardBtn_Click(object sender, EventArgs e)
        {
            //string tnumberRest;
            //if (checkDopNumbers.Checked)
            //{
            //    tnumberRest = "";
            //}
            //else
            //{
            //    tnumberRest = "2006";
            //}
            string tnumberRest = "2006";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;  
            comboBox1.Text = "";
        }

        private void GAIDforwardBtn_Click(object sender, EventArgs e)
        {
            //string tnumberRest;
            //if (checkDopNumbers.Checked)
            //{
            //    tnumberRest = "";
            //}
            //else
            //{
            //    tnumberRest = "2007";
            //}
            string tnumberRest = "2007";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }

        private void KAWAIforwardBtn_Click(object sender, EventArgs e)
        {
            string tnumberRest = "2005";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;           
            comboBox1.Text = "";
        }

        private void MAUROforwardBtn_Click(object sender, EventArgs e)
        {
            string tnumberRest = "2008";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }
        //2002 - находка
        private void KIOTOforwardBtn_Click(object sender, EventArgs e)
        {
            string tnumberRest = "2004";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }

        private void NAHODforwardBtn_Click(object sender, EventArgs e)
        {
            string tnumberRest = "2002";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }

        private void BONUSforwardBtn_Click(object sender, EventArgs e)
        {
            //89841953583
            string tnumberRest = "303";
            
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер БК и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }

        private void USSURforwardBtn_Click(object sender, EventArgs e)
        {
            string tnumberRest = "2009";
            //проверяем, есть ли звонок
            //если нет - при нажатии набираем номер ресторана и звоним на него
            if (!rejectButton.Enabled || v_hCall.StateId == EStateId.HOLDING)
            {
                comboBox1.Text = tnumberRest;
                callButton_Click(sender, e);
            }
            else
            {
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            }
            CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
            HOMEforwardBtn.Visible = false;
            GAIDforwardBtn.Visible = false;
            KAWAIforwardBtn.Visible = false;
            MAUROforwardBtn.Visible = false;
            KIOTOforwardBtn.Visible = false;
            NAHODforwardBtn.Visible = false;
            USSURforwardBtn.Visible = false;
            BONUSforwardBtn.Visible = false;
            comboBox1.Text = "";
        }
        #endregion
        //кнопки работы с тел. справочником
        #region tel-buttons
        //Проверка пароля для добавления записи в тел. справочник
        public void addbutton_Click(object sender, EventArgs e)
        {
            Form4 FPass = new Form4();
            FPass.Show();
            FPass.status_action.Text = "1";
            FPass.formopenbutton.Text += " добавления";
        }
        //Проверка пароля для редактирования записи из тел. справочнике
        public void editbutton_Click(object sender, EventArgs e)
        {
            Form4 FPass = new Form4();
            FPass.Show();
            FPass.status_action.Text = "2";
            FPass.formopenbutton.Text += " редактирования";
        }
        //Проверка пароля для удаления записи из тел. справочнике
        public void delbutton_Click(object sender, EventArgs e)
        {
            Form4 FPass = new Form4();
            FPass.Show();
            FPass.status_action.Text = "3";
            FPass.formopenbutton.Text += " удаления";
        }
        #endregion

        private void calltransfer_Click(object sender, EventArgs e)
        {
            if (v_hCall != null && !v_hCall.IsNull && comboBox1.Text!="")
            {
                string tnumberRest = comboBox1.Text;
                CallManager.onUserTransfer(v_hCall.Session, tnumberRest);
                comboBox1.Text = "";
            }
            else
            {
                MessageBox.Show("Введите номер телефона для переадресации", "Ай-яй-яй");
            }
        }
        //кнопки-статусы
        #region status-buttons
        private void onlinebutton_Click(object sender, EventArgs e)
        {
            //старый способ ухода в офлайн
            //readyToolStripMenuItem.Checked = false;
            //restToolStripMenuItem.Checked = true;
            //internetToolStripMenuItem.Checked = false;
            //studyToolStripMenuItem.Checked = false;

            //CallManager.createOutboundCall("*78");
            //DB.SetStatus("REST");

            //новый способ
            restToolStripMenuItem.PerformClick();
            //замена кнопок при изменении статуса
            onlinebutton.Visible = false;
            offlinebutton.Visible = true;
        }

        private void offlinebutton_Click(object sender, EventArgs e)
        {
            //старый способ ухода в онлайн
            //readyToolStripMenuItem_Click(sender, e);
            //readyToolStripMenuItem.Checked = true;
            //restToolStripMenuItem.Checked = false;
            //internetToolStripMenuItem.Checked = false;
            //studyToolStripMenuItem.Checked = false;
            
            //DB.SetStatus("ONLINE");
            //System.Threading.Thread.Sleep(1000);
            //CallManager.createOutboundCall("*79");

            //новый способ
            readyToolStripMenuItem.PerformClick();
            //замена кнопок при изменении статуса
            onlinebutton.Visible = true;
            offlinebutton.Visible = false;
        }
        #endregion
       
        

        //кнопки запуска/остановки оператора
        #region operator-buttons
        private void OperatorButton_Click(object sender, EventArgs e)
        {            
            string opname = "Operator"; //имя искомого процесса
            Process[] allproc = Process.GetProcesses(); //список всех запущенных процессов
            
            for (int i = 0; i < allproc.Count(); i++)
            {
                //перебираем все запущенные процессы на наличие искомого
                if (allproc[i].ProcessName == opname)
                {                 
                    MessageBox.Show("Operator уже запущен!","Запуск оператора");
                    //если предыдущий вызов не нашёл процесс - сделать метку metkaop отличной от 0
                    //чтобы не произошёл повторный запуск программы
                    if (metkaop < 1)
                    {
                        metkaop++;
                    }
                    break;
                }
                //если процесс не найден, т.е. не запущен - передаём метке 0
                else
                {
                    metkaop=0;
                }
                
            }
            //если метка соответствующая - запускаем оператора
            if (metkaop == 0)
            {
                try
                {
                    //Process.Start(@"C:\DELIVERY\Operator\Operator.exe");
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = @"C:\DELIVERY\Operator\Operator.exe";
                    proc.StartInfo.WorkingDirectory = @"C:\DELIVERY\Operator";
                    proc.Start();  
                }
                catch
                {
                    MessageBox.Show("О БОГИ! Оператор НЕ был запущен!!!", "Запуск оператора");
                }
                //finally
                //{
                //    MessageBox.Show("Оператор успешно запущен!", "Запуск оператора");
                //}
                
                

            }
            
        }

        private void CloseOpButton_Click(object sender, EventArgs e)
        {
            //Operator opera
            //SDispather notepad++
            string opclose = "Operator"; //процесс самого оператора
            string dispclose = "SDispather"; //диспетчера
            string cashclose = "DOSCASH"; //кэш-станция
            DialogResult result = MessageBox.Show("Вы уверены, что хотите завершить Operator?", "Завершение оператора", 
               MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Process[] allproc = Process.GetProcesses(); //список всех запущенных процессов

                for (int i = 0; i < allproc.Length; i++)
                {
                    //перебираем все запущенные процессы на наличие opertorа
                    if (allproc[i].ProcessName == opclose)
                    {
                        //убиваем тот процесс на котором остановился цикл (искомый процесс)
                        Process target_proc = allproc[i];
                        try
                        {
                            target_proc.Kill();
                        }
                        catch
                        {
                            MessageBox.Show("Упс...", "Оператору плохо");
                        }
                        
                    }
                    //перебираем все запущенные процессы на наличие диспетчера
                    if (allproc[i].ProcessName == dispclose)
                    {
                        //убиваем тот процесс на котором остановился цикл (искомый процесс)
                        Process target_proc2 = allproc[i];
                        try
                        {
                            target_proc2.Kill();
                        }
                        catch
                        {
                            MessageBox.Show("Упс...", "Диспетчеру плохо");
                        }
                    }
                    //перебираем все запущенные процессы на наличие doscash
                    if (allproc[i].ProcessName == cashclose)
                    {
                        //убиваем тот процесс на котором остановился цикл (искомый процесс)
                        Process target_proc3 = allproc[i];
                        try
                        {
                            target_proc3.Kill();
                        }
                        catch
                        {
                            MessageBox.Show("Упс...", "Cash-станции плохо");
                        }

                    }
                }
                MessageBox.Show("Оператор завершён", "Оператор мёртв...");
            }
        }
        #endregion       

        private void button14_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(Convert.ToString(v_hCall.Session));
            //IStateMachine sm = CallManager.getCall(v_hCall.Session);
            //string id = Convert.ToString(sm.StateId);
            //MessageBox.Show(id);

            //CallManager.createOutboundCall("*78");
            //DB.SetStatus("REST");
              //вызвать нажатие кнопки ухода в офлайн(зелёная), и сделать невидимой красную, а зелёную - видимой
            //if(sm.StateId == EStateId.ACTIVE) - проверить, вроде sm.StateId - id статуса (онлайн/офлайн...)
            //чтобы не играл звук перехода в офлайн/онлайн - ...
            //...ставить метку при активном звонке, потом проверять эту метку на воспроизведении звука "режим офлайн отключен..."
            //бабёнка которая озвучивает - лежит на астериске (найти подключение)
        }

        private void button15_Click(object sender, EventArgs e)
        {            
            //DB.SetStatus("ONLINE");
            //System.Threading.Thread.Sleep(1000);
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //для копирования номера по нажатию на ячейку
            string num="";
            try
            {
                num = Convert.ToString(dataGridView2.Rows[e.RowIndex].Cells[0].Value);
            }
            catch
            {
                
            }
            
            Clipboard.SetData(DataFormats.Text, (Object)num);
        }   
        
        //private void button14_Click(object sender, EventArgs e)
        //{
        //    Form7 FAdd = new Form7();
        //    FAdd.Show();
        //}

        //private void button15_Click(object sender, EventArgs e)
        //{
        //    Form6 FAdd = new Form6();
        //    FAdd.Show();
        //}

        //private void button16_Click(object sender, EventArgs e)
        //{
        //    Form5 FAdd = new Form5();
        //    FAdd.Show();
        //}


    }
}
