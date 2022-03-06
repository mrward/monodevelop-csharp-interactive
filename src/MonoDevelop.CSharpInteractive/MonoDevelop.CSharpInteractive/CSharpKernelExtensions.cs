//
// CSharpKernelExtensions.cs
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

using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;

namespace MonoDevelop.CSharpInteractive
{
	static class CSharpKernelExtensions
	{
		public static ScriptOptions GetScriptOptions (this CSharpKernel kernel)
		{
			var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
			var field = typeof (CSharpKernel).GetField ("ScriptOptions", bindingFlags);
			return field?.GetValue (kernel) as ScriptOptions;
		}

		public static ScriptOptions GetScriptOptions (this Kernel kernel)
		{
			if (kernel is CSharpKernel csharpKernel) {
				return GetScriptOptions (csharpKernel);
			}
			return null;
		}
	}
}

