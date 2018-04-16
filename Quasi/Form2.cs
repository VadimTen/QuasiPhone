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

using Sipek.Common;
using Sipek.Common.CallControl;
using Sipek.Sip;

using iLLi.VOIP;

namespace Quasi
{
    public partial class Form2 : Form
    {
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
        public Form2()
        {
            InitializeComponent();

            CallManager.CallStateRefresh += new DCallStateRefresh(CallManager_CallStateRefresh);
            CallManager.IncomingCallNotification += new DIncomingCallNotification(CallManager_IncomingCallNotification);
            pjsipRegistrar.Instance.AccountStateChanged += new DAccountStateChanged(Instance_AccountStateChanged);

            CallManager.StackProxy = pjsipStackProxy.Instance;
            Config.Accounts.Clear();

            listener.onLicenseRequest += new Listener.GetLicenseInfoEvent(GetLicenseInfo);
            listener.onRegisterRequest += new Listener.RegisterEvent(Register);
            listener.onCallAnswer += new Listener.CallAnswerEvent(CallAnswer);
            listener.onCallBye += new Listener.CallByeEvent(CallBye);
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
                    IStateMachine sm = CallManager.getCall(ActiveCallList[i].SessionID);

                    if(sm != null && !sm.IsNull && sm.StateId == EStateId.ACTIVE)
                        curCallInfoDG.Rows[i].Cells[1].Value = sm.RuntimeDuration.ToString(@"hh\:mm\:ss");
                    else
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

            for (int i = CallList.Count - 1; i >= 0; i--)
            {
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            LoginForm login = new LoginForm();
            login.ShowDialog();

            if (RegisterInfo.Login == "")
            {
                Close();
                return;
            }

            this.Text = "КвазиФон 1.0 - " + RegisterInfo.DisplayName + "(" + RegisterInfo.Login + ")";

            //Инициализация СИП-библиотеки и подключение к серверу
            rc_AccountCfg config = new rc_AccountCfg();
            config.AAFlag = false;
            config.AccountName = RegisterInfo.Login;
            config.UserName = RegisterInfo.Login;
            config.Id = RegisterInfo.Login;
            config.DisplayName = RegisterInfo.DisplayName;
            config.HostName = RegisterInfo.SIPServerIP;
            config.Password = RegisterInfo.Password;
            config.ProxyAddress = "";
            config.Enabled = true;

            Config.Accounts.Add(config);
            CallManager.Config = Config;
            pjsipStackProxy.Instance.Config = Config;
            pjsipRegistrar.Instance.Config = Config;
            CallManager.Initialize();

            pjsipRegistrar.Instance.registerAccounts();

            speakerVolumeRegulator.Maximum = 255;
            micVolumeRegulator.Maximum = 255;

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
        private void micVolumeRegulator_ValueChanged(object sender, EventArgs e)
        {
        }
        private void speakerVolumeRegulator_ValueChanged(object sender, EventArgs e)
        {
        }

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

        private void callButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text == "" && v_hCall != null && !v_hCall.IsNull && v_hCall.Incoming && v_hCall.StateId == EStateId.INCOMING)
                Answer();
            else
                Call();
        }

        private void rejectButton_Click(object sender, EventArgs e)
        {
            HangUp();
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

        private void forwardBtn_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
                if(v_hCall != null)
                    CallManager.onUserTransfer(v_hCall.Session, comboBox1.Text);

            comboBox1.Text = "";
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 13)
            {
                if(comboBox1.Text != "")
                    Call();
            }
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
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
                System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                player.SoundLocation = "Sounds\\disconnect.wav";
                player.Load();
                player.Play();

                /*if (sm.StateId == EStateId.IDLE)
                {
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                    player.SoundLocation = "Sounds\\disconnect.wav";
                    player.Load();
                    player.Play();
                }*/

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

                    dataGridView2.Rows.Add(ci.Phone, (DateTime.Now - sm.Duration).ToString("dd.MM.yyyy HH:mm:ss"), sm.Duration.ToString(@"hh\:mm\:ss"), "ОК");

                    int ind = ActiveCallList.IndexOf(ci);

                    ActiveCallList.Remove(ci);

                    if(ind >= 0)
                        curCallInfoDG.Rows.RemoveAt(ind);
                }
            }

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
                System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                player.SoundLocation = "Sounds\\aa.wav";
                player.Load();
                player.Play();
            }

            if (sm.IsHeld)
                rejectButton.Checked = true;
            else
                rejectButton.Checked = false;

            if (ActiveCallList.Count > 0)
            {
                rejectButton.Enabled = true;
                holdButton.Enabled = true;
                timer1.Enabled = true;
            }
            else
            {
                rejectButton.Enabled = false;
                holdButton.Enabled = false;
                timer1.Enabled = false;
            }
        }

        private void OnIncomingCallNotification(Int32 iSessionId, String szNumber, String szInfo)
        {
            IStateMachine sm = CallManager.getCall(iSessionId);
            string message = "{\"Event\":\"Invite\",\"Call-ID\":\"" + iSessionId.ToString() + "@1.1.1.1:5060\",\"From\":\"sip:" + sm.CallingNumber + "@1.1.1.1\",\"To\":\"sip:" + DeliveryRI.Login + "@1.1.1.1:8780\"}";
            listener.SendResponse(message);
        }
        #endregion

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(CallManager.IsInitialized)
                CallManager.Shutdown();

            try
            {
                listener.listen = false;
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
                CallManager.onUserHoldRetrieve(v_hCall.Session);
        }

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
                DB.SetStatus("REST");
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
                DB.SetStatus("INTERNET");
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
                DB.SetStatus("TRAINING");
            }
            else
            {
                studyToolStripMenuItem.Checked = true;
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Улюлю!");
        }

        private void curCallInfoDG_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(RegisterInfo.Login != "")
                DB.SetStatus("OFFLINE");

            Global.SaveSettings();
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
            if (portGoToolStripMenuItem.Checked)
                phonerLiteToolStripMenuItem.Checked = false;
        }

        private void phonerLiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (phonerLiteToolStripMenuItem.Checked)
                portGoToolStripMenuItem.Checked = false;
        }
    }
}
