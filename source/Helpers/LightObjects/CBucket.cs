using System;
using System.Data.SqlTypes;
using System.Xml.Serialization;
using S3stat.SecureSetup.Helpers.Interfaces;

namespace S3stat.SecureSetup.Helpers.LightObjects
{
	public class CBucket : IBucket
	{
		/// <summary>
		/// This is a hack to work around Mono's lack of support for SqlDateTime objects.
		/// Since SqlDateTime doesn't recognize DateTime.MinValue, we need to define
		/// our own magic date to represent nulls across both types.
		/// </summary>
		public static DateTime NullDateValue = new DateTime(1970, 1, 1);

		#region properties

		public int BucketID { get; set; }

		public int S3AccountID { get; set; }

		public string BucketName { get; set; }

		public string DisplayName { get; set; }

		public string LogPath { get; set; }

		public string LogPrefix { get; set; }

		public string LogFormatPattern { get; set; }

		public string StatPath { get; set; }

		public string LogOutPath { get; set; }

		public bool UploadProcessedLogs { get; set; }

		public bool UploadReports { get; set; }

		public bool FtpProcessedLogs { get; set; }

		public bool FtpReports { get; set; }

		public string FtpSite { get; set; }

		public string FtpStatPath { get; set; }

		public string FtpLogPath { get; set; }

		public string FtpUsername { get; set; }

		public string FtpPassword { get; set; }

		public int StatusID { get; set; }

		public string LogBucketName { get; set; }

		public string StatBucketName { get; set; }

		[XmlIgnore]
		public SqlDateTime LastProcessDate { get; set; }

		[XmlIgnore]
		public SqlDateTime OldestLogfileDate { get; set; }

		[XmlIgnore]
		public SqlDateTime OldestProcessedDate { get; set; }


		public DateTime LastProcessed
		{
			get 
			{
				if (LastProcessDate.IsNull)
				{
					return NullDateValue;
				}

				return LastProcessDate.Value; 
			}
			set { LastProcessDate = value; }
		}

		public bool IsActive { get; set; }

		public bool IsLogging { get; set; }

		public bool IsSelfManaged { get; set; }

		public bool UseStoneStepsWebalizer { get; set; }

		public bool IsPrivateStats { get; set; }

		public bool IsCompactStats { get; set; }
		public bool IsReadable { get; set; }
		public string LogRegion { get; set; }

		[XmlIgnore]
		public IS3Account S3Account { get; set; }

		#endregion


		public CBucket()
		{
		}

		public CBucket(IBucket bucket)
		{
			BucketID = bucket.BucketID;
			S3AccountID = bucket.S3AccountID;
			IsLogging = bucket.IsLogging;
			IsActive = bucket.IsActive;
			LastProcessDate = bucket.LastProcessDate;
			StatusID = bucket.StatusID;
			FtpPassword = bucket.FtpPassword;
			FtpUsername = bucket.FtpUsername;
			FtpLogPath = bucket.FtpLogPath;
			FtpStatPath = bucket.FtpStatPath;
			FtpSite = bucket.FtpSite;
			FtpReports = bucket.FtpReports;
			FtpProcessedLogs = bucket.FtpProcessedLogs;
			UploadReports = bucket.UploadReports;
			UploadProcessedLogs = bucket.UploadProcessedLogs;
			LogOutPath = bucket.LogOutPath;
			StatPath = bucket.StatPath;
			LogFormatPattern = bucket.LogFormatPattern;
			LogPrefix = bucket.LogPrefix;
			LogPath = bucket.LogPath;
			DisplayName = bucket.DisplayName;
			BucketName = bucket.BucketName;
			LogBucketName = bucket.LogBucketName;
			StatBucketName = bucket.StatBucketName;
			UseStoneStepsWebalizer = bucket.UseStoneStepsWebalizer;
			IsPrivateStats = bucket.IsPrivateStats;
			IsCompactStats = bucket.IsCompactStats;
			IsReadable = bucket.IsReadable;
		}


		public void SetDefaults()
		{
			LogPath = @"log/";
			LogPrefix = @"access_log-";
			LogFormatPattern = @"exyyMMdd.log";
			StatPath = @"stats/";
			LogOutPath = @"log/";

			IsActive = true;
			IsReadable = true;
			UploadProcessedLogs = false;
			UploadReports = true;
			FtpProcessedLogs = false;
			FtpReports = false;
		}

		private string WinPath(string unixPath)
		{
			return unixPath.Replace(@"/", @"\");
		}

		public virtual string GetLocalLogPath()
		{
			if (BucketID != 0)
			{
				return String.Format(@"{0}\{1}", BucketID, WinPath(LogPath));
			}
			
			// for local runs, just use bucketname.
			return String.Format(@"{0}\{1}", BucketName, WinPath(LogPath));
		}

		public virtual string GetLocalStatPath()
		{
			if (BucketID != 0)
			{
				return String.Format(@"{0}\{1}", BucketID, WinPath(StatPath));
			}
			
			// for local runs, just use bucketname.
			return String.Format(@"{0}\{1}", BucketName, WinPath(StatPath));
		}

		public virtual string GetLogFileName(DateTime dateToProcess)
		{
			return String.Format(@"ex{0}.log"
				, dateToProcess.ToString("yyMMdd"));
		}

		public virtual string GetLogFilePath(DateTime dateToProcess)
		{
			return String.Format(@"{0}{1}"
				, GetLocalLogPath()
				, GetLogFileName(dateToProcess));
		}

		public virtual string GetLogFileFilter()
		{
			return String.Format(@"{0}*"
				, LogPrefix);
		}

		public virtual string GetLogFileFilter(DateTime dateToProcess)
		{
			return String.Format(@"{0}{1}*"
				, LogPrefix
				, dateToProcess.ToString("yyyy-MM-dd"));
		}

		public virtual string GetLogFilePrefix()
		{
			return String.Format(@"{0}{1}"
				, LogPath
				, LogPrefix);
		}

		public virtual string GetLogFilePrefix(DateTime dateToProcess)
		{
			return String.Format(@"{0}{1}{2}"
				, LogPath
				, LogPrefix
				, dateToProcess.ToString("yyyy-MM-dd-"));
		}

		public string GetStatUrl()
		{
			// compact stats are always hosted:
			if (IsCompactStats)
			{
				return String.Format("https://s3.amazonaws.com/reports.s3stat.com/{0}", GetHostedStatPath());
				//return String.Format("https://d14nptsr52j2r9.cloudfront.net/{0}", GetHostedStatPath());
			}
			if (StatBucketName.ToLower() == StatBucketName)
			{
				return String.Format(@"http://{0}.s3.amazonaws.com/{1}", StatBucketName, StatPath);
			}

			// legacy buckets with caps need the oldskool path:
			return String.Format(@"http://s3.amazonaws.com/{0}/{1}", StatBucketName, StatPath);
		}

		public string GetHostedStatPath()
		{
			return String.Format(@"{0}/{1}/stats/", S3Account.UserID, BucketName);
		}
		public virtual string GetHostedExportPath()
		{
			return String.Format(@"{0}/{1}/export/", S3Account.UserID, BucketName);
		}
	}
}
