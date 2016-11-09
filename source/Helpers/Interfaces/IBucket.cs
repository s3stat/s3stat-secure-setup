using System;
using System.Data.SqlTypes;

namespace S3stat.SecureSetup.Helpers.Interfaces
{
	public interface IBucket : ILoggable
	{
		int BucketID { get; set; }
		string DisplayName { get; set; }
		string LogPrefix { get; set; }
		string LogFormatPattern { get; set; }
		bool FtpProcessedLogs { get; set; }
		bool FtpReports { get; set; }
		string FtpSite { get; set; }
		string FtpStatPath { get; set; }
		string FtpLogPath { get; set; }
		string FtpUsername { get; set; }
		string FtpPassword { get; set; }
		bool IsActive { get; set; }
		bool IsLogging { get; set; }
		bool IsReadable { get; set; }

		void SetDefaults();
	}
}
