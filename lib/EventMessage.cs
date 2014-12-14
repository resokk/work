using System;
using System.Collections.Generic;

namespace CacheEngine
{
	[Serializable]
	public class EventMessage
	{
		public Guid Id { get; set; }

		public DateTime Date { get; set; }

		public EventType Type { get; set; }
	
		public IEnumerable<EventMessageRow> Data { get; set; }

		public IEnumerable<Recepient> Recepients { get; set; }	
	}

	[Serializable]
	public class EventMessageRow
	{
		public string Key { get; set; }

		public string Value { get; set; }
	}

	[Serializable]
	public class Recepient
	{
		public string System { get; set; }

		public string Code { get; set; }

		public string Channel { get; set; }

		public string ChannelAddress { get; set; }

		public string Title { get; set; }
	}
		
	[Serializable]
	public class EventType
	{
		public string System { get; set; }

		public string Code { get; set; }
	}

	[Serializable]
	public class Event
	{
		public Guid Id { get; set; }

		public object Message { get; set; }

		public Recepient Recepient { get; set; }

		public string Header { get; set; }

		public string Body { get; set; }
	}
}

