//
// VSCodeObjectSource.cs
//
// Author:
//       David Karlaš <david.karlas@xamarin.com>
//
// Copyright (c) 2016 Xamarin, Inc (http://www.xamarin.com)
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

namespace MonoDevelop.Debugger.VsCodeDebugProtocol
{
	static class VSCodeObjectSource
	{
		internal static string Unquote (string text)
		{
			if (string.IsNullOrEmpty (text))
				return text;

			if (text.Length == 1)
				return text;

			int start = 1;
			int end = text.Length - 1;

			if (text[0] != '"') {
				start = 0;
			}

			if (text[text.Length - 1] != '"') {
				end = text.Length;
			}

			var unquoted = new char[end - start];
			bool escaped = false;
			int count = 0;

			for (int i = start; i < end; i++) {
				char c = text[i];

				switch (c) {
					case '\\':
						if (escaped)
							unquoted[count++] = '\\';
						escaped = !escaped;
						break;
					case '0':
						unquoted[count++] = escaped ? '\0' : c;
						escaped = false;
						break;
					case 'a':
						unquoted[count++] = escaped ? '\a' : c;
						escaped = false;
						break;
					case 'b':
						unquoted[count++] = escaped ? '\b' : c;
						escaped = false;
						break;
					case 'n':
						unquoted[count++] = escaped ? '\n' : c;
						escaped = false;
						break;
					case 'r':
						unquoted[count++] = escaped ? '\r' : c;
						escaped = false;
						break;
					case 't':
						unquoted[count++] = escaped ? '\t' : c;
						escaped = false;
						break;
					case 'v':
						unquoted[count++] = escaped ? '\v' : c;
						escaped = false;
						break;
					default:
						unquoted[count++] = c;
						escaped = false;
						break;
				}
			}

			return new string (unquoted, 0, count);
		}
	}
}
