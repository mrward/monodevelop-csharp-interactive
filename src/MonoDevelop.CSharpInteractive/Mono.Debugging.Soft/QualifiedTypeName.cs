// 
// From SoftDebuggerAdaptor.cs
//  
// Authors: Lluis Sanchez Gual <lluis@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// Copyright (c) 2011,2012 Xamain Inc. (http://www.xamarin.com)
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

namespace Mono.Debugging.Soft
{
	static class QualifiedTypeName
	{
		public static string GetName (string name, object[] typeArgs)
		{
			int i = name.IndexOf (',');
			if (i != -1) {
				// Find first comma outside brackets
				int nest = 0;
				for (int n = 0; n < name.Length; n++) {
					char c = name[n];
					if (c == '[')
						nest++;
					else if (c == ']')
						nest--;
					else if (c == ',' && nest == 0) {
						name = name.Substring (0, n).Trim ();
						break;
					}
				}
			}

			if (typeArgs != null && typeArgs.Length > 0) {
				string args = "";

				foreach (var argType in typeArgs) {
					if (args.Length > 0)
						args += ",";

					string tn;

					if (!(argType is Type atm)) {
						var att = (Type)argType;

						tn = att.FullName + ", " + att.Assembly.GetName ();
					} else {
						tn = atm.FullName + ", " + atm.Assembly.GetName ();
					}

					if (tn.IndexOf (',') != -1)
						tn = "[" + tn + "]";

					args += tn;
				}

				name += "[" + args + "]";
			}

			return name;
		}
	}
}
