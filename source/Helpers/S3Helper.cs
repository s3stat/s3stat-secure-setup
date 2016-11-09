using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.CloudSearchDomain.Model;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using S3stat.SecureSetup.Helpers.LightObjects;

namespace S3stat.SecureSetup.Helpers
{
	public class S3Helper
	{
		public enum LogOrSource
		{
			Log,
			Source
		}

		private static List<string> _regionList;

		public static List<string> GetRegionList()
		{
			if (_regionList == null)
			{
				_regionList = new List<string>();

				// Start with the ones our client library knows about.
				foreach (var region in RegionEndpoint.EnumerableAllRegions)
				{
					if (!_regionList.Contains(region.SystemName))
					{
						_regionList.Add(region.SystemName);
					}
				}

				try
				{
					// Try to grab any new ones from AWS
					var client = new AmazonEC2Client(AppState.AWSAccessKey, AppState.AWSSecretKey, RegionEndpoint.USEast1);
					var list = client.DescribeRegions(new DescribeRegionsRequest { });

					foreach (var region in list.Regions)
					{
						if (!_regionList.Contains(region.RegionName))
						{
							_regionList.Add(region.RegionName);
						}
					}
				}
				catch (Exception e)
				{
					AppState.NoteException(e, "GetRegionList", true);
				}
			}

			return _regionList;
		}
		public static void UpdateEndpointRegions(CombinedEndpoint endpoint, LogOrSource which)
		{
			if (which == LogOrSource.Log && String.IsNullOrEmpty(endpoint.LogRegion))
			{
				endpoint.LogRegion = GetRegionForBucketUsingCredentials(AppState.AWSAccessKey, AppState.AWSSecretKey, endpoint.LogBucketName).SystemName;
			}
			if (which == LogOrSource.Source && endpoint.IsS3 && String.IsNullOrEmpty(endpoint.SourceRegion))
			{
				endpoint.SourceRegion = GetRegionForBucketUsingCredentials(AppState.AWSAccessKey, AppState.AWSSecretKey, endpoint.BucketName).SystemName;
			}

			if (which == LogOrSource.Log)
			{
				if (endpoint.IsS3 && endpoint.Endpoint != null)
				{
					((CBucket) (endpoint.Endpoint)).LogRegion = endpoint.LogRegion;
				}

				if (endpoint.IsCloudfront && endpoint.Endpoint != null)
				{
					((CDistribution) (endpoint.Endpoint)).LogRegion = endpoint.LogRegion;
				}
			}
		}

		public static AmazonS3Client GetS3ClientForEndpoint(CombinedEndpoint endpoint, LogOrSource which)
		{
			UpdateEndpointRegions(endpoint, which);

			var regionName = which == LogOrSource.Log ? endpoint.LogRegion : endpoint.SourceRegion;

			return new AmazonS3Client(AppState.AWSAccessKey, AppState.AWSSecretKey, new AmazonS3Config() { RegionEndpoint = RegionEndpoint.GetBySystemName(regionName) });
		}

		public static RegionEndpoint GetRegionFromLocation(string location)
		{
			// Amazon helpfully returns all manner of garbage in addition to valid region codes.
			// Let's special case them...
			if (!String.IsNullOrEmpty(location))
			{
				if (location == "EU")
				{
					return RegionEndpoint.EUWest1;
				}

				return RegionEndpoint.GetBySystemName(location);
			}

			return RegionEndpoint.USEast1;
		}

		public static RegionEndpoint GetRegionFromExceptionMessage(string message)
		{
			// Amazon offers a convenient method to ask a bucket which region it belongs to.
			// But if you ask it from a region other than the one it belongs to, it will throw.
			// Happily, the exception tells you the region you should have asked from which, it turns out,
			// is the region the bucket lives in.  So even though Amazon hates us, we can win.

			var splitter = new Regex(@"The authorization header is malformed; the region '(.*?)' is wrong; expecting '(.*?)'", RegexOptions.Compiled);
			if (splitter.IsMatch(message))
			{
				var newRegion = splitter.Split(message)[2];
				return GetRegionFromLocation(newRegion);
			}

			return null;
		}

		/// <summary>
		/// Returns a valid RegionEndpoint for the specified bucket.
		/// </summary>
		/// <param name="publicKey"></param>
		/// <param name="privateKey"></param>
		/// <param name="bucketName"></param>
		/// <returns></returns>
		public static RegionEndpoint GetRegionForBucketUsingCredentials(string publicKey, string privateKey, string bucketName)
		{
			try
			{
				var client = new AmazonS3Client(publicKey, privateKey, RegionEndpoint.USEast1);
				var region = client.GetBucketLocation(new GetBucketLocationRequest()
				{
					BucketName = bucketName
				});

				if (region.Location != null)
				{
					return GetRegionFromLocation(region.Location.Value);
				}

				return RegionEndpoint.USEast1;
			}
			catch (Exception e)
			{
				var exceptionRegion = GetRegionFromExceptionMessage(e.Message);
				if (exceptionRegion != null)
				{
					return exceptionRegion;
				}
				return GetRegionForBucketUsingBruteForce(publicKey, privateKey, bucketName);
			}

		}		
		
		/// <summary>
		/// Amazon, being awesome, provides no sane way of asking a bucket which region it is in.
		/// It does, however, helpfully crash all over you if attempt to access a bucket
		/// without specifying its correct region.
		/// 
		/// Here, we're applying the only possible technique to determine an endpoint's region.
		/// Namely, getting AWS to crash all over the place until it finds a region that
		/// will work for the endpoint.  Fun stuff.
		/// </summary>
		/// <param name="publicKey"></param>
		/// <param name="privateKey"></param>
		/// <param name="bucketName"></param>
		/// <returns></returns>
		public static RegionEndpoint GetRegionForBucketUsingBruteForce(string publicKey, string privateKey, string bucketName)
		{
			var bestRegion = RegionEndpoint.USEast1;
			//foreach (var region in RegionEndpoint.EnumerableAllRegions)
			foreach (var region in GetRegionList())
			{
				var regionEndpoint = RegionEndpoint.GetBySystemName(region);
				try
				{
					var client = new AmazonS3Client(publicKey, privateKey, regionEndpoint);
					client.ListObjects(new ListObjectsRequest()
					{
						BucketName = bucketName,
						MaxKeys = 1
					});
					return regionEndpoint;
				}
				catch (Exception e)
				{
					if (e.Message.StartsWith("Access Denied"))
					{
						bestRegion = regionEndpoint;
					}
				}
			}

			return bestRegion;
		}
	}
}
