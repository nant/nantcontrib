//
// NAntContrib - NAntAddin
// Copyright (C) 2002 Jayme C. Edwards (jedwards@wi.rr.com)
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//

using System;
using System.Reflection;
using System.Collections;
using System.Threading;
using System.Reflection.Emit;
using System.Globalization;
using System.ComponentModel;

namespace NAnt.Contrib.NAntAddin.Nodes
{
	/// <summary>
	/// Must be implemented by objects that 
	/// are used by the Properties Window to 
	/// edit. <see cref="FileSet"/> and 
	/// <see cref="NDocDocumenter"/> both 
	/// implement this interface. So do 
	/// all instances of 
	/// <see cref="NAntBaseNode"/>.
	/// </summary>
	/// <remarks>None.</remarks>
	public interface ConstructorArgsResolver
	{
		/// <summary>
		/// Returns the arguments that must be passed to 
		/// the constructor of an object to create the 
		/// same object.
		/// </summary>
		/// <returns>The arguments to pass.</returns>
		/// <remarks>None.</remarks>
		Object[] GetConstructorArgs();
	}

	/// <summary>
	/// Returns a Proxy node for any object 
	/// that marks its Properties as read only. 
	/// </summary>
	/// <remarks>None.</remarks>
	public class NAntReadOnlyNodeBuilder
	{
		private static AssemblyName assemblyName;
		private static AssemblyBuilder assemblyBuilder;
		private static ModuleBuilder typeDefiner;
		private static Hashtable typeBuilders;

		private NAntReadOnlyNodeBuilder() {}

		/// <summary>
		/// Returns a Proxy node for any object that marks
		/// its Properties read only.
		/// </summary>
		/// <param name="BaseNode">The object to create a read only Proxy for.</param>
		/// <returns>The new read only Proxy object.</returns>
		/// <remarks>None.</remarks>
		public static object GetReadOnlyNode(ConstructorArgsResolver BaseNode)
		{
			if (typeDefiner == null)
			{
				// Create the Assembly
				assemblyName = new AssemblyName();
				assemblyName.Name = "NAntAddinNodeProxy";

				assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(
					assemblyName, AssemblyBuilderAccess.RunAndSave);
			
				typeDefiner = assemblyBuilder.DefineDynamicModule(
					"NAntAddinNodeProxy", "NAntAddinNodeProxy.dll");

				typeBuilders = new Hashtable();
			}
			
