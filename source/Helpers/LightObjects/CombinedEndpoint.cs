using Amazon.CloudFront.Model;
using Amazon.S3.Model;
using S3stat.SecureSetup.Helpers.Interfaces;

namespace S3stat.SecureSetup.Helpers.LightObjects
{
	public class CombinedEndpoint
	{
		public enum EndpointType
		{
			Cloudfront,
			S3,
			Streaming
		}

		public string Id { get; set; }
		public EndpointType Type { get; set; }
		public bool IsCloudfront { get; set; }
		public bool IsS3 { get; set; }
		public bool IsStreaming { get; set; }
		public bool IsS3stat { get; set; }
		public bool IsLogging { get; set; }
		public bool HasReports { get; set; }

		public bool IsLoggingKnown { get; set; }
		public bool IsS3statKnown { get; set; }

		public string BucketName { get; set; }
		public string Title { get; set; }
		public string Subtitle { get; set; }

		public ILoggable Endpoint { get; set; }
		public DistributionConfig DistributionConfig { get; set; }
		public StreamingDistributionConfig StreamingDistributionConfig { get; set; }
		public S3BucketLoggingConfig BucketLoggingConfig { get; set; }
		public string Policy { get; set; }
		public string ETag { get; set; }

		public string LogPath { get; set; }
		public string LogPrefix { get; set; }
		public string LogBucketName { get; set; }
	}
}