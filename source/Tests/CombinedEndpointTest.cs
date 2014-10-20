using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using S3stat.SecureSetup.Helpers.LightObjects;
using S3stat.SecureSetup.Pages;

namespace S3stat.SecureSetup.Tests
{
	[TestFixture]
	class CombinedEndpointTest
	{
		[Test]
		public void LoggingPathTest()
		{
			var endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "logs/access-log-");
			Assert.AreEqual(endpoint.LogPath, "logs/");
			Assert.AreEqual(endpoint.LogPrefix, "access-log-");

			endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "logs/");
			Assert.AreEqual(endpoint.LogPath, "logs/");
			Assert.AreEqual(endpoint.LogPrefix, "");

			endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "just-a-prefix-");
			Assert.AreEqual(endpoint.LogPath, "");
			Assert.AreEqual(endpoint.LogPrefix, "just-a-prefix-");

			endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "long/complex/path/with-a-prefix-");
			Assert.AreEqual(endpoint.LogPath, "long/complex/path/");
			Assert.AreEqual(endpoint.LogPrefix, "with-a-prefix-");

			endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "/funky");
			Assert.AreEqual(endpoint.LogPath, "/");
			Assert.AreEqual(endpoint.LogPrefix, "funky");

			endpoint = new CombinedEndpoint();
			EndpointSetup.ApplyBucketLoggingPaths(endpoint, "");
			Assert.AreEqual(endpoint.LogPath, "");
			Assert.AreEqual(endpoint.LogPrefix, "");

		}

	}
}
