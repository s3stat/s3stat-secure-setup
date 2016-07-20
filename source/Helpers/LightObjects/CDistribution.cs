using System;
using System.Data.SqlTypes;
using System.Xml.Serialization;
using S3stat.SecureSetup.Helpers.Interfaces;

namespace S3stat.SecureSetup.Helpers.LightObjects
{
	public class CDistribution : IDistribution
	{
		/// <summary>
		/// This is a hack to work around Mono's lack of support for SqlDateTime objects.
		/// Since SqlDateTime doesn't recognize DateTime.MinValue, we need to define
		/// our own magic date to represent nulls across both types.
		/// </summary>
		public static DateTime NullDateValue = new DateTime(1970, 1, 1);

		#region properties

		public int DistributionID { get; set; }

		public int S3AccountID { get; set; }

		public string AWSDistributionID { get; set; }

		[XmlIgnore]
		public SqlInt32 BucketID { get; set; }

		public string BucketName { get; set; }

		public string LogBucketName { get; set; }

		public string LogPath { get; set; }

		public string StatBucketName { get; set; }

		public string StatPath { get; set; }

		public int StatusID { get; set; }

		[XmlIgnore]
		public SqlDateTime LastProcessDate { get; set; }

		[XmlIgnore]
		public SqlDateTime OldestLogfileDate { get; set; }

		[XmlIgnore]
		public SqlDateTime OldestProcessedDate { get; set; }

		public bool IsActive { get; set; }

		public bool IsLogging { get; set; }

		public bool IsEU { get; set; }

		public bool IsEC2 { get; set; }

		public bool IsStreaming { get; set; }

		public bool UploadReports
		{
			get { return true; }
		}

		public bool UploadProcessedLogs
		{
			get { return true; }
		}

		public string LogOutPath
		{
			get { return LogPath; }
		}
		public string LogRegion { get; set; }



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

		public bool IsSelfManaged { get; set; }

		public bool UseStoneStepsWebalizer { get; set; }

		public bool IsPrivateStats { get; set; }

		public bool IsCompactStats { get; set; }


		[XmlIgnore]
		public IS3Account S3Account { get; set; }

		#endregion


		public CDistribution()
		{
		}
		public CDistribution(IDistribution dist)
		{
			AWSDistributionID = dist.AWSDistributionID;
			BucketID = dist.BucketID;
			BucketName = dist.BucketName;
			DistributionID = dist.DistributionID;
			IsActive = dist.IsActive;
			IsEC2 = dist.IsEC2;
			IsEU = dist.IsEU;
			IsLogging = dist.IsLogging;
			IsStreaming = dist.IsStreaming;
			LastProcessDate = dist.LastProcessDate;
			LogBucketName = dist.LogBucketName;
			LogPath = dist.LogPath;
			S3AccountID = dist.S3AccountID;
			StatBucketName = dist.StatBucketName;
			StatPath = dist.StatPath;
			StatusID = dist.StatusID;
			UseStoneStepsWebalizer = dist.UseStoneStepsWebalizer;
			IsPrivateStats = dist.IsPrivateStats;
			IsCompactStats = dist.IsCompactStats;
		}


		public void SetDefaults()
		{
			LogPath = @"cflog/";
			StatPath = @"cfstats/";

			IsActive = true;
		}

		private string WinPath(string unixPath)
		{
			return unixPath.Replace(@"/", @"\");
		}

		public virtual string GetLocalLogPath()
		{
			return String.Format(@"\{0}_{1}\{2}", BucketName, AWSDistributionID, WinPath(LogPath));
		}

		public virtual string GetLocalStatPath()
		{
			return String.Format(@"\{0}_{1}\{2}", BucketName, AWSDistributionID, WinPath(StatPath));
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
			return String.Format(@"{0}.*.log"
				, AWSDistributionID);
		}

		public virtual string GetLogFileFilter(DateTime dateToProcess)
		{
			return String.Format(@"{0}.{1}*.log"
				, AWSDistributionID
				, dateToProcess.ToString("yyyy-MM-dd"));
		}

		public virtual string GetLogFilePrefix()
		{
			return String.Format(@"{0}{1}."
				, LogPath
				, AWSDistributionID);
		}

		public virtual string GetLogFilePrefix(DateTime dateToProcess)
		{
			return String.Format(@"{0}{1}.{2}"
				, LogPath
				, AWSDistributionID
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

		public virtual string GetHostedStatPath()
		{
			var folder = IsStreaming ? "streamstats" : "cfstats";
			return String.Format(@"{0}/{1}_{2}/{3}/", S3Account.UserID, BucketName, AWSDistributionID, folder);
		}
		public virtual string GetHostedExportPath()
		{
			return String.Format(@"{0}/{1}_{2}/export/", S3Account.UserID, BucketName, AWSDistributionID);
		}

	}
}
