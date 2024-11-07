using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Management;  
using System.Windows.Forms;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace GetKeyFromDiscord
{
    public partial class Form1 : Form
    {
        private string generatedCode;
        private string userHWID;

        private DateTime lastWebhookSendTime;  
        private TimeSpan webhookCooldown = TimeSpan.FromHours(2);

        public Form1()
        {
            InitializeComponent();
            userHWID = GetHWID();  
            lastWebhookSendTime = DateTime.MinValue;  
        }

        private string GenerateRandomCode()
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 6)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private string GetHWID()
        {
            string hwid = string.Empty;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    hwid = wmi_HD["SerialNumber"].ToString();
                    break; 
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting HWID: {ex.Message}", "Error");
            }
            return hwid;
        }


        private async void SendCodeToDiscord(string code, string hwid)
        {
            string webhookUrl = "https://discord.com/api/webhooks/1297393328442048642/BD5mXGkNxIv4OUyt_lNMf3zhGf2h08lH4J6vwNlOZ9K-DFU92j62LD0kcom4ahbpBh8w";
            string content = $"User HWID: {hwid}\nGenerated Code: {code}";

            using (HttpClient client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "content", content }
                };

                var contentData = new FormUrlEncodedContent(values);
                var response = await client.PostAsync(webhookUrl, contentData);
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == generatedCode)
            {
                string currentHWID = GetHWID();
                if (currentHWID == userHWID)
                {
                    MessageBox.Show("Login successful!", "Success");
                }
                else
                {
                    MessageBox.Show("Login failed! Incorrect HWID.", "Failed");
                }
            }
            else
            {
                MessageBox.Show("Invalid code!", "Failed");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (DateTime.Now - lastWebhookSendTime >= webhookCooldown)
            {
                generatedCode = GenerateRandomCode();
                SendCodeToDiscord(generatedCode, userHWID);
                lastWebhookSendTime = DateTime.Now;  
                MessageBox.Show("A random code has been sent to Discord!", "Code Generated");
            }
            else
            {
   
                TimeSpan timeRemaining = webhookCooldown - (DateTime.Now - lastWebhookSendTime);
                MessageBox.Show($"Please wait {timeRemaining.Hours} hour(s) and {timeRemaining.Minutes} minute(s) before generating a new code.", "Cooldown in effect");
            }
        }
    }
}
