﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.Core;

namespace ICSharpCode.SharpDevelop.Workbench
{
	public sealed class FileChangeWatcher : IDisposable
	{
		public static bool DetectExternalChangesOption {
			get {
				return PropertyService.Get("SharpDevelop.FileChangeWatcher.DetectExternalChanges", true);
			}
			set {
				SD.MainThread.VerifyAccess();
				PropertyService.Set("SharpDevelop.FileChangeWatcher.DetectExternalChanges", value);
				foreach (FileChangeWatcher watcher in activeWatchers) {
					watcher.SetWatcher();
				}
			}
		}
		
		public static bool AutoLoadExternalChangesOption {
			get {
				return PropertyService.Get("SharpDevelop.FileChangeWatcher.AutoLoadExternalChanges", true);
			}
			set {
				PropertyService.Set("SharpDevelop.FileChangeWatcher.AutoLoadExternalChanges", value);
			}
		}
		
		static HashSet<FileChangeWatcher> activeWatchers = new HashSet<FileChangeWatcher>();
		
		static int globalDisableCount;
		
		public static bool AllChangeWatchersDisabled {
			get { return globalDisableCount > 0; }
		}
		
		public static void DisableAllChangeWatchers()
		{
			SD.MainThread.VerifyAccess();
			globalDisableCount++;
			foreach (FileChangeWatcher w in activeWatchers)
				w.SetWatcher();
			Project.ProjectChangeWatcher.OnAllChangeWatchersDisabledChanged();
		}
		
		public static void EnableAllChangeWatchers()
		{
			SD.MainThread.VerifyAccess();
			if (globalDisableCount == 0)
				throw new InvalidOperationException();
			globalDisableCount--;
			foreach (FileChangeWatcher w in activeWatchers)
				w.SetWatcher();
			Project.ProjectChangeWatcher.OnAllChangeWatchersDisabledChanged();
		}
		
		FileSystemWatcher watcher;
		bool wasChangedExternally = false;
		OpenedFile file;
		
		internal FileChangeWatcher(OpenedFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");
			this.file = file;
			SD.Workbench.MainWindow.Activated += MainForm_Activated;
			file.FileNameChanged += file_FileNameChanged;
			activeWatchers.Add(this);
			SetWatcher();
		}

		void file_FileNameChanged(object sender, EventArgs e)
		{
			SetWatcher();
		}
		
		public void Dispose()
		{
			SD.MainThread.VerifyAccess();
			activeWatchers.Remove(this);
			if (file != null) {
				SD.Workbench.MainWindow.Activated -= MainForm_Activated;
				file.FileNameChanged -= file_FileNameChanged;
				file = null;
			}
			if (watcher != null) {
				watcher.Dispose();
				watcher = null;
			}
		}
		
		bool enabled = true;
		
		public bool Enabled {
			get { return enabled; }
			set {
				enabled = value;
				SetWatcher();
			}
		}
		
		void SetWatcher()
		{
			SD.MainThread.VerifyAccess();
			
			if (watcher != null) {
				watcher.EnableRaisingEvents = false;
			}
			
			if (!enabled)
				return;
			if (globalDisableCount > 0)
				return;
			if (DetectExternalChangesOption == false)
				return;
			
			string fileName = file.FileName;
			if (string.IsNullOrEmpty(fileName))
				return;
			if (FileUtility.IsUrl(fileName))
				return;
			if (!Path.IsPathRooted(fileName))
				return;
			
			try {
				if (watcher == null) {
					watcher = new FileSystemWatcher();
					watcher.SynchronizingObject = SD.MainThread.SynchronizingObject;
					watcher.Changed += OnFileChangedEvent;
					watcher.Created += OnFileChangedEvent;
					watcher.Renamed += OnFileChangedEvent;
				}
				watcher.Path = Path.GetDirectoryName(fileName);
				watcher.Filter = Path.GetFileName(fileName);
				watcher.EnableRaisingEvents = true;
			} catch (PlatformNotSupportedException) {
				if (watcher != null) {
					watcher.Dispose();
				}
				watcher = null;
			} catch (FileNotFoundException) {
				// can occur if directory was deleted externally
				if (watcher != null) {
					watcher.Dispose();
				}
				watcher = null;
			} catch (ArgumentException) {
				// can occur if parent directory was deleted externally
				if (watcher != null) {
					watcher.Dispose();
				}
				watcher = null;
			}
		}
		
		void OnFileChangedEvent(object sender, FileSystemEventArgs e)
		{
			if (file == null)
				return;
			LoggingService.Debug("File " + file.FileName + " was changed externally: " + e.ChangeType);
			if (!wasChangedExternally) {
				wasChangedExternally = true;
				if (SD.Workbench.IsActiveWindow) {
					// delay reloading message a bit, prevents showing two messages
					// when the file changes twice in quick succession; and prevents
					// trying to reload the file while it is still being written
					SD.MainThread.CallLater(
						TimeSpan.FromSeconds(0.5),
						delegate { MainForm_Activated(this, EventArgs.Empty); } );
				}
			}
		}
		
		void MainForm_Activated(object sender, EventArgs e)
		{
			if (wasChangedExternally) {
				wasChangedExternally = false;
				
				if (file == null)
					return;
				
				var fileName = file.FileName;
				if (!SD.FileSystem.FileExists(fileName))
					return;
				
				string message = StringParser.Parse(
					"${res:ICSharpCode.SharpDevelop.DefaultEditor.Gui.Editor.TextEditorDisplayBinding.FileAlteredMessage}",
					new StringTagPair("File", Path.GetFullPath(fileName))
				);
				if ((AutoLoadExternalChangesOption && !file.IsDirty)
				    || MessageService.AskQuestion(message, StringParser.Parse("${res:MainWindow.DialogName}")))
				{
					if (SD.FileSystem.FileExists(fileName)) {
						file.ReloadFromDisk();
					}
				} else {
					file.MakeDirty();
				}
			}
		}
	}
}
