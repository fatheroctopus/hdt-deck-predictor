using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeckPredictor;

namespace DeckPredictorTests.Tests
{
	[TestClass]
	public class LogTest
	{
		[TestMethod]
		public void Info_AddedToLogQueue()
		{
			Log.Info("test string");
			Assert.IsTrue(Log.PrevLine.Contains("test string"));
		}

	}
}