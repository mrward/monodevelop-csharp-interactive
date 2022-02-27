//
// repl.cs: Support for using the compiler in interactive mode (read-eval-print loop)
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2004, 2005, 2006, 2007, 2008 Novell, Inc
// Copyright 2011-2013 Xamarin Inc

using System;
using System.Collections;
using System.IO;

namespace Mono.CSharp
{
	static class PrettyPrinter
	{
		internal static void PrettyPrint (TextWriter output, object result)
		{
			if (result == null) {
				p (output, "null");
				return;
			}

			if (result is Array) {
				Array a = (Array)result;

				p (output, "{ ");
				int top = a.GetUpperBound (0);
				for (int i = a.GetLowerBound (0); i <= top; i++) {
					PrettyPrint (output, a.GetValue (i));
					if (i != top)
						p (output, ", ");
				}
				p (output, " }");
			} else if (result is bool) {
				if ((bool)result)
					p (output, "true");
				else
					p (output, "false");
			} else if (result is string) {
				p (output, "\"");
				EscapeString (output, (string)result);
				p (output, "\"");
			} else if (result is IDictionary) {
				IDictionary dict = (IDictionary)result;
				int top = dict.Count, count = 0;

				p (output, "{");
				foreach (DictionaryEntry entry in dict) {
					count++;
					p (output, "{ ");
					PrettyPrint (output, entry.Key);
					p (output, ", ");
					PrettyPrint (output, entry.Value);
					if (count != top)
						p (output, " }, ");
					else
						p (output, " }");
				}
				p (output, "}");
			} else if (WorksAsEnumerable (result)) {
				int i = 0;
				p (output, "{ ");
				foreach (object item in (IEnumerable)result) {
					if (i++ != 0)
						p (output, ", ");

					PrettyPrint (output, item);
				}
				p (output, " }");
			} else if (result is char) {
				EscapeChar (output, (char)result);
			} else {
				p (output, result.ToString ());
			}
		}


		static void p (TextWriter output, string s)
		{
			output.Write (s);
		}

		static void EscapeString (TextWriter output, string s)
		{
			foreach (var c in s) {
				if (c >= 32) {
					output.Write (c);
					continue;
				}
				switch (c) {
					case '\"':
						output.Write ("\\\""); break;
					case '\a':
						output.Write ("\\a"); break;
					case '\b':
						output.Write ("\\b"); break;
					case '\n':
						output.Write ("\\n");
						break;

					case '\v':
						output.Write ("\\v");
						break;

					case '\r':
						output.Write ("\\r");
						break;

					case '\f':
						output.Write ("\\f");
						break;

					case '\t':
						output.Write ("\\t");
						break;

					default:
						output.Write ("\\x{0:x}", (int)c);
						break;
				}
			}
		}

		static void EscapeChar (TextWriter output, char c)
		{
			if (c == '\'') {
				output.Write ("'\\''");
				return;
			}
			if (c >= 32) {
				output.Write ("'{0}'", c);
				return;
			}
			switch (c) {
				case '\a':
					output.Write ("'\\a'");
					break;

				case '\b':
					output.Write ("'\\b'");
					break;

				case '\n':
					output.Write ("'\\n'");
					break;

				case '\v':
					output.Write ("'\\v'");
					break;

				case '\r':
					output.Write ("'\\r'");
					break;

				case '\f':
					output.Write ("'\\f'");
					break;

				case '\t':
					output.Write ("'\\t");
					break;

				default:
					output.Write ("'\\x{0:x}'", (int)c);
					break;
			}
		}

		// Some types (System.Json.JsonPrimitive) implement
		// IEnumerator and yet, throw an exception when we
		// try to use them, helper function to check for that
		// condition
		static internal bool WorksAsEnumerable (object obj)
		{
			IEnumerable enumerable = obj as IEnumerable;
			if (enumerable != null) {
				try {
					enumerable.GetEnumerator ();
					return true;
				} catch {
					// nothing, we return false below
				}
			}
			return false;
		}
	}
}
