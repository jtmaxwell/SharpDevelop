// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;

namespace ICSharpCode.SharpDevelop.Workbench
{
	sealed class FileServiceOpenedFile : OpenedFile
	{
		readonly FileService fileService;
		List<IViewContent> registeredViews = new List<IViewContent>();
		FileChangeWatcher fileChangeWatcher;
		
		protected override void ChangeFileName(FileName newValue)
		{
			fileService.OpenedFileFileNameChange(this, this.FileName, newValue);
			base.ChangeFileName(newValue);
		}
		
		internal FileServiceOpenedFile(FileService fileService, FileName fileName)
		{
			this.fileService = fileService;
			this.FileName = fileName;
			fileChangeWatcher = new FileChangeWatcher(this);
		}
		
		protected override void UnloadFile()
		{
			bool wasDirty = this.IsDirty;
			fileService.OpenedFileClosed(this);
			base.UnloadFile();
			
			//FileClosed(this, EventArgs.Empty);
			
			if (fileChangeWatcher != null) {
				fileChangeWatcher.Dispose();
				fileChangeWatcher = null;
			}
			
			if (wasDirty) {
				// We discarded some information when closing the file,
				// so we need to re-parse it.
				if (SD.FileSystem.FileExists(this.FileName))
					SD.ParserService.ParseAsync(this.FileName).FireAndForget();
				else
					SD.ParserService.ClearParseInformation(this.FileName);
			}
		}
		
		public override void SaveToDisk(FileName fileName)
		{
			try {
				if (fileChangeWatcher != null)
					fileChangeWatcher.Enabled = false;
				base.SaveToDisk(fileName);
			} finally {
				if (fileChangeWatcher != null)
					fileChangeWatcher.Enabled = true;
			}
		}
		
		//public override event EventHandler FileClosed = delegate {};
	}
}
