using System;
using MsgPack;
using MsgPack.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;
using System.Text;
using EasyNetQ;
using System.Threading.Tasks;

namespace CacheEngine
{
	public class Ser : ISerializer
	{

		#region ISerializer implementation
		public byte[] MessageToBytes<T> (T message) where T : class
		{
			var ps = MessagePackSerializer.Get<T> ();
			var res = ps.PackSingleObject (message);
			return res;
		}
		public T BytesToMessage<T> (byte[] bytes)
		{
			var ps = MessagePackSerializer.Get<T> ();
			var res = ps.UnpackSingleObject (bytes);
			return res;
		}
		public object BytesToMessage (string typeName, byte[] bytes)
		{
			var t = Type.GetType (typeName);
			var ps = MessagePackSerializer.Get (t);
			var res = ps.UnpackSingleObject (bytes);
			return res;
		}
		#endregion
	}

	public class TypeNameSer : ITypeNameSerializer
	{
		#region ITypeNameSerializer implementation
		public string Serialize (Type type)
		{
			return type.AssemblyQualifiedName;
		}
		public Type DeSerialize (string typeName)
		{
			return Type.GetType (typeName);
		}
		#endregion
	}

	public class Log : IEasyNetQLogger
	{
		#region IEasyNetQLogger implementation

		public void DebugWrite (string format, params object[] args)
		{

		}

		public void InfoWrite (string format, params object[] args)
		{
		}

		public void ErrorWrite (string format, params object[] args)
		{
			var l = new EasyNetQ.Loggers.ConsoleLogger ();
			l.ErrorWrite (format, args);
		}

		public void ErrorWrite (Exception exception)
		{
			var l = new EasyNetQ.Loggers.ConsoleLogger ();
			l.ErrorWrite (exception);
		}

		#endregion


	}

	class MainClass
	{
		public static void Main (string[] args)
		{
			var e = new Event () {
				Id = Guid.NewGuid (),
				Header = "Снятие комисии со счета 123123123",
				Body = "For more control over how an object is serialized the JsonSerializer can be used directly. The JsonSerializer is able to read and write JSON text directly to a stream via JsonTextReader and JsonTextWriter. Other kinds of JsonWriters can also be used such as JTokenReader/JTokenWriter to convert your object to and from LINQ to JSON objects or BsonReader/BsonWriter to convert to and from BSON.",
				Message = new EventMessage () {
					Id = Guid.NewGuid (),
					Date = DateTime.Now,
					Data = new [] {
						new EventMessageRow () {
							Key = "amount",
							Value = "500"
						},
						new EventMessageRow () {
							Key = "currency",
							Value = "USD"
						},
						new EventMessageRow () {
							Key = "reason",
							Value = "Оформление ПС"
						},
						new EventMessageRow () {
							Key = "client",
							Value = "ООО Ромашка"
						},
						new EventMessageRow () {
							Key = "date",
							Value = "2014-11-10"
						}
					},
					Recepients = new [] {
						new Recepient () {
							System = "EQ",
							Code = "WSLM",
							Title = "Снеговой Александр Сергеевич",
							Channel = "email",
							ChannelAddress = "resokk@gmail.com"
						},
						new Recepient () {
							System = "EQ",
							Code = "WSLM",
							Title = "Сокк Александр Сергеевич",
							Channel = "sms",
							ChannelAddress = "+79260296795"
						},
					},
					Type = new EventType () {
						System = "EQ",
						Code = "commision"
					}
				},
				Recepient = new Recepient () {
					System = "EQ",
					Code = "WSLM",
					Title = "Сокк Александр Сергеевич",
					Channel = "sms",
					ChannelAddress = "+79260296795"
				}
			};


			using (var cl = RabbitHutch.CreateBus ("host=localhost;prefetchcount=100", s => {
				s.Register<ISerializer, Ser>();
				s.Register<IEasyNetQLogger, Log>();
				s.Register<ITypeNameSerializer, TypeNameSer>();
			})) {
				for (int i = 0; i < 100000; i++) {
					cl.Send ("queue", e);
				}
			}

			var cl1 = RabbitHutch.CreateBus ("host=localhost;prefetchcount=100", s => {
				s.Register<ISerializer, Ser>();
				s.Register<IEasyNetQLogger, Log>();
				s.Register<ITypeNameSerializer, TypeNameSer>();
			});
			var cl2 = RabbitHutch.CreateBus ("host=localhost;prefetchcount=100", s => {
				s.Register<IEasyNetQLogger, Log>();
				s.Register<ISerializer, Ser>();
				s.Register<ITypeNameSerializer, TypeNameSer>();
			});
			var cl3 = RabbitHutch.CreateBus ("host=localhost;prefetchcount=100", s => {
				s.Register<IEasyNetQLogger, Log>();
				s.Register<ISerializer, Ser>();
				s.Register<ITypeNameSerializer, TypeNameSer>();
			});


			cl1.Receive<Event> ("queue", t =>
				Task.Factory.StartNew (() => {
					Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
					Thread.Sleep(200);
				})
			);

			cl2.Receive<Event> ("queue", t =>
				Task.Factory.StartNew (() => {
					Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
					Thread.Sleep(200);
				})
			);

			cl3.Receive<Event> ("queue", t =>
				Task.Factory.StartNew (() => {
					Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
					Thread.Sleep(200);
				})
			);

			Console.ReadKey ();
			cl1.Dispose ();
			cl2.Dispose ();
			cl3.Dispose ();


		}
	}
}
