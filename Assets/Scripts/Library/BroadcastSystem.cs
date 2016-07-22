using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct BroadcastInfo
{
	public object from;
	public Dictionary<string,object> info;
}

public struct BroadCastParam
{
	public string key;
	public object value;

	public BroadCastParam(string key,object value)
	{
		this.key = key;
		this.value = value;
	}
}

public class BroadcastSystem
{
	private static BroadcastSystem _defaultBoardcast;

	public static BroadcastSystem defaultBoardcast {
		get {
			if(_defaultBoardcast == null)
				_defaultBoardcast = new BroadcastSystem();
			return _defaultBoardcast;
		}
	}

	public delegate void Callback(BroadcastInfo arg1);

	private Dictionary<string, Callback> eventTable = new Dictionary<string, Callback>();

	public void AddListener(string eventType, Callback handler)
	{
		Callback d;

		if(eventTable.TryGetValue(eventType,out d))
		{
			d += handler;
			eventTable[eventType] = d;
		}
		else
		{
			eventTable[eventType] = handler;
		}
	}

	public void RemoveListener(string eventType, Callback handler)
	{
		Callback d;

		if(eventTable.TryGetValue(eventType,out d))
		{
			d -= handler;
			eventTable[eventType] = d;
		}

		if(d == null)
			eventTable.Remove(eventType);
	}

	public void SendMessage(string eventType, object from,params BroadCastParam[] args)
	{
		Callback d;
		if (eventTable.TryGetValue(eventType, out d))
		{
			if (d != null)
			{
				Dictionary<string,object> dic = new Dictionary<string, object>();
				if (args != null) {
					for(int i = 0;i < args.Length;i++)
					{
						dic[args[i].key] = args[i].value;
					}
				}

				BroadcastInfo info = default(BroadcastInfo);
				info.from = from;
				info.info = dic;
				d(info);
			}
		}
	}


	public void Cleanup()
	{
		eventTable.Clear();
	}

	public void PrintEventTable()
	{
		Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

		foreach (KeyValuePair<string, Callback> pair in eventTable)
		{
			Debug.Log("\t\t\t" + pair.Key + "\t\t" + pair.Value);
		}

		Debug.Log("\n");
	}
}
