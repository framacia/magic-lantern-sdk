using UnityEngine;
using System.IO;
using System;

public class Logger
{
	public static string LastValidPath;
	StreamWriter file;

	string msg;
	public bool logToFile = true;
	bool logClosed;
	string fname = "/data_log/";

	public Logger()
	{
		if (!Directory.Exists(Application.persistentDataPath + fname))
		{
			Directory.CreateDirectory(Application.persistentDataPath + fname);
		}

		LastValidPath = Application.persistentDataPath + fname + "_" + DateTime.Now.ToString("yyyMMddHHmm") + " - log.txt";
		file = new System.IO.StreamWriter(LastValidPath, true);
		file.WriteLine("#Starting log sesion");
	}

	public void LogMessage(string message)
	{
		if (file != null)
		{
			string logMsg = message;
			file.WriteLine(logMsg);
		}
	}

	public void Close()
	{
		if (file != null)
		{
			file.WriteLine("#Ending log sesion");
			file.Close();
			file = null;
		}
	}

	public void OnDestroy()
	{
		Close();
	}
}
