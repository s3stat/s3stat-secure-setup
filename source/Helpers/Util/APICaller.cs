using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace S3stat.SecureSetup.Helpers.Util
{
	public class APICaller
	{
		public enum StatusCodes
		{
			Success,
			Fail
		}

		public string Endpoint { get; set; }
		protected Dictionary<string, string> _params = new Dictionary<string, string>();

		private string _html;
		private Exception _lastException;
		private int _lastErrorCode;
		private StatusCodes _status;
		public const int CLIENT_VERSION = 12;

		public string Html
		{
			get { return _html; }
		}

		public Exception LastException
		{
			get { return _lastException; }
		}

		public int LastErrorCode
		{
			get { return _lastErrorCode; }
		}

		public int IntValue
		{
			get 
			{
				int result;
				if (Int32.TryParse(_html, out result))
				{
					return result;
				}
				return -1; 
			}
		}

		public StatusCodes Status
		{
			get { return _status; }
		}

		public APICaller()
		{
			Add("clientversion", CLIENT_VERSION);
		}

		public APICaller(string endpoint) : this()
		{
			Endpoint = endpoint;
		}

		public static implicit operator string(APICaller x)
		{
			return x.ToString();
		}

		#region parameters
		public void Add(string key, string value)
		{
			_params[key] = value;
		}

		public void Add(string key, int value)
		{
			_params[key] = value.ToString();
		}

		public void Add(string query)
		{
			string[] names = query.Split('=');
			if (names.Length > 1)
			{
				Add(names[0], names[1]);
			}
		}

		public void Clear()
		{
			_params.Clear();
		}
		#endregion

		public bool Call()
		{
			try
			{
				HttpWebRequest webRequest = PreparePostRequest(Endpoint, BuildQueryString());

				if (webRequest != null)
				{
					var responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
					_html = responseReader.ReadToEnd();
					responseReader.Close();
				}
				else
				{
					_lastException = new Exception("API connection error");
					return false;
				}

				if (_html.StartsWith("-1"))
				{
					_lastException = new Exception("API call failed: " + _html);
					return false;
				}

				_lastErrorCode = 200;
				_status = StatusCodes.Success;


			}
			catch (WebException e)
			{
				ParseErrorCode(e.Message);
				_lastException = e;
				_status = StatusCodes.Fail;
				return false;
			}
			catch (Exception e)
			{
				_lastException = e;
				_status = StatusCodes.Fail;
				return false;
			}

			_lastException = null;
			return true;
		}

		private HttpWebRequest PreparePostRequest(string url, string fields)
		{
			// set up header and POST request
			var webRequest = WebRequest.Create(url) as HttpWebRequest;
			if (webRequest != null)
			{
				webRequest.Method = "POST";
				webRequest.ContentType = "application/x-www-form-urlencoded";
				webRequest.CookieContainer = new CookieContainer();
				webRequest.UserAgent = @"S3stat .NET Client Library v1.0; (+https://www.s3stat.com/)";

				var requestWriter = new StreamWriter(webRequest.GetRequestStream());
				requestWriter.Write(fields);
				requestWriter.Close();
			}
			return webRequest;
		}

		public string BuildQueryString()
		{
			string qs = "";
			if (_params.Count > 0)
			{
				string separator = "";
				foreach (string key in _params.Keys)
				{
					string value = (_params[key] == null) ? "" : System.Web.HttpUtility.UrlEncode(_params[key]);

					if (key != "")
					{
						qs += separator
							  + key
							  + "="
							  + value;
					}
					else
					{
						qs += separator
							  + value;
					}
					separator = "&";
				}
			}
			return qs;
		}

		private void ParseErrorCode(string message)
		{
			_lastErrorCode = 0;

			//The remote server returned an error: (404)
			string[] tokens = message.Split("()".ToCharArray());
			if (tokens.Length > 1)
			{
				int.TryParse(tokens[1], out _lastErrorCode);
			}
		}
	}
}
