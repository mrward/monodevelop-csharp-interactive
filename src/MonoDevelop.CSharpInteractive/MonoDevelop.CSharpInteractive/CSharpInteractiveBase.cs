//
// CSharpInteractiveBase.cs
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
	class CSharpInteractiveBase
	{
		public static TextWriter Output = Console.Out;

		public static TextWriter Error = Console.Error;

		public static Evaluator Evaluator;

		internal static Action OnClear = () => { };

		internal static Action<object> OnInspect = obj => { };

		public static string help =
			"Static methods:\n" +
			"  Describe(object);       - Describes the object's type\n" +
			"  Inpect(object);         - Displays object in Object Inspector\n" +
			"  LoadAssembly(assembly); - Loads the given assembly (like -r:ASSEMBLY)\n" +
			"  ShowVars();             - Shows defined local variables.\n" +
			"  ShowUsing();            - Show active using declarations.\n" +
			"  Time(() => { });        - Times the specified code\n" +
			"  print(obj);             - Shorthand for Console.WriteLine\n" +
			"  clear();                - Clears the output\n" +
			"  help;                   - This help text\n";

		public static void ShowVars ()
		{
			//Output.Write (Evaluator.GetVars ());
			Output.Flush ();
		}

		public static void ShowUsing ()
		{
			//Output.Write (Evaluator.GetUsing ());
			Output.Flush ();
		}

		public static TimeSpan Time (Action a)
		{
			DateTime now = DateTime.Now;
			a ();
			return DateTime.Now - now;
		}

		public static void LoadAssembly (string assembly)
		{
			//Evaluator.LoadAssembly (assembly);
		}

		public static void print (object obj)
		{
			Output.WriteLine (obj);
		}

		public static void print (string fmt, params object[] args)
		{
			Output.WriteLine (fmt, args);
		}

		public static string Describe (object x)
		{
			//if (x == null) {
			//	return "<null>";
			//}
			//Type t = (x as Type) ?? x.GetType ();
			//StringWriter stringWriter = new StringWriter ();
			//new Outline (t, stringWriter, declared_only: true, show_private: false, filter_obsolete: false).OutlineType ();
			//return stringWriter.ToString ();
			return string.Empty;
		}

		public static void clear ()
		{
			OnClear ();
		}

		public static void Inspect (object obj)
		{
			OnInspect (obj);
		}
	}
}
