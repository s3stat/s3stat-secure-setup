using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using S3stat.SecureSetup.Helpers.Interfaces;

namespace S3stat.SecureSetup.Helpers.Interfaces
{
	public interface ILoggable
	{
		int S3AccountID { get; set; }
		string BucketName { get; set; }
		string LogBucketName { get; set; }
		string StatBucketName { get; set; }
		string LogPath { get; set; }
		string StatPath { get; set; }
		string LogOutPath { get; }
		bool UploadProcessedLogs { get;}
		bool UploadReports { get; }
		bool IsSelfManaged { get; set; }
		bool UseStoneStepsWebalizer { get; set; }
		bool IsPrivateStats { get; set; }
		bool IsCompactStats { get; set; }
		bool IsReadable { get; set; }
		int StatusID { get; set; }
		SqlDateTime LastProcessDate { get; set; }

		IS3Account S3Account { get; set; }


		string GetLocalLogPath();
		string GetLocalStatPath();
		string GetStatUrl();
		string GetHostedStatPath();
		string GetHostedExportPath();
		string GetLogFilePath(DateTime dateToProcess);
		string GetLogFileName(DateTime dateToProcess);
		string GetLogFileFilter();
		string GetLogFileFilter(DateTime dateToProcess);
		string GetLogFilePrefix();
		string GetLogFilePrefix(DateTime dateToProcess);
	}
}
