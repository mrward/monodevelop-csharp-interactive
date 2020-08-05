//
// MemberResolver.cs
// Based on https://github.com/mono/debugger-libs/blob/master/Mono.Debugging.Soft/SoftDebuggerAdaptor.cs
//
// Authors: Lluis Sanchez Gual <lluis@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
//          Matt Ward <matt.ward@microsoft.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// Copyright (c) 2011,2012 Xamain Inc. (http://www.xamarin.com)
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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Debugging.Client;
using Mono.Debugging.Evaluation;

namespace Mono.Debugging.Soft
{
	class MemberResolver
	{
		public MemberResolver ()
		{
		}

		public IEnumerable<ValueReference> GetMembers (
			EvaluationContext ctx,
			IObjectSource objectSource,
			object t,
			object value,
			BindingFlags bindingFlags)
		{
			var subProps = new Dictionary<string, PropertyInfo> ();
			var type = t as Type;
			Type realType = null;

			if (value != null && (bindingFlags & BindingFlags.Instance) != 0)
				realType = GetValueType (value) as Type;

			// First of all, get a list of properties overridden in sub-types
			while (realType != null && realType != type) {
				foreach (PropertyInfo property in realType.GetProperties (bindingFlags | BindingFlags.DeclaredOnly)) {
					MethodInfo method = property.GetGetMethod (true);

					if (method == null || method.GetParameters ().Length != 0 || method.IsAbstract || !method.IsVirtual || method.IsStatic)
						continue;

					if (method.IsPublic && ((bindingFlags & BindingFlags.Public) == 0))
						continue;

					if (!method.IsPublic && ((bindingFlags & BindingFlags.NonPublic) == 0))
						continue;

					subProps[property.Name] = property;
				}

				realType = realType.BaseType;
			}

			var tupleNames = GetTupleElementNames (objectSource, ctx);
			bool hasExplicitInterface = false;
			while (type != null) {
				var fieldsBatch = new FieldReferenceBatch (value);
				foreach (var field in type.GetFields (bindingFlags)) {
					if (field.IsStatic) {
						yield return new FieldValueReference (ctx, field, value, type);
					} else {
						fieldsBatch.Add (field);
						string vname = MapTupleName (field.Name, tupleNames);
						yield return new FieldValueReference (ctx, field, value, type, vname, ObjectValueFlags.Field, fieldsBatch);
					}
				}

				foreach (var prop in type.GetProperties (bindingFlags)) {
					var getter = prop.GetGetMethod (true);

					if (getter == null || getter.GetParameters ().Length != 0 || getter.IsAbstract)
						continue;

					if (getter.IsStatic && ((bindingFlags & BindingFlags.Static) == 0))
						continue;

					if (!getter.IsStatic && ((bindingFlags & BindingFlags.Instance) == 0))
						continue;

					if (getter.IsPublic && ((bindingFlags & BindingFlags.Public) == 0))
						continue;

					//This is only possible in case of explicitly implemented interface property, which we handle later
					if (getter.IsVirtual && getter.IsPrivate) {
						hasExplicitInterface = true;
						continue;
					}

					if (!getter.IsPublic && ((bindingFlags & BindingFlags.NonPublic) == 0))
						continue;

					// If a property is overriden, return the override instead of the base property
					PropertyInfo overridden;
					if (getter.IsVirtual && subProps.TryGetValue (prop.Name, out overridden)) {
						getter = overridden.GetGetMethod (true);
						if (getter == null)
							continue;

						yield return new PropertyValueReference (ctx, overridden, value, overridden.DeclaringType, getter, null);
					} else {
						yield return new PropertyValueReference (ctx, prop, value, type, getter, null);
					}
				}

				if ((bindingFlags & BindingFlags.DeclaredOnly) != 0)
					break;

				type = type.BaseType;
			}

			type = t as Type;
			if (type == null ||
				!hasExplicitInterface ||
				(bindingFlags & BindingFlags.Instance) == 0 ||
				(bindingFlags & BindingFlags.Public) == 0) {
				yield break;
			}

			var interfaces = type.GetInterfaces ();
			foreach (Type intr in interfaces) {
				var map = type.GetInterfaceMap (intr);
				foreach (PropertyInfo prop in intr.GetProperties (bindingFlags)) {
					var getter = prop.GetGetMethod (true);
					if (getter == null || getter.GetParameters ().Length != 0)
						continue;
					var implementationGetter = map.TargetMethods[Array.IndexOf (map.InterfaceMethods, getter)];
					//We are only intersted into private(explicit) implementations because public ones are already handled before
					if (implementationGetter.IsPublic)
						continue;
					yield return new PropertyValueReference (ctx, prop, value, type, getter, null);
				}
			}
		}

