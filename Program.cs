using System;
using MoBro.Plugin.RssReader;
using MoBro.Plugin.SDK;
using Serilog.Events;

// create and start the plugin to test it locally
using var plugin = MoBroPluginBuilder
  .Create<Plugin>()
  .WithLogLevel(LogEventLevel.Verbose)
  .Build();

// prevent the program from exiting immediately
Console.ReadLine();