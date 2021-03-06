﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace LoadSaveFile
{
    public partial class rxConfig : Form
    {
        List<string> wifitypes = new List<string>() { "802.11nHT20", "802.11nHT40", "802.11b", "802.11g" };
        public rxConfig()
        {
            InitializeComponent();
            cbwifi.DataSource = wifitypes;
        }
        /// <summary>
        /// Set Rx
        /// </summary>
        string path;
        private void loadFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "txt file|*.txt";
            op.ShowDialog();
            try
            {
                string[] line = File.ReadAllLines(op.FileName);
                path = op.FileName;
                ListData.Items.AddRange(line);
            }
            catch (Exception)
            {
                MessageBox.Show("Cant Not Load File");
                //throw;
            }
        }

        private void saveFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListData.Items.Count > 0)
            {
                using (TextWriter TW = new StreamWriter(path))
                {
                    foreach (string item in ListData.Items)
                    {
                        TW.WriteLine(item);
                    }
                }
            }
        }
        private void saveAsFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListData.Items.Count > 0)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.Filter = "txt file|*.txt";
                savefile.ShowDialog();
                try
                {
                    using (TextWriter TW = new StreamWriter(savefile.FileName))
                    {
                        foreach (string item in ListData.Items)
                        {
                            TW.WriteLine(item);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Cant Not Save File");
                }
            }
        }
        /// <summary>
        /// Set button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Save_Click_1(object sender, EventArgs e)
        {
            ListData.Items.Add("Wifi=" + cbwifi.Text + ";Anten=" + txtanten.Text + ";Channel=" + txtchannel.Text + ";Rate=" + txtrate.Text + ";Power=" + txtpower.Text + ";Packet count=" + txtpacket.Text+";Delay="+txtdelay.Text);
        }
        private void btnmodify_Click(object sender, EventArgs e)
        {
            int index = ListData.SelectedIndex;
            ListData.Items.RemoveAt(index);
            ListData.Items.Insert(index, "Wifi=" + cbwifi.Text + ";Anten=" + txtanten.Text + ";Channel=" + txtchannel.Text + ";Rate=" + txtrate.Text + ";Power=" + txtpower.Text + ";Packet count=" + txtpacket.Text + ";Delay=" + txtdelay.Text);
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            if (this.ListData.SelectedIndex >= 0)
            {
                this.ListData.Items.RemoveAt(this.ListData.SelectedIndex);
            }
        }

        private void cbwifi_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            switch (cbwifi.Items[cbwifi.SelectedIndex].ToString())
            {
                case "802.11nHT20":
                    {
                        txtanten.Text = "1,2";
                        txtchannel.Text = "1,2,3,4,5,6,7,8,9,10,11,12";
                        txtrate.Text = "0,1,2,3,4,5,6,7";
                        txtpower.Text = "-30";
                        txtpacket.Text = "1000";
                        txtdelay.Text = "5000";
                        break;

                    }
                case "802.11nHT40":
                    {
                        txtanten.Text = "1,2";
                        txtchannel.Text = "5,8,13";
                        txtrate.Text = "7,8,13";
                        txtpower.Text = "-30";
                        txtpacket.Text = "1000";
                        txtdelay.Text = "5000";
                        break;

                    }
                case "802.11b":
                    {
                        txtanten.Text = "1,2";
                        txtchannel.Text = "1,2,3,4,5,6,7,8,9,10,11,12";
                        txtrate.Text = "1,2,5.5,11";
                        txtpower.Text = "-30";
                        txtpacket.Text = "1000";
                        txtdelay.Text = "5000";
                        break;

                    }
                case "802.11g":
                    {
                        txtanten.Text = "1,2";
                        txtchannel.Text = "1,2,3,4,5,6,7,8,9,10,11,12";
                        txtrate.Text = "6,9,12,18,24,36,48,54";
                        txtpower.Text = "-30";
                        txtpacket.Text = "1000";
                        txtdelay.Text = "5000";
                        break;

                    }
            }
        }

        private void ListData_Click(object sender, EventArgs e)
        {
            try
            {
                string data = ListData.Items[ListData.SelectedIndex].ToString();
                //string[] array = data.Split(';');
                string[] buffer = data.Split(';');
                string wifi = buffer[0].Split('=')[1];
                string anten = buffer[1].Split('=')[1];
                string channel = buffer[2].Split('=')[1];
                string rate = buffer[3].Split('=')[1];
                string power = buffer[4].Split('=')[1];
                string numberpacket = buffer[5].Split('=')[1];
                string delay = buffer[6].Split('=')[1];
                cbwifi.Text = wifi;
                txtanten.Text = anten;
                txtchannel.Text = channel;
                txtpower.Text = power;
                txtrate.Text = rate;
                txtpacket.Text = numberpacket;
                txtdelay.Text = delay;
            }
            catch { }
        }
    }
}
