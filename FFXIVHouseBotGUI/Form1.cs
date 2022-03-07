using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace FFXIVHouseBotGUI
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, UInt32 Msg, int wParam, int lParam);


        private Process _process { get; set; }

        private bool _running = false;
        
        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_KEYUP = 0x0101;

        const int VK_NUMZERO = 0x60;
        const int VK_NUMTWO = 0x62;
        const int VK_NUMFOUR = 0x64;
        const int VK_NUMEIGHT = 0x68;

        List<int> Keystrokes;

        List<int> buyPersonal = new()
        {
            // select placard
            VK_NUMZERO,
            // open placard
            VK_NUMZERO,
            // click purchase
            VK_NUMZERO,
            // click personal
            VK_NUMZERO,
            // move left to Yes
            VK_NUMFOUR,
            // click yes
            VK_NUMZERO
        };

        List<int> buyFC = new()
        {
            // select placard
            VK_NUMZERO,
            // open placard
            VK_NUMZERO,
            // click purchase
            VK_NUMZERO,
            // select FC
            VK_NUMTWO,
            // click FC
            VK_NUMZERO,
            // move left to Yes
            VK_NUMFOUR,
            // click yes
            VK_NUMZERO
        };

        List<int> reloPersonal = new()
        {
            // select placard
            VK_NUMZERO,
            // open placard
            VK_NUMZERO,
            // click purchase
            VK_NUMZERO,
            // select FC
            VK_NUMTWO,
            // select Relo Personal
            VK_NUMTWO,
            // click relo personal
            VK_NUMZERO,
            // move left to Yes
            VK_NUMFOUR,
            // click yes
            VK_NUMZERO
        };

        List<int> reloFC = new()
        {
            // select placard
            VK_NUMZERO,
            // open placard
            VK_NUMZERO,
            // click purchase
            VK_NUMZERO,
            // select Relo FC
            VK_NUMEIGHT,
            // click relo FC
            VK_NUMZERO,
            // move left to Yes
            VK_NUMFOUR,
            // click yes
            VK_NUMZERO
        };

        public Form1()
        {            
            InitializeComponent();

            comboBoxMode.SelectedIndex = 0;

            HandleProcess();

            Keystrokes = new List<int>();

            backgroundWorker.DoWork += WorkerOnDoWork;
        }

        private void WorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            
            if (null != _process && !_process.HasExited)
            {
                while (!worker.CancellationPending)
                {
                    RunBot();
                }
            }
        }

        private Process FindProcess()
        {
            Process proc = Process.GetProcesses()
                .Where(p => p.ProcessName != typeof(Form1).Assembly.GetName().Name)
                .FirstOrDefault(p => p.ProcessName.StartsWith("ffxiv", StringComparison.CurrentCultureIgnoreCase));

            return proc;
        }

        private void timerProcessRefresh_Elapsed(object sender, ElapsedEventArgs e) => HandleProcess();
        private void labelProcess_Click(object sender, EventArgs e) => HandleProcess();

        private void HandleProcess()
        {
            var proc = FindProcess();

            if (null != proc)
            {
                _process = proc;
                buttonStart.Enabled = true;
                labelProcess.Text = $"Process: {_process.ProcessName} ({_process.Id})";
            }
            else
            {
                _process = null;
                buttonStart.Enabled = false;
                labelProcess.Text = "Click me to find process";
            }
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            // need to select mode
            switch (comboBoxMode.SelectedItem as string)
            {
                case "Personal":
                    Keystrokes = buyPersonal;
                    break;
                case "FC":
                    Keystrokes = buyFC;
                    break;
                case "Relo Personal":
                    Keystrokes = reloPersonal;
                    break;
                case "Relo FC":
                    Keystrokes = reloFC;
                    break;
            }

            _running = !_running;

            if (_running)
            {
                buttonStart.Text = "Stop";
                backgroundWorker.RunWorkerAsync();
            }
            else
            {
                buttonStart.Text = $"Start";
                backgroundWorker.CancelAsync();
            }
        }

        private void RunBot()
        {
            int sleep = (int)numericUpDownDelay.Value;

            while (_running)
            {
                foreach(var key in Keystrokes)
                {
                    PressKey(key);
                    Thread.Sleep(sleep);
                }
                // extra delay between attempts for stability
                Thread.Sleep(sleep);
            }
        }

        private void PressKey(int key)
        {
            PostMessage(_process.MainWindowHandle, WM_KEYDOWN, key, 0);
            //Debug.WriteLine("down");
            Thread.Sleep(25);
            PostMessage(_process.MainWindowHandle, WM_KEYUP, key, 3);
            //Debug.WriteLine("up");
        } 
    }
}