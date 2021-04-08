using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using System;
using System.Runtime.Serialization;

namespace Frame
{
	[ILAdapter]
	public class ISerializableAdaptorClassInheritanceAdaptor : CrossBindingAdaptor
	{
		public override Type AdaptorType
		{
			get
			{
				return typeof(ISerializableAdaptorAdaptor);
			}
		}

		public override Type BaseCLRType
		{
			get
			{
				return typeof(ISerializable);
			}
		}

		public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
		{
			return new ISerializableAdaptorAdaptor(appdomain, instance);
		}

		public class ISerializableAdaptorAdaptor : ISerializable, CrossBindingAdaptorType
		{
			private readonly object[] param0 = new object[2];
			private ILRuntime.Runtime.Enviorment.AppDomain appDomain;
			private IMethod iGetObjectData;
			private ILTypeInstance instance;

			public ISerializableAdaptorAdaptor()
			{
			}

			public ISerializableAdaptorAdaptor(ILRuntime.Runtime.Enviorment.AppDomain appDomain, ILTypeInstance instance)
			{
				this.appDomain = appDomain;
				this.instance = instance;
			}

			public ILTypeInstance ILInstance
			{
				get
				{
					return instance;
				}
			}

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				if (this.iGetObjectData == null)
				{
					this.iGetObjectData = instance.Type.GetMethod("GetObjectData", 2);
				}
				param0[0] = info;
				param0[1] = context;
				this.appDomain.Invoke(this.iGetObjectData, instance, this.param0);
			}
		}
	}
}