		public static object GetValueType (object value)
		{
			if (value == null)
				return typeof (object);

			return value.GetType ();
		}

		string[] GetTupleElementNames (IObjectSource source, EvaluationContext ctx)
		{
			switch (source) {
				//case FieldValueReference field:
				//	if ((field.Type as TypeMirror)?.Name?.StartsWith ("ValueTuple`", StringComparison.Ordinal) != true)
				//		return null;
				//	return field.GetTupleElementNames ();
				//case PropertyValueReference prop:
				//	if ((prop.Type as TypeMirror)?.Name?.StartsWith ("ValueTuple`", StringComparison.Ordinal) != true)
				//		return null;
				//	return prop.GetTupleElementNames ();
				//case VariableValueReference variable:
				//	if ((variable.Type as TypeMirror)?.Name?.StartsWith ("ValueTuple`", StringComparison.Ordinal) != true)
				//		return null;
				//	return variable.GetTupleElementNames ((SoftEvaluationContext)ctx);
				default:
					return null;
			}
		}

		static string MapTupleName (string name, string[] tupleNames)
		{
			if (tupleNames != null &&
				name.Length > 4 &&
				name.StartsWith ("Item", StringComparison.Ordinal) &&
				int.TryParse (name.Substring (4), out var tupleIndex) &&
				tupleNames.Length >= tupleIndex &&
				tupleNames[tupleIndex - 1] != null)
				return tupleNames[tupleIndex - 1];

			return name;
		}

		public ValueReference GetMember (
			EvaluationContext ctx,
			IObjectSource objectSource,
			object t,
			object value,
			string name)
		{
			var type = t as Type;
			var tupleNames = GetTupleElementNames (objectSource, ctx);
			while (type != null) {
				var field = FindByName (type.GetFields (), f => MapTupleName (f.Name, tupleNames), name, ctx.CaseSensitive);

				if (field != null && (field.IsStatic || value != null))
					return new FieldValueReference (ctx, field, value, type);

				var prop = FindByName (type.GetProperties (), p => p.Name, name, ctx.CaseSensitive);

				if (prop != null && (IsStatic (prop) || value != null)) {
					var getter = prop.GetGetMethod (true);
					// Optimization: if the property has a CompilerGenerated backing field, use that instead.
					// This way we avoid overhead of invoking methods on the debugee when the value is requested.
					//But also check that this method is not virtual, because in that case we need to call getter to invoke override
					if (!getter.IsVirtual) {
						var cgFieldName = string.Format ("<{0}>{1}", prop.Name, type.IsAnonymousType () ? "" : "k__BackingField");
						if ((field = FindByName (type.GetFields (), f => f.Name, cgFieldName, true)) != null && IsCompilerGenerated (field))
							return new FieldValueReference (ctx, field, value, type, prop.Name, ObjectValueFlags.Property);
					}
					// Backing field not available, so do things the old fashioned way.
					return getter != null ? new PropertyValueReference (ctx, prop, value, type, getter, null) : null;
				}
				if (type.IsInterface) {
					foreach (var inteface in type.GetInterfaces ()) {
						var result = GetMember (ctx, null, inteface, value, name);
						if (result != null)
							return result;
					}
					//foreach above recursively checked all "base" interfaces
					//nothing was found, quit, otherwise we would loop forever
					return null;
				}

				type = type.BaseType;
			}

			return null;
		}

		static bool IsStatic (PropertyInfo prop)
		{
			var method = prop.GetGetMethod (true) ?? prop.GetSetMethod (true);
			return method.IsStatic;
		}

		static bool IsCompilerGenerated (FieldInfo field)
		{
			var attrs = field.GetCustomAttributes (true);
			var generated = TypeExtensions.GetAttribute<CompilerGeneratedAttribute> (attrs);

			return generated != null;
		}

		static T FindByName<T> (IEnumerable<T> items, Func<T, string> getName, string name, bool caseSensitive)
		{
			var best = default (T);

			foreach (var item in items) {
				var itemName = getName (item);

				if (itemName == name)
					return item;

				if (!caseSensitive && itemName.Equals (name, StringComparison.CurrentCultureIgnoreCase))
					best = item;
			}

			return best;
		}
	}
}
