using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace S3stat.SecureSetup.Helpers
{
	/// <summary>
	/// Summary description for SerializationHelper.
	/// </summary>
	public class SerializationHelper
	{
		/// <summary>
		/// Given a serializable object, returns an XML string representing that object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string Serialize(object obj)
		{
			var xs = new XmlSerializer(obj.GetType());
			var buffer = new MemoryStream();
			xs.Serialize(buffer, obj);
			return ASCIIEncoding.ASCII.GetString(buffer.ToArray());
		}

		/// <summary>
		/// Given a serializable object, returns an XML string representing that object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string SerializeBinary(object obj)
		{
			try
			{
				var formatter = new BinaryFormatter();
				var buffer = new MemoryStream();
				formatter.Serialize(buffer, obj);

				return ASCIIEncoding.ASCII.GetString(buffer.ToArray());
			}
			catch (Exception ex)
			{
				AppState.NoteException(ex, "SerializeBinary", false);
				throw new Exception("could not serialize this object:\n" + obj + "\n\n" + ex);
			}
		}

		/// <summary>
		/// Given a serializable object, creates an XML file.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="fileName"></param>
		public static void SerializeToFile(object obj, string fileName)
		{
			try
			{
				var xs = new XmlSerializer(obj.GetType());
				using (TextWriter writer = new StreamWriter(fileName, false))
				{
					xs.Serialize(writer, obj);
					writer.Close();
				}
			}
			catch (Exception ex)
			{
				AppState.NoteException(ex, "SerializeToFile", false);
				throw new Exception("could not serialize this object:\n" + obj + "\n\n" + ex);
			}
		}

		/// <summary>
		/// Given an XML string representing an object, returns that object.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object DeSerialize(string xml, Type type)
		{
			// empty strings represent null objects
			if (xml == "")
				return null;

			var xs = new XmlSerializer(type);
			var buffer = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml));
			try
			{
				return xs.Deserialize(buffer);
			}
			catch (Exception ex)
			{
				AppState.NoteException(ex, "DeSerialize", false);
				throw new Exception("could not deserialize this string:\n" + xml);
			}
		}

		/// <summary>
		/// Given an XML string representing an object, returns that object.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static object DeSerializeBinary(string data)
		{
			// empty strings represent null objects
			if (data == "")
				return null;

			var formatter = new BinaryFormatter();
			var buffer = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(data));
			try
			{
				return formatter.Deserialize(buffer);
			}
			catch (Exception ex)
			{
				AppState.NoteException(ex, "DeSerializeBinary", false);
				throw new Exception("could not deserialize this string:\n" + data);
			}
		}

		/// <summary>
		/// Given the name of an XML file representing an object, returns that object.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object DeSerializeFromFile(string fileName, Type type)
		{
			try
			{
				var xs = new XmlSerializer(type);

				using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					object obj = xs.Deserialize(fs);
					fs.Close();
					return obj;
				}
			}
			catch (FileNotFoundException e1)
			{
				AppState.NoteException(e1, "DeSerializeFromFile", false);
				throw new FileNotFoundException("could not find file:" + fileName, fileName);
			}
			catch (Exception e)
			{
				AppState.NoteException(e, "DeSerializeFromFile", false);
				throw new Exception("could not deserialize this file:"
				                    + fileName
				                    + Environment.NewLine + e);
			}
		}
	}
}