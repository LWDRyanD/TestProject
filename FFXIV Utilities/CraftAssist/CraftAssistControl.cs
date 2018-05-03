namespace CraftAssist
{
    using Advanced_Combat_Tracker;
    using System.Collections.Generic;
    using System.IO;
    using System;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Serialization;
    using System.Windows.Forms.Integration;

    public class CraftAssistControl : UserControl, IActPluginV1
	{
		#region Designer Created Code (Avoid editing)
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.WPFHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // WPFHost
            // 
            this.WPFHost.AutoSize = true;
            this.WPFHost.Location = new System.Drawing.Point(0, 0);
            this.WPFHost.Name = "WPFHost";
            this.WPFHost.Size = new System.Drawing.Size(1, 1);
            this.WPFHost.TabIndex = 2;
            this.WPFHost.Text = "WPFHost";
            this.WPFHost.Child = null;
            // 
            // CraftAssist
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WPFHost);
            this.Name = "CraftAssist";
            this.Size = new System.Drawing.Size(686, 384);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        #endregion

        private string _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\CraftAssist.config.xml");
        private Label _pluginLabel;
        private ElementHost WPFHost;
        private CraftAssistViewModel _viewModel = new CraftAssistViewModel();

        public CraftAssistControl()
		{
			InitializeComponent();
		}
        
		#region IActPluginV1 Members
		public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
		{
            // Add to ACT.
            pluginScreenSpace.Controls.Add(this);   
            this._pluginLabel = pluginStatusText;
            this._pluginLabel.Text = "Plugin Started";

            this.Dock = DockStyle.Fill;

            this.LoadSettings();

            // Set up WPF control.
            var settingsControl = new SettingsControl() { DataContext = this._viewModel };
            this.WPFHost.Child = settingsControl;
            
            // Hook up to log line reads.
            ActGlobals.oFormActMain.BeforeLogLineRead += ProcessLogLine;
		}

        private List<int> wantedIds = new List<int>() { 21, 0, 26 };

        private void ProcessLogLine(bool isImport, LogLineEventArgs logInfo)
        {
            string timeStamp = logInfo.logLine.Substring(0, 14);
            string[] tokens = logInfo.logLine.Substring(15).Split(':');
            int id = int.Parse(tokens[0], System.Globalization.NumberStyles.HexNumber);

            // Filter to what we want.
            if (wantedIds.Contains(id))
            {
                using (var stream = new StreamWriter(this._viewModel.FilteredLogFile, true))
                {
                    stream.WriteLine(logInfo.logLine);
                }
            }

            using (var stream = new StreamWriter(this._viewModel.VerboseLogFile, true))
            {
                stream.WriteLine(logInfo.logLine);
            }
        }

        public void DeInitPlugin()
		{
			// Unsubscribe from any events you listen to when exiting!
			ActGlobals.oFormActMain.BeforeLogLineRead -= ProcessLogLine;

			this.SaveSettings();
			this._pluginLabel.Text = "Plugin Exited";
		}
		#endregion
        
		private void LoadSettings()
		{
            if (File.Exists(this._settingsFile))
            {
                try
                {
                    using (var xmlReader = XmlReader.Create(this._settingsFile))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(CraftAssistViewModel));
                        if (xmlSerializer.CanDeserialize(xmlReader))
                        {
                            this._viewModel = (CraftAssistViewModel)xmlSerializer.Deserialize(xmlReader);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._pluginLabel.Text = $"{ex.GetType().Name} thrown while trying to load settings file '{this._settingsFile}': {ex.Message}";
                }
            }
		}
        
		private void SaveSettings()
		{
            using (var fileStream = File.Open(this._settingsFile, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(CraftAssistViewModel));
                xmlSerializer.Serialize(fileStream, this._viewModel);
            }
		}
	}
}
