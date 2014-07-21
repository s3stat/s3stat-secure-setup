using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using S3stat.SecureSetup.Helpers.LightObjects;

namespace S3stat.SecureSetup.Helpers
{
	internal class AppState
	{
		public static string UserName { get; set; }
		public static string Password { get; set; }
		public static string AWSAccessKey { get; set; }
		public static string AWSSecretKey { get; set; }
		public static string AWSAccountID { get; set; }
		public static bool RememberS3statLogin { get; set; }
		public static bool RememberAWSCredentials { get; set; }
		public static CAccount Account { get; set; }

		private const string ConfigFileName = "s3stat.config";

		static AppState()
		{
			RememberS3statLogin = true;
			RememberAWSCredentials = true;
		}

		public static void Save()
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
				IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

			using (var writer = new StreamWriter(new IsolatedStorageFileStream(ConfigFileName, FileMode.Create, FileAccess.Write, isoStore)))
			{
				writer.WriteLine(SerializationHelper.Serialize(new SaveData()));
				writer.Close();
			}
		}


		public static void Load()
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
				IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
			if (isoStore.FileExists(ConfigFileName))
			{
				using (var reader = new StreamReader(new IsolatedStorageFileStream(ConfigFileName, FileMode.Open, isoStore)))
				{
					string xml = reader.ReadToEnd();
					var data = (SaveData) SerializationHelper.DeSerialize(xml, typeof (SaveData));
					RememberS3statLogin = data.RememberS3statLogin;
					RememberAWSCredentials = data.RememberAWSCredentials;
					UserName = data.UserName;
					Password = data.Password;
					AWSAccessKey = data.AWSAccessKey;
					AWSSecretKey = data.AWSSecretKey;
					AWSAccountID = data.AWSAccountID;
				}
			}
		}

		public static string Hash()
		{
			string input = UserName + Password + AWSAccessKey + AWSSecretKey;
			MD5 md5Hasher = MD5.Create();
			byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

			var sBuilder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			return sBuilder.ToString();
		}
	}


	public class SaveData
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string AWSAccessKey { get; set; }
		public string AWSSecretKey { get; set; }
		public string AWSAccountID { get; set; }
		public bool RememberS3statLogin { get; set; }
		public bool RememberAWSCredentials { get; set; }

		public SaveData()
		{
			RememberS3statLogin = AppState.RememberS3statLogin;
			RememberAWSCredentials = AppState.RememberAWSCredentials;
			if (RememberS3statLogin)
			{
				UserName = AppState.UserName;
				Password = AppState.Password;
			}
			if (RememberAWSCredentials)
			{
				AWSAccessKey = AppState.AWSAccessKey;
				AWSSecretKey = AppState.AWSSecretKey;
				AWSAccountID = AppState.AWSAccountID;
			}
		}
	}
}
