// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;

using ICSharpCode.AvalonEdit.AddIn.Options;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.SharpDevelop.Widgets.MyersDiff;

namespace ICSharpCode.AvalonEdit.AddIn
{
	public static class Utils
	{
		/// <summary>
		/// Copies editor options and default element customizations.
		/// Does not copy the syntax highlighting.
		/// </summary>
		public static void CopySettingsFrom(this TextEditor editor, TextEditor source)
		{
			editor.Options = source.Options;
			string language = source.SyntaxHighlighting != null ? source.SyntaxHighlighting.Name : null;
			CustomizingHighlighter.ApplyCustomizationsToDefaultElements(editor, CustomizedHighlightingColor.FetchCustomizations(language));
			HighlightingOptions.ApplyToRendering(editor, CustomizedHighlightingColor.FetchCustomizations(language));
		}
		
		static IEnumerable<CustomizedHighlightingColor> FetchCustomizations(string languageName)
		{
			return CustomizedHighlightingColor.FetchCustomizations(languageName);
		}
	}
}
