﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoadSaveFile;
using DUT;
using TESTER;
using WIFI;
using System.IO;

namespace ontWifiTest {

    #region MAINFORM
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    public partial class mainForm : Form {

        #region User Interface

        //KHAI BÁO BIẾN TOÀN CỤC
        //---------------------------------//
        GW ontDevice = null;
        EXM6640A Instrument = null;


        List<dataFields> txListTestCase = new List<dataFields>();
        string[] dataLines;

        List<txGridDataRow> txGridDataContext = new List<txGridDataRow>();
        List<rxGridDataRow> rxGridDataContext = new List<rxGridDataRow>();

        List<TextBox> listControls = null;
        List<TextBox> defaultSettings {
            get {
                List<TextBox> list = new List<TextBox>() { txtInstrAddress, txtPackets, txtWaitSent, txtDUTAddr, txtDUTUser, txtDUTPassword };
                var Settings = Properties.Settings.Default;
                txtInstrAddress.Text = Settings.Instrument;
                txtPackets.Text = Settings.Packets;
                txtWaitSent.Text = Settings.WaitSent;
                txtDUTAddr.Text = Settings.DUT;
                txtDUTUser.Text = Settings.User;
                txtDUTPassword.Text = Settings.Password;
                return list;
            }
            set {
                var Settings = Properties.Settings.Default;
                Settings.Instrument = value[0].Text;
                Settings.Packets = value[1].Text;
                Settings.WaitSent = value[2].Text;
                Settings.DUT = value[3].Text;
                Settings.User = value[4].Text;
                Settings.Password = value[5].Text;
                Settings.Save();
            }
        }
        /// <summary>
        /// INIT CONTROL DATACONTENT
        /// </summary>
        bool initialControlContext {
            set {
                lblProgress.Text = "0 / 0";
                lblTimeElapsed.Text = "00:00:00.0000";
                lblType.Text = "--";
                lblStatus.Text = "Ready!";
                lblProjectName.Text = ProductName.ToString();
                lblProjectVer.Text = string.Format("Verion: {0}", ProductVersion);
                rtbDetails.Clear();
                progressBarTotal.Value = 0;
                dgTXGrid.DataSource = null;
                dgTXGrid.DataSource = txGridDataContext;
                dgRXGrid.DataSource = null;
                dgRXGrid.DataSource = rxGridDataContext;
                btnStart.Focus();
            }
        }

        //KHỞI TẠO, SAVE SETTING
        //---------------------------------//
        /// <summary>
        ///INITIAL FORM
        /// </summary>
        public mainForm() {
            InitializeComponent();
            listControls = new List<TextBox>() { txtInstrAddress, txtPackets, txtWaitSent, txtDUTAddr, txtDUTUser, txtDUTPassword };
        }

        /// <summary>
        /// LOAD FORM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainForm_Load(object sender, EventArgs e) {
            listControls = defaultSettings;
            initialControlContext = true;
        }

        /// <summary>
        /// EXIT APPLICATION
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ckChangeSettings_CheckedChanged(object sender, EventArgs e) {
            if (ckChangeSettings.Checked == true) {
                foreach (var item in listControls) { //Enable textboxs for changing content
                    item.Enabled = true;
                }
            }
            else {
                foreach (var item in listControls) { //Disable textboxs for saving content
                    item.Enabled = false;
                }
                defaultSettings = listControls;
            }
        }

        private void tXToolStripMenuItem_Click(object sender, EventArgs e) {
            LoadSaveFile.txConfig tfrm = new LoadSaveFile.txConfig();
            tfrm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e) {
            this.Close();
        }

