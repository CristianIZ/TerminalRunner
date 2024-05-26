using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;
using System.IO;
using System.Management;

namespace UI
{
    public partial class Form1 : Form
    {
        IList<ProcessDTO> commands = new List<ProcessDTO>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.timer1.Interval = 1000;
            this.timer1.Enabled = true;

            var items = new List<ProcessDTO>()
            {
                new ProcessDTO() { Name = "Advantage API", Type = ProcessType.AdvantageAPI },
                new ProcessDTO() { Name = "Palermo App Front", Type = ProcessType.PalermoAppFront },
                new ProcessDTO() { Name = "Palermo App API", Type = ProcessType.PalermoAppAPI },
                new ProcessDTO() { Name = "Intranet Front", Type = ProcessType.IntranetFront },
                new ProcessDTO() { Name = "Intranet API", Type = ProcessType.IntranetAPI },
                new ProcessDTO() { Name = "Prohibitions API", Type = ProcessType.ProhibitionsAPI },
                new ProcessDTO() { Name = "Promotions Front", Type = ProcessType.PromotionsFront },
                new ProcessDTO() { Name = "Promotions API", Type = ProcessType.PromotionsAPI },
                new ProcessDTO() { Name = "Sales Force API", Type = ProcessType.SalesForceAPI },
            };

            foreach (var item in items.OrderBy(i => i.Name))
            {
                cmbService.Items.Add(item);
            }
        }

        private Process StartProcess(string command)
        {
            var cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.Arguments = $"/C {command}";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.CreateNoWindow = false;
            cmd.StartInfo.Verb = "runas";
            cmd.Start();

            return cmd;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (cmbService.SelectedItem == null)
                return;

            var item = (ProcessDTO)cmbService.SelectedItem;

            if (commands.Any(p => p.Type == item.Type))
            {
                MessageBox.Show("Este proceso ya se encuentra en ejecucion");
                return;
            }

            var process = this.StartProcess(BuildCommand(item.Type));
            commands.Add(new ProcessDTO() { Type = item.Type, Process = process, Name = item.Name });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (lstProcess.SelectedItem == null)
                return;

            var item = (ProcessDTO)lstProcess.SelectedItem;
            var process = commands.Where(p => p.Type == item.Type).FirstOrDefault();

            KillProcessAndChildren(process.Process.Id);
        }

        private void btnGetCommand_Click(object sender, EventArgs e)
        {
            if (cmbService.SelectedItem == null)
                return;

            var item = (ProcessDTO)cmbService.SelectedItem;

            Clipboard.SetText(BuildCommand(item.Type));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (lstProcess.Items.Count != commands.Count)
            {
                lstProcess.Items.Clear();

                foreach (var item in commands.OrderBy(p => p.Name))
                {
                    lstProcess.Items.Add(item);
                }
            }

            IList<ProcessDTO> processTypesToRemove = new List<ProcessDTO>();

            foreach (var p in commands)
            {
                if (p.Process.HasExited)
                {
                    processTypesToRemove.Add(p);
                }
            }

            commands = commands.Where(pr => !pr.Process.HasExited).ToList();
        }

        private string BuildCommand(ProcessType processType)
        {
            string npmCommand = "npm run start --prefix";
            string dotnetCommand = "dotnet run --project";
            string path;

            switch (processType)
            {
                case ProcessType.AdvantageAPI:
                    path = ConfigurationManager.AppSettings["AdvantageAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                case ProcessType.PalermoAppFront:
                    path = ConfigurationManager.AppSettings["PalermoAppFrontPath"];
                    return $"{npmCommand} {path}";

                case ProcessType.PalermoAppAPI:
                    path = ConfigurationManager.AppSettings["PalermoAppAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                case ProcessType.IntranetFront:
                    path = ConfigurationManager.AppSettings["IntranetFrontPath"];
                    return $"{npmCommand} {path}";

                case ProcessType.IntranetAPI:
                    path = ConfigurationManager.AppSettings["IntranetAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                case ProcessType.ProhibitionsAPI:
                    path = ConfigurationManager.AppSettings["ProhibitionsAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                case ProcessType.PromotionsFront:
                    path = ConfigurationManager.AppSettings["PromotionsFrontPath"];
                    return $"{npmCommand} {path}";

                case ProcessType.PromotionsAPI:
                    path = ConfigurationManager.AppSettings["PromotionsAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                case ProcessType.SalesForceAPI:
                    path = ConfigurationManager.AppSettings["SalesForceAPIPath"];
                    return $"{dotnetCommand} {path}\\Api.csproj";

                default:
                    return $"";
            }
        }

        private void KillProcessAndChildren(int pid)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Process WHERE ParentProcessID={pid}");
            ManagementObjectCollection moc = searcher.Get();

            foreach (ManagementObject m in moc)
            {
                KillProcessAndChildren(Convert.ToInt32(m["ProcessID"]));
            }

            try
            {
                Process proc = Process.GetProcessById(pid);
                proc.Kill();
            }
            catch (Exception)
            {

            }
        }

        private void chkAlwaysOnTop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = this.chkAlwaysOnTop.Checked;
        }
    }
}
