//
// ConsoleOutputTextWriter.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2020 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

namespace MonoDevelop.CSharpInteractive
{
	/// <summary>
	/// IDE maps the Console.Out and Console.Error so they write to the IDE log. For the
	/// C# interactive window we need to temporarily replace these so the output goes to the
	/// window not the IDE log. Otherwise code that uses Console.WriteLine will not show any
	/// output in the interactive window.
	/// </summary>
	class ConsoleOutputTextWriter : IDisposable
	{
		readonly TextWriter originalOutputWriter;
		readonly TextWriter originalErrorWriter;

		ConsoleOutputTextWriter (TextWriter originalOutputWriter, TextWriter originalErrorWriter)
		{
			this.originalOutputWriter = originalOutputWriter;
			this.originalErrorWriter = originalErrorWriter;
		}

		public static ConsoleOutputTextWriter Create (TextWriter outputWriter)
		{
			return Create (outputWriter, outputWriter);
		}

		public static ConsoleOutputTextWriter Create (TextWriter outputWriter, TextWriter errorWriter)
		{
			var consoleOutputWriter = new ConsoleOutputTextWriter (Console.Out, Console.Error);

			ReplaceConsoleWriters (outputWriter, errorWriter);

			return consoleOutputWriter;
		}

		static void ReplaceConsoleWriters (TextWriter outputWriter, TextWriter errorWriter)
		{
			Console.SetOut (outputWriter);
			Console.SetError (errorWriter);
		}

		public void Dispose ()
		{
			ReplaceConsoleWriters (originalOutputWriter, originalErrorWriter);
		}
	}
}
