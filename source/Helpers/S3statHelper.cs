using System;
using S3stat.SecureSetup.Helpers.Interfaces;
using S3stat.SecureSetup.Helpers.LightObjects;
using S3stat.SecureSetup.Helpers.Util;

namespace S3stat.SecureSetup.Helpers
{
	public class S3statHelper
	{

		public string S3statUsername = "";
		public string S3statPassword = "";

		/// <summary>
		/// A list of valid response formats, for calls that return data
		/// </summary>
		public enum ResponseFormat
		{
			CSV,
			XML
		}

		public S3statHelper(string s3statUsername, string s3statPassword)
		{
			S3statUsername = s3statUsername;
			S3statPassword = s3statPassword;
		}

		/// <summary>
		/// For future expansion:
		/// </summary>
		/// <returns></returns>
		public string GetS3statHost()
		{
#if DEBUG
			return "http://dev.s3stat";
#endif
			return "https://www.s3stat.com";
		}

		/// <summary>
		/// Ensure we can log in to S3stat using the supplied user/pass
		/// </summary>
		/// <returns></returns>
		/// 
		public bool CheckUser()
		{
			string apiEndpoint = String.Format(@"{0}/API/GetUser.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);

			if (caller.Call())
			{
				return caller.IntValue != -1;
			}

			return false;
		}

		/// <summary>
		/// List S3stat endpoints for this AWS account
		/// </summary>
		/// <returns></returns>
		public CAccount ListEndpoints(string awsAccountID)
		{
			string apiEndpoint = String.Format(@"{0}/API/ListEndpoints.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);
			caller.Add("awsaccountid", awsAccountID);

			if (caller.Call())
			{
				return (CAccount) SerializationHelper.DeSerialize(caller.Html, typeof (CAccount));
			}

			throw caller.LastException;
		}

		/// <summary>
		/// Push endpoint configuration info to S3stat
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public int SetEndpoint(ILoggable endpoint)
		{
			string apiEndpoint = String.Format(@"{0}/API/SetEndpoint.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);
			caller.Add("endpoint", SerializationHelper.Serialize(endpoint));

			if (caller.Call())
			{
				return caller.IntValue;
			}

			throw caller.LastException;

		}

		/// <summary>
		/// Ensure that S3stat has a record for this AWS Account, return said record.
		/// </summary>
		/// <param name="awsAccountID"></param>
		/// <returns></returns>
		public CAccount GetOrCreateS3StatAccount(string awsAccountID)
		{
			string apiEndpoint = String.Format(@"{0}/API/GetS3Account.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);
			caller.Add("awsaccountid", awsAccountID);
			caller.Add("createIfMissing", 1);

			if (caller.Call())
			{
				return (CAccount)SerializationHelper.DeSerialize(caller.Html, typeof(CAccount));
			}

			throw caller.LastException;

		}

		/// <summary>
		/// Push AWS Account info up to S3stat.
		/// </summary>
		/// <param name="s3statAccount"></param>
		/// <returns></returns>
		public int SetS3statAccount(CAccount s3statAccount)
		{
			string apiEndpoint = String.Format(@"{0}/API/SetS3Account.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);
			caller.Add("account", SerializationHelper.Serialize(s3statAccount));

			if (caller.Call())
			{
				return caller.IntValue;
			}

			throw caller.LastException;

		}

		public bool NoteException(Exception e, string context, bool handled)
		{
			string apiEndpoint = String.Format(@"{0}/API/NoteException.aspx", GetS3statHost());

			var caller = new APICaller(apiEndpoint);
			caller.Add("username", S3statUsername);
			caller.Add("password", S3statPassword);
			caller.Add("message", e.ToString());
			caller.Add("context", context);
			caller.Add("handled", handled ? "1" : "0");

			return caller.Call();
		}
	}
}
