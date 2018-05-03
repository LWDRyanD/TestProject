using System.Reflection;

[assembly: AssemblyTitle("CraftAssist")]
[assembly: AssemblyDescription("An ACT plugin for crafting on Final Fantasy XIV.")]
[assembly: AssemblyCompany("Ryan Durand (Cole Graves of Ultros)")]
[assembly: AssemblyVersion("1.0.0.0")]

namespace ACT_Plugin
{
    using Advanced_Combat_Tracker;
    using System.Collections.Generic;
    using System.IO;
    using System;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;
    using System.Xml.Serialization;

    public class CraftAssist : UserControl, IActPluginV1
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
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(434, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "This is the user interface that appears as a new tab under the Plugins tab.  [REP" +
				"LACE ME]";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(6, 16);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(431, 20);
			this.textBox1.TabIndex = 1;
			this.textBox1.Text = "Sample TextBox that has its value stored to the settings file automatically.";
			// 
			// PluginSample
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Name = "PluginSample";
			this.Size = new System.Drawing.Size(686, 384);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private TextBox textBox1;

		private System.Windows.Forms.Label label1;

        #endregion

        private string _settingsFile = Path.Combine(ActGlobals.oFormActMain.AppDataFolder.FullName, "Config\\CraftAssist.config.xml");
        private Label _pluginLabel;
        private SettingsSerializer _actxmlSettingsSerializer;

        public CraftAssist()
		{
			InitializeComponent();
		}
        
		#region IActPluginV1 Members
		public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
		{
            // Set up plug-in tab.
			this._pluginLabel = pluginStatusText;

			pluginScreenSpace.Controls.Add(this);	// Add this UserControl to the tab ACT provides
			this.Dock = DockStyle.Fill;	// Expand the UserControl to fill the tab's client space
			_actxmlSettingsSerializer = new SettingsSerializer(this);	// Create a new settings serializer and pass it this instance
			LoadSettings();


            ActGlobals.oFormActMain.BeforeLogLineRead += OFormActMain_BeforeLogLineRead;
			_pluginLabel.Text = "Plugin Started";
		}

        private List<int> wantedIds = new List<int>() { 21, 0, 26 };

        private void OFormActMain_BeforeLogLineRead(bool isImport, LogLineEventArgs logInfo)
        {
            string timeStamp = logInfo.logLine.Substring(0, 14);
            string[] tokens = logInfo.logLine.Substring(15).Split(':');
            int id = int.Parse(tokens[0], System.Globalization.NumberStyles.HexNumber);

            // Filter to what we want.
            if (wantedIds.Contains(id))
            {
                using (var file = File.Open(, FileMode.Append))
                {
                    using (var stream = new StreamWriter(file))
                    {
                        stream.WriteLine(logInfo.logLine);
                    }
                }
            }
        }

        public void DeInitPlugin()
		{
			// Unsubscribe from any events you listen to when exiting!
			ActGlobals.oFormActMain.BeforeLogLineRead -= OFormActMain_BeforeLogLineRead;

			SaveSettings();
			_pluginLabel.Text = "Plugin Exited";
		}
		#endregion
        
		void LoadSettings()
		{
			_actxmlSettingsSerializer.AddControlSetting(textBox1.Name, textBox1);

			if (File.Exists(settingsFile))
			{
				FileStream fs = new FileStream(settingsFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				XmlTextReader xReader = new XmlTextReader(fs);

				try
				{
					while (xReader.Read())
					{
						if (xReader.NodeType == XmlNodeType.Element)
						{
							if (xReader.LocalName == "SettingsSerializer")
							{
								_actxmlSettingsSerializer.ImportFromXml(xReader);
							}
						}
					}
				}
				catch (Exception ex)
				{
					_pluginLabel.Text = "Error loading settings: " + ex.Message;
				}
				xReader.Close();
			}
		}

        internal class CraftAssistSettings
        {
            public string LogFile { get; set; }
            public string FilteredLogFile { get; set; }
        }

		private void SaveSettings()
		{
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CraftAssistSettings));
            xmlSerializer.Serialize(File.OpenWrite(this.log))

            using (FileStream fs = new FileStream(settingsFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                XmlTextWriter xWriter = new XmlTextWriter(fs, Encoding.UTF8);
                xWriter.WriteStartDocument(true);
                xWriter.WriteStartElement("Config");    // <Config>
                xWriter.WriteStartElement("SettingsSerializer");    // <Config><SettingsSerializer>
                _actxmlSettingsSerializer.ExportToXml(xWriter);   // Fill the SettingsSerializer XML
                xWriter.WriteEndElement();  // </SettingsSerializer>
                xWriter.WriteEndElement();  // </Config>
                xWriter.WriteEndDocument(); // Tie up loose ends (shouldn't be any)
                xWriter.Flush();    // Flush the file buffer to disk
                xWriter.Close();
            }
		}
	}
}
