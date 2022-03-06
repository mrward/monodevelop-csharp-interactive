//
// UsingsCommandHandler.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2022 Microsoft Corporation
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
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.VisualStudio.UI;

namespace MonoDevelop.CSharpInteractive
{
	class UsingsCommandHandler : ICommandHandler
	{
		readonly TextWriter outputTextWriter;

		public UsingsCommandHandler (TextWriter outputTextWriter)
		{
			this.outputTextWriter = outputTextWriter;
		}

		public Task<int> InvokeAsync (InvocationContext context)
		{
			var kernelContext = context.BindingContext.GetService (typeof (KernelInvocationContext)) as KernelInvocationContext;

			if (kernelContext == null) {
				return Task.FromResult (0);
			}

			ScriptOptions scriptOptions = kernelContext.HandlingKernel.GetScriptOptions ();
			if (scriptOptions != null) {
				DisplayUsings (scriptOptions);
			} else {
				string message = GettextCatalog.GetString ("Unable to access Roslyn's ScriptOptions to find usings");
				outputTextWriter.WriteLine (message);
			}

			return Task.FromResult (0);
		}

		void DisplayUsings (ScriptOptions scriptOptions)
		{
			foreach (string import in scriptOptions.Imports) {
				outputTextWriter.WriteLine ("using {0};", import);
			}
		}
	}

	static class KernelExtensions_UsingsCommand
	{
		public static T AddUsingsCommand<T> (this T kernel, TextWriter outputTextWriter)
			where T : Kernel
		{
			var command = new Command ("#!using", GettextCatalog.GetString ("Show active using declarations"));
			command.Handler = new UsingsCommandHandler (outputTextWriter);

			kernel.AddDirective (command);

			return kernel;
		}
	}
}