        private void loadTestCaseToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "file *.txt|*.txt";
            if (op.ShowDialog() == DialogResult.OK) {
                string Str = op.FileName;
                lbltestcasefilePath.Text = Str;
                dataLines = File.ReadAllLines(Str);
                lblType.Text = Str.ToUpper().Contains("TX") == true ? "TX" : "RX";
            }
        }

        #endregion

        #region Main Program

        #region WriteDebug 

        void debugWrite(string data) {
            rtbDetails.AppendText(data);
        }

        void debugWriteLine(params string[] data) {
            if (data.Length > 1) {
                debugWrite(string.Format("{0}, {1}\n", DateTime.Now.ToString("HH:mm:ss ffff"), data[1]));
            }
            else {
                debugWrite(string.Format("{0}\n", data));
            }

        }
        #endregion

        #region Connect
        //Connect to ONT
        bool connectONT() {
            try {
                bool result;
                string message;
                var Settings = Properties.Settings.Default;
                debugWriteLine("Connecting to ONT...");
                ontDevice = new GW020(Settings.DUT, 23);
                result = ontDevice.Connection();
                if (!result) goto NG;
                result = ontDevice.Login(Settings.User, Settings.Password, out message);
                if (!result) goto NG;
                ontDevice.WriteLine("sh");
                message += ontDevice.Read();
                goto OK;
            }
            catch {
                goto NG;
            }
            OK:
            debugWriteLine("Connected");
            return true;
            NG:
            debugWriteLine("Disconnected");
            return false;
        }

        //Connect to Instrument
        bool connectInstrument() {
            try {
                var Settings = Properties.Settings.Default;
                debugWriteLine("Connecting to Instrument...");
                Instrument = new EXM6640A(Settings.Instrument);
                goto OK;
            }
            catch {
                goto NG;
            }
            OK:
            debugWriteLine("Connected");
            return true;
            NG:
            debugWriteLine("Disconnected");
            return false;
        }
        #endregion

        #region Load_TX_TestCase

        private bool convertStringtoList(string data, ref List<string> list) {
            try {
                if (!data.Contains(",")) list.Add(data);
                else {
                    string[] buffer = data.Split(',');
                    for (int i = 0; i < buffer.Length; i++) {
                        list.Add(buffer[i]);
                    }
                }
                return true;
            }
            catch {
                return false;
            }
        }

        private bool convertInputData(string datainput, ref List<dataFields> list) {
            try {
                //transfer data input to variables
                string[] buffer = datainput.Split(';');
                string _wifi = buffer[0].Split('=')[1];
                string _bandwidth = "";
                if (_wifi.Contains("HT20")) _bandwidth = "2";
                else if (_wifi.Contains("HT40")) _bandwidth = "4";
                else _bandwidth = "2";

                string anten = buffer[1].Split('=')[1];
                string channel = buffer[2].Split('=')[1];
                string rate = buffer[3].Split('=')[1];
                string power = buffer[4].Split('=')[1];

                //transfer variables to ienumerator
                bool ret;
                List<string> antenlist = new List<string>();
                List<string> channellist = new List<string>();
                List<string> ratelist = new List<string>();
                List<string> powerlist = new List<string>();
                ret = convertStringtoList(anten, ref antenlist);
                ret = convertStringtoList(channel, ref channellist);
                ret = convertStringtoList(rate, ref ratelist);
                ret = convertStringtoList(power, ref powerlist);

                //transfer data from ienumerator to list
                foreach (var atitem in antenlist) {
                    foreach (var chitem in channellist) {
                        foreach (var ritem in ratelist) {
                            foreach (var pitem in powerlist) {
                                dataFields data = new dataFields() { wifi = _wifi, bandwidth = _bandwidth, anten = atitem, channel = chitem, rate = ritem, power = pitem };
                                list.Add(data);
                            }
                        }
                    }
                }
                return true;
            }
            catch {
                return false;
            }
        }


        bool load_AllTXTestCase() {
            try {
                debugWriteLine("Load all TX test case to list...");
                if (dataLines.Length > 0) {
                    bool ret = true;
                    foreach (var item in dataLines) {
                        bool result = convertInputData(item, ref txListTestCase);
                        if (!result) ret = false;
                    }
                    if (!ret) goto NG;
                    else goto OK;
                } else {
                    goto NG;
                }
            }
            catch {
                goto NG;
            }
            OK:
            debugWriteLine("Success");
            return true;
            NG:
            debugWriteLine("Fail");
            return false;
        }

        #endregion



        private void btnStart_Click(object sender, EventArgs e) {
            //Ket noi toi ONT
            if (!connectONT()) return;
            //Ket noi toi Instrument
            if (!connectInstrument()) return;
            //Load settings vao List....
            if (!load_AllTXTestCase()) return;

            //Start LOOP
            //Send ONT command
            //Wait ....
            //Send Instrument command
            //Wait ....
            //Get result


        }

        #endregion
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    #endregion

    #region CUSTOM USER
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//

    /// <summary>
    /// 
    /// </summary>
    public class dataFields {

        public string wifi { get; set; }
        public string bandwidth { get; set; }
        public string anten { get; set; }
        public string channel { get; set; }
        public string rate { get; set; }
        public string power { get; set; }
    }

    /// <summary>
    /// CLASS ĐỊNH NGHĨA KIỂU DỮ LIỆU CỦA CHO TXGRID
    /// </summary>
    public class txGridDataRow {
        private int _anten;
        private double _evm;
        private double _pwr;
        private double _freqerr;
        private double _symclock;
        private int _bandwidth;

        //public int Order { get; set; }
        public string Wifi { get; set; }
        public string ANT {
            get { return string.Format("ANT {0}", _anten); }
            set { _anten = int.Parse(value); }
        }
        public string Bandwidth {
            get { return string.Format("{0}", _bandwidth * 10); }
            set { _bandwidth = int.Parse(value); }
        }
        public int Channel { get; set; }
        public int Freq { get; set; }
        public double Rate { get; set; }
        //public double Power { get; set; }

        public string PL_Limit { get; set; }
        public string Pwr {
            get { return string.Format("{0} dBm", _pwr); }
            set { _pwr = double.Parse(value); }
        }
        public string PU_Limit { get; set; }

        public string FEL_Limit { get; set; }
        public string FreqErr {
            get { return string.Format("{0} kHz", _freqerr); }
            set { _freqerr = double.Parse(value); }
        }
        public string FEU_Limit { get; set; }


        public string SymClock {
            get { return string.Format("{0} ppm", _symclock); }
            set { _symclock = double.Parse(value); }
        }
        public string SCU_Limit { get; set; }

        public string EVM {
            get { return string.Format("{0} {1}", _evm, this.Wifi.ToUpper().Contains("B") == true ? "%" : "dBm"); }
            set { _evm = double.Parse(value); }
        }
        public string EVMU_Limit { get; set; }
    }


    /// <summary>
    /// CLASS ĐỊNH NGHĨA KIỂU DỮ LIỆU CỦA CHO RXGRID
    /// </summary>
    public class rxGridDataRow {
        private int _anten;
        private double _pwr;

        public string Wifi { get; set; }
        public string ANT {
            get { return string.Format("ANT {0}", _anten); }
            set { _anten = int.Parse(value); }
        }
        public int Channel { get; set; }
        public double Rate { get; set; }
        public string Pwr {
            get { return string.Format("{0} dBm", _pwr); }
            set { _pwr = double.Parse(value); }
        }
        public int packetSent { get; set; }
        public int packetGet { get; set; }
        public string PER {
            get {
                return ((packetGet * 1.0) / (packetSent * 1.0)).ToString("00.00%");
            }
        }
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~//
    #endregion
}