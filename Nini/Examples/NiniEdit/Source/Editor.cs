#region Copyright
//
// Nini Configuration Project.
// Copyright (C) 2004 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.IO;
using System.Text;
using Nini.Config;

namespace NiniEdit
{
	/// <summary>
	/// Summary description for Config.
	/// </summary>
	public class Editor
	{
		#region Private variables
		ArgvConfigSource argvSource = null;
		const string configName = "ConfigList";
		string configPath = null;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructor.  Accepts the command line arguments.
		/// </summary>
		public Editor (string[] arguments)
		{
			argvSource = new ArgvConfigSource (arguments);
			SetSwitches ();
		}
		#endregion

		#region Private utility methods
		/// <summary>
		/// Processes all arguments.
		/// </summary>
		public void ProcessArgs ()
		{
			if (argvSource.GetArguments ().Length < 1 
				|| IsArg ("help")) {
				PrintUsage ();
				return;
			}
			
			if (IsArg ("version")) {
				PrintVersion ();
				return;
			}
			
			configPath = ConfigPath ();
			if (!File.Exists (configPath)) {
				ThrowError ("Config file does not exist");
			}

			if (IsArg ("list")) {
				ListConfigs ();
			}
			if (IsArg ("add")) {
				AddConfig ();
			}
			if (IsArg ("remove")) {
				RemoveConfig ();
			}
			if (IsArg ("list-keys")) {
				ListKeys ();
			}
			if (IsArg ("set-key")) {
				SetKey ();
			}
			if (IsArg ("remove-key")) {
				RemoveKey ();
			}
			if (IsArg ("get-key")) {
				GetKey ();
			}
		}
		
		/// <summary>
		/// Returns the config file (input) path.
		/// </summary>
		private string ConfigPath ()
		{
			int length = argvSource.GetArguments ().Length;
			return argvSource.GetArguments ()[length - 1];
		}
		
		/// <summary>
		/// Prints a message to the console.
		/// </summary>
		private void PrintLine (string message)
		{
			Console.WriteLine (message);
		}
		
		/// <summary>
		/// Throws an exception with the specified message.
		/// </summary>
		private void ThrowError (string message)
		{
			throw new Exception (message);
		}
		
		/// <summary>
		/// Returns the value of an argv argument.
		/// </summary>
		private string GetArg (string key)
		{
			return argvSource.Configs[configName].Get (key);
		}

		/// <summary>
		/// Returns true if an argv argument is present.
		/// </summary>
		private bool IsArg (string key)
		{
			return (GetArg (key) != null);
		}
		
		/// <summary>
		/// Returns the named config or throws an exception if it
		/// is not found.
		/// </summary>
		private IConfig GetConfig (IConfigSource source, string configName)
		{
			IConfig result = source.Configs[configName];
			
			if (result == null) {
				ThrowError ("A config of that name does not exist");
			}
			
			return result;
		}
		
		/// <summary>
		/// Loads a sourc from file.
		/// </summary>
		private IConfigSource LoadSource (string path)
		{
			IConfigSource result = null;
			string extension = null;

			if (IsArg ("set-type")) {
				extension = "." + GetArg ("set-type");
			} else {
				FileInfo info = new FileInfo (path);
				extension = info.Extension;
			}
			
			switch (extension)
			{
				case ".ini":
					result = new IniConfigSource (path);
					break;
				case ".config":
					result = new DotNetConfigSource (path);
					break;
				case ".xml":
					result = new XmlConfigSource (path);
					break;
				default:
					ThrowError ("Unknown config file type");
					break;
			}
			
			return result;
		}
		#endregion
		
		#region Config switch methods
		/// <summary>
		/// Lists all configs in the file.
		/// </summary>
		private void ListConfigs ()
		{
			IConfigSource source = LoadSource (configPath);
			
			foreach (IConfig config in source.Configs)
			{
				PrintLine (config.Name);
			}
		}
		
		/// <summary>
		/// Removes a config.
		/// </summary>
		private void RemoveConfig ()
		{
			string configName = GetArg ("remove");
			if (configName == null) {
				ThrowError ("You must supply a config switch");
			}
			
			IConfigSource source = LoadSource (configPath);
			IConfig config = GetConfig (source, configName);
			
			source.Configs.Remove (config);
			source.Save ();
		}
		
		/// <summary>
		/// Adds a new config.
		/// </summary>
		private void AddConfig ()
		{
			IConfigSource source = LoadSource (configPath);
			source.AddConfig (GetArg ("add"));
			
			source.Save ();
		}
		#endregion

		#region Key switch methods
		/// <summary>
		/// Lists the keys in a specified config.
		/// </summary>
		private void ListKeys ()
		{
			string configName = GetArg ("config");
			if (configName == null) {
				ThrowError ("You must supply a config switch");
			}
			
			IConfigSource source = LoadSource (configPath);
			string[] keys = GetConfig (source, configName).GetKeys ();
			
			foreach (string key in keys)
			{
				PrintLine (key);
			}
		}
		
