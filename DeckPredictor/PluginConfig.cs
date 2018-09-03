using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeckPredictor
{
	public class PluginConfig
	{
		private static readonly string ConfigPath =
				Path.Combine(DeckPredictorPlugin.DataDirectory, "config.xml");

		public string CurrentMetaFileVersion { get; set;  }
		public DateTime CurrentMetaFileDownloadTime { get; set; }
		public bool FitDeckListToDisplay { get; set; } = true;

		public static PluginConfig Load()
		{
			if (File.Exists(ConfigPath))
			{
				using (var reader = new StreamReader(ConfigPath))
				{
					return Load(reader);
				}
			}
			else
			{
				return new PluginConfig();
			}
		}

		public static PluginConfig Load(StreamReader reader) {
			var serializer = new XmlSerializer(typeof(PluginConfig));
			PluginConfig config = null;
			try
			{
				config = (PluginConfig)serializer.Deserialize(reader);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
			return config;
		}

		public PluginConfig()
		{
			this.CurrentMetaFileVersion = "1";
		}

		public void Save()
		{
			Log.Debug("Saving config");
			using (var writer = new StreamWriter(ConfigPath))
			{
				Save(writer);
			}
		}

		public void Save(StreamWriter writer) {
			var serializer = new XmlSerializer(typeof(PluginConfig));
			try
			{
				serializer.Serialize(writer, this);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}
	}
}