			TypeBuilder typeBuilder = (TypeBuilder)typeBuilders[BaseNode.GetType().FullName + "_ReadOnly"];
			if (typeBuilder == null)
			{
				//
				// Define Constructor
				//

				ConstructorInfo baseConstructor = null;
				ParameterInfo[] baseConstructorParamTypes = null;
				Type[] baseConstructorTypes = null;

				// Find the base Constructor of the class we're creating a Proxy for
				//
				MethodInfo[] baseMethods = BaseNode.GetType().GetMethods();
				ConstructorInfo[] baseConstructors = BaseNode.GetType().GetConstructors();
				for (int i = 0; i < baseConstructors.Length; i++)
				{
					ConstructorInfo baseInfo = (ConstructorInfo)baseConstructors.GetValue(i);
					if (baseInfo.GetParameters().Length > 0)
					{
						baseConstructor = baseInfo;
						baseConstructorParamTypes = baseConstructor.GetParameters();

						baseConstructorTypes = new Type[baseConstructorParamTypes.Length];
						for (int j = 0; j < baseConstructorParamTypes.Length; j++)
						{
							baseConstructorTypes[j] = baseConstructorParamTypes[j].ParameterType;
						}

						break;
					}
				}

				// Create the Type
				typeBuilder = typeDefiner.DefineType(
					BaseNode.GetType().FullName + "_ReadOnly", TypeAttributes.Public);

				// Set the Type's Base Class
				typeBuilder.SetParent(BaseNode.GetType());

				// Define the Type's Constructor
				ConstructorBuilder constructor = typeBuilder.DefineConstructor(
					MethodAttributes.Public, 
					CallingConventions.Standard, 
					baseConstructorTypes);

				ILGenerator generator = constructor.GetILGenerator();

				generator.Emit(OpCodes.Ldarg_0);

				// Load the Constructor arguments
				for (int i = 0; i < baseConstructorTypes.Length; i++)
				{
					if (i == 0)
					{
						generator.Emit(OpCodes.Ldarg_1);
					}
					else if (i == 1)
					{
						generator.Emit(OpCodes.Ldarg_2);
					}
					else if (i == 2)
					{
						generator.Emit(OpCodes.Ldarg_3);
					}
				}
				
				// Get the Base Constructor
				ConstructorInfo superConstructor = 
					BaseNode.GetType().GetConstructor(baseConstructorTypes);

				// Call the Base Constructor and return
				generator.Emit(OpCodes.Call, baseConstructor);
				generator.Emit(OpCodes.Ret);

				//
				// Define Properties
				//

				// Find the base Properties of the class we're creating a Proxy for
				//
				PropertyInfo[] baseProps = BaseNode.GetType().GetProperties();
				for (int i = 0; i < baseProps.Length; i++)
				{
					PropertyInfo baseInfo = (PropertyInfo)baseProps.GetValue(i);

					object[] browseableAttrs = baseInfo.GetCustomAttributes(
						typeof(BrowsableAttribute), true);

					if (browseableAttrs.Length == 0)
					{
						PropertyBuilder property = typeBuilder.DefineProperty(
							baseInfo.Name, PropertyAttributes.None, 
							baseInfo.PropertyType, null);

						Type readOnlyType = typeof(ReadOnlyAttribute);
						ConstructorInfo readOnlyConst = readOnlyType.GetConstructor(
							new Type[] { typeof(Boolean) });

						CustomAttributeBuilder propCustAttr = new CustomAttributeBuilder(
							readOnlyConst, new Object[] { true });

						property.SetCustomAttribute(propCustAttr);

						MethodBuilder propGetMethod = null;
						MethodBuilder propSetMethod = null;

						if (baseInfo.CanRead)
						{
							propGetMethod = typeBuilder.DefineMethod(
								"get_" + baseInfo.Name, 
								MethodAttributes.Public | 
								MethodAttributes.HideBySig | 
								MethodAttributes.SpecialName, 
								baseInfo.PropertyType, new Type[0]);

							ILGenerator propGetGen = propGetMethod.GetILGenerator();
							LocalBuilder localProp = propGetGen.DeclareLocal(baseInfo.PropertyType);
							propGetGen.Emit(OpCodes.Ldarg_0);
							propGetGen.Emit(OpCodes.Call, baseInfo.GetGetMethod());
							propGetGen.Emit(OpCodes.Stloc_0);
							Label label = propGetGen.DefineLabel();
							propGetGen.Emit(OpCodes.Br_S, label);
							propGetGen.MarkLabel(label);
							propGetGen.Emit(OpCodes.Ldloc_0);
							propGetGen.Emit(OpCodes.Ret);

							property.SetGetMethod(propGetMethod);
							
						}
						if (baseInfo.CanWrite)
						{
							propSetMethod = typeBuilder.DefineMethod(
								"set_" + baseInfo.Name, 
								MethodAttributes.Public | 
								MethodAttributes.HideBySig | 
								MethodAttributes.SpecialName, 
								typeof(void), new Type[] { baseInfo.PropertyType });

							ILGenerator propSetGen = propSetMethod.GetILGenerator();
							propSetGen.Emit(OpCodes.Nop);
							propSetGen.Emit(OpCodes.Ret);

							property.SetSetMethod(propSetMethod);
						}
					}
				}

				typeBuilder.CreateType();

				typeBuilders.Add(BaseNode.GetType().FullName + "_ReadOnly", typeBuilder);
			}

			object proxyNode = null;

			Object[] constructorArgs = BaseNode.GetConstructorArgs();

			proxyNode = assemblyBuilder.CreateInstance(
				BaseNode.GetType().FullName + "_ReadOnly", false, 
				BindingFlags.Default, null, 
				constructorArgs, 
				null, new object[0]);

			return proxyNode;
		}
	}
}