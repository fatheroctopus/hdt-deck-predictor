using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeckPredictor;

namespace DeckPredictorTests
{
	[TestClass]
	public class PluginConfigTest
	{
		[TestMethod]
		public void CurrentMetaFileVersion_DefaultIs1()
		{
			var config = new PluginConfig();
			Assert.AreEqual("1", config.CurrentMetaFileVersion);
		}

		[TestMethod]
		public void CurrentMetaFileVersion_SaveLoad()
		{
			var config1 = new PluginConfig();
			config1.CurrentMetaFileVersion = "2";
			var memoryStream = new MemoryStream();
			config1.Save(new StreamWriter(memoryStream));
			memoryStream.Seek(0, SeekOrigin.Begin);

			var config2 = PluginConfig.Load(new StreamReader(memoryStream));
			Assert.AreEqual("2", config2.CurrentMetaFileVersion);
		}
	}
}