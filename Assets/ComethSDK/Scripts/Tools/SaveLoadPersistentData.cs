using System;
using System.IO;
using System.Text;
using ComethSDK.Scripts.Types;
using Newtonsoft.Json;
using UnityEngine;

namespace ComethSDK.Scripts.Tools
{
	public static class SaveLoadPersistentData
	{
		/// <summary>
		///     Save data to a file (overwrite completely)
		/// </summary>
		public static void Save(EncryptionData data, string folder, string file)
		{
			// get the data path of this save data
			var dataPath = GetFilePath(folder, file);

			var jsonData = JsonConvert.SerializeObject(data);
			var byteData = Encoding.ASCII.GetBytes(jsonData);

			// create the file in the path if it doesn't exist
			// if the file path or name does not exist, return the default SO
			if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
				Directory.CreateDirectory(Path.GetDirectoryName(dataPath));

			// attempt to save here data
			//can throw exception
			File.WriteAllBytes(dataPath, byteData);
			Debug.Log("Save data to: " + dataPath);
		}

		/// <summary>
		///     Load all data at a specified file and folder location
		/// </summary>
		public static EncryptionData Load(string folder, string file)
		{
			// get the data path of this save data
			var dataPath = GetFilePath(folder, file);

			// if the file Directory does not exist, return the default SO
			if (!Directory.Exists(Path.GetDirectoryName(dataPath)))
			{
				Debug.LogError("Directory does not exist! " + dataPath);
				throw new Exception("Directory does not exist! " + dataPath);
			}
			
			if(!File.Exists(dataPath))
			{
				Debug.LogError("File does not exist! " + dataPath);
				throw new Exception("File does not exist! " + dataPath);
			}

			// load in the save data as byte array
			//can throw exception
			var jsonDataAsBytes = File.ReadAllBytes(dataPath);
			Debug.Log("<color=green>Loaded all data from: </color>" + dataPath);

			// convert the byte array to json
			var jsonData = Encoding.ASCII.GetString(jsonDataAsBytes);

			// convert to the specified object type
			return JsonConvert.DeserializeObject<EncryptionData>(jsonData);
		}

		/// <summary>
		///     Create file path for where a file is stored on the specific platform given a folder name and file name
		/// </summary>
		private static string GetFilePath(string folderName, string fileName = "")
		{
			string filePath;
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			// mac
			filePath = Path.Combine(Application.persistentDataPath, ("data/" + folderName));

			if (fileName != "")
				filePath = Path.Combine(filePath, (fileName + ".txt"));
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			// windows
			filePath = Path.Combine(Application.persistentDataPath, "data/" + folderName);

			if (fileName != "")
				filePath = Path.Combine(filePath, fileName + ".txt");
#elif UNITY_ANDROID
        // android
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".txt"));
#elif UNITY_IOS
        // ios
        filePath = Path.Combine(Application.persistentDataPath, ("data/" + FolderName));

        if(FileName != "")
            filePath = Path.Combine(filePath, (FileName + ".txt"));
#endif
			return filePath;
		}
	}
}