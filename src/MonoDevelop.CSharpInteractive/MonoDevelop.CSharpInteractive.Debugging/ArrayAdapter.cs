//
// ArrayAdapter.cs
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
using Mono.Debugging.Evaluation;

namespace MonoDevelop.CSharpInteractive.Debugging
{
	class ArrayAdapter : ICollectionAdaptor
	{
		readonly EvaluationContext context;
		readonly Array array;
		readonly Type type;
		readonly Type elementType;
		int[] dimensions;
		int[] bounds;

		public ArrayAdapter (EvaluationContext context, object value)
		{
			this.context = context;
			array = value as Array;

			type = value.GetType ();
			elementType = type.GetElementType ();
		}

		public object ElementType { get => elementType; }

		public int[] GetDimensions ()
		{
			if (dimensions == null) {
				dimensions = new int[array.Rank];
				for (int i = 0; i < array.Rank; i++) {
					dimensions[i] = array.GetLength (i);
				}
			}

			return dimensions;
		}

		public object GetElement (int[] indices)
		{
			return array.GetValue (indices);
		}

		public int[] GetLowerBounds ()
		{
			if (bounds == null) {
				bounds = new int[array.Rank];
				for (int i = 0; i < array.Rank; i++) {
					bounds[i] = array.GetLowerBound (i);
				}
			}

			return bounds;
		}

		public void SetElement (int[] indices, object val)
		{
			throw new NotImplementedException ();
		}

		public Array GetElements (int[] indices, int count)
		{
			int i = GetIndex (indices);

			var newArray = Array.CreateInstance (elementType, count);
			for (int j = 0; j < count; ++j) {
				var val = array.GetValue (i + j);
				newArray.SetValue (val, j);
			}

			return newArray;
		}

		int GetIndex (int[] indices)
		{
			int ts = 1;
			int i = 0;
			int[] dims = GetDimensions ();
			var lowerBounds = GetLowerBounds ();
			for (int n = indices.Length - 1; n >= 0; n--) {
				i += (indices[n] - lowerBounds[n]) * ts;
				ts *= dims[n];
			}
			return i;
		}
	}
}