		/// <summary>
		/// Sets a new key and value.
		/// </summary>
		private void SetKey ()
		{
			string configName = GetArg ("config");
			if (configName == null) {
				ThrowError ("You must supply a config switch");
			}
			
			IConfigSource source = LoadSource (configPath);
			IConfig config = GetConfig (source, configName);
			
			string[] keyValue = GetArg ("set-key").Split (',');
			if (keyValue.Length < 2) {
				throw new Exception ("Must supply KEY,VALUE");
			}
			config.Set (keyValue[0], keyValue[1]);
			source.Save ();
		}
		
		/// <summary>
		/// Prints out the value of a given key.
		/// </summary>
		private void GetKey ()
		{
			string configName = GetArg ("config");
			if (configName == null) {
				ThrowError ("You must supply a config switch");
			}
			
			IConfigSource source = LoadSource (configPath);
			IConfig config = GetConfig (source, configName);
			
			string text = config.Get (GetArg ("get-key"));
			
			if (text != null) {
				PrintLine (text);
			} else {
				PrintLine ("Key not found");
			}
		}
		
		/// <summary>
		/// Removes a key.
		/// </summary>
		private void RemoveKey ()
		{
			string configName = GetArg ("config");
			if (configName == null) {
				ThrowError ("You must supply a config switch");
			}
			
			IConfigSource source = LoadSource (configPath);
			IConfig config = GetConfig (source, configName);
			
			config.Remove (GetArg ("remove-key"));
			source.Save ();
		}
		
		#endregion
		
		#region Application switch methods
		/// <summary>
		/// Sets the switches for the application.
		/// </summary>
		private void SetSwitches ()
		{
			// Application switches
			argvSource.AddSwitch (configName, "help", "h");
			argvSource.AddSwitch (configName, "version", "v");
								  
			// Config switches
			argvSource.AddSwitch (configName, "list", "l");
			argvSource.AddSwitch (configName, "remove", "r");
			argvSource.AddSwitch (configName, "add", "a");
			argvSource.AddSwitch (configName, "set-type", "s");

			// Key switches
			argvSource.AddSwitch (configName, "config", "c");
			argvSource.AddSwitch (configName, "list-keys", "L");
			argvSource.AddSwitch (configName, "remove-key", "R");
			argvSource.AddSwitch (configName, "set-key", "k");
			argvSource.AddSwitch (configName, "get-key", "g");
		}

		/// <summary>
		/// Prints out the usage for the program.
		/// </summary>
		private void PrintUsage ()
		{
			StringWriter writer = new StringWriter ();

			writer.WriteLine ("Nini Editor " + GetProductVersion () + 
							  ", command-line configuration file editor");
			writer.WriteLine ("Usage: niniedit [OPTION] [FILE]");
			writer.WriteLine ("");
			writer.WriteLine ("General Options:");
			writer.WriteLine ("  -h,  --help                   Shows this help");
			writer.WriteLine ("  -v,  --version                Displays the application version");
			writer.WriteLine ("  -s,  --set-type=TYPE          Specifies file type (ini, xml, or config)");
			writer.WriteLine ("");
			writer.WriteLine ("Config Options:");
			writer.WriteLine ("  -l,  --list                   Lists all configs");
			writer.WriteLine ("  -r,  --remove=CONFIG          Removes a config");
			writer.WriteLine ("  -a,  --add=CONFIG             Adds a config");
			writer.WriteLine ("");
			writer.WriteLine ("Key Options:");
			writer.WriteLine ("  -c,  --config=CONFIG          Selects a config");
			writer.WriteLine ("  -L,  --list-keys              Lists all keys (needs -c switch)");
			writer.WriteLine ("  -R,  --remove-key=KEY         Removes a key (needs -c switch)");
			writer.WriteLine ("  -k,  --set-key=KEY,VALUE      Sets/adds a key and value (needs -c switch)");
			writer.WriteLine ("  -g,  --get-key=KEY            Gets a key value (needs -c switch)");
			writer.WriteLine ("");
			writer.WriteLine ("Nini homepage: http://nini.sourceforge.net/");

			PrintLine (writer.ToString ());
		}
		
		/// <summary>
		/// Prints out the version of this application.
		/// </summary>
		private void PrintVersion ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("Nini Editor " + GetProductVersion ());
			writer.WriteLine ("");
			writer.WriteLine ("Copyright 2004 Brent R. Matzelle");
			writer.WriteLine ("This program is distributed under the MIT/X11 license:");
			writer.WriteLine ("http://www.opensource.org/licenses/mit-license.php");
			
			PrintLine (writer.ToString ());
		}
		
		/// <summary>
		/// Returns the product version.
		/// </summary>
		private string GetProductVersion ()
		{
			return "0.1.0";
		}
		#endregion
	}
}