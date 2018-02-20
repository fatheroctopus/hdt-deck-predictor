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
		public string CurrentMetaFileVersion { get; set;  }

		public PluginConfig() {
			this.CurrentMetaFileVersion = "1";
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
