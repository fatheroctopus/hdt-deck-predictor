using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System;

namespace DeckPredictor
{
	public partial class SettingsWindow : MetroWindow
	{
		public SettingsWindow(PluginConfig config)
		{
			InitializeComponent();
		}

		private void ButtonReadme_Click(object sender, RoutedEventArgs e)
		{
		    System.Diagnostics.Process.Start("https://github.com/fatheroctopus/hdt-deck-predictor");
		}
	}
}
