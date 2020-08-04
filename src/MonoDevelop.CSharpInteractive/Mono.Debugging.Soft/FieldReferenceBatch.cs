// 
// FieldReferenceBatch.cs
//  
// Authors: David Karlaš <david.karlas@xamarin.com>
// 
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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

using System.Collections.Generic;
using System.Reflection;

namespace Mono.Debugging.Soft
{
	class FieldReferenceBatch
	{
		readonly object locker = new object ();
		readonly object obj;
		List<FieldInfo> fields = new List<FieldInfo> ();
		object[] results;

		public FieldReferenceBatch (object obj)
		{
			this.obj = obj;
		}

		public void Add (FieldInfo fieldVal)
		{
			lock (locker) {
				fields.Add (fieldVal);
			}
		}

		public object GetValue (FieldInfo field)
		{
			lock (locker) {
				return field.GetValue (obj);
				//if (results == null || fields.Count != results.Length)
				//	results = ((ObjectMirror)obj).GetValues (fields);
				//return results[fields.IndexOf (field)];
			}
		}

		public void Invalidate ()
		{
			lock (locker) {
				results = null;
			}
		}
	}
}
