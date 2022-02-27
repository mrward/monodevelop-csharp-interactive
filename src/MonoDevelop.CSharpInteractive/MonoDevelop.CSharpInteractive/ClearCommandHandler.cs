﻿//
// ClearCommandHandler.cs
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
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.VisualStudio.UI;

namespace MonoDevelop.CSharpInteractive
{
	class ClearCommandHandler : ICommandHandler
	{
		public static Action OnClear = () => { };

		public Task<int> InvokeAsync (InvocationContext context)
		{
			OnClear ();
			return Task.FromResult (0);
		}
	}

	static class KernelExtensions_ClearCommand
	{
		public static T AddClearCommand<T> (this T kernel)
			where T : Kernel
		{
			var command = new Command ("#!clear", GettextCatalog.GetString ("Clears the output"));
			command.Handler = new ClearCommandHandler ();

			kernel.AddDirective (command);

			return kernel;
		}
	}
}

