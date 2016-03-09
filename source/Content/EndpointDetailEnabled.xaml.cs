using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using S3stat.SecureSetup.Helpers;
using S3stat.SecureSetup.Helpers.LightObjects;
using S3stat.SecureSetup.Pages;

namespace S3stat.SecureSetup.Content
{
	/// <summary>
	/// Interaction logic for EndpointDetailEnabled.xaml
	/// </summary>
	public partial class EndpointDetailEnabled : UserControl
	{
		public CombinedEndpoint Endpoint
		{
			get;
			set;
		}

		public EndpointDetailEnabled()
		{
			InitializeComponent();
		}
		private EndpointSetup FindContainingPage()
		{
			var parent = Parent;
			while (parent != null && parent is FrameworkElement)
			{
				var page = parent as EndpointSetup;
				if (page != null)
				{
					return page;
				}
				parent = ((FrameworkElement)parent).Parent;
			}


			return null;
		}

		private void StopReporting()
		{
			var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
			try
			{
				if (s3stat.DeleteEndpoint(Endpoint.Endpoint))
				{
					Endpoint.IsS3stat = false;
				}
				else
				{
					ModernDialog.ShowMessage("Credential Error.", "If this problem persists, please contact S3stat support.", MessageBoxButton.OK);
				}
			}
			catch
			{
				ModernDialog.ShowMessage("Couldn't delete.", "If this problem persists, please contact S3stat support.", MessageBoxButton.OK);
			}

		}

		private void StopLogging(EndpointSetup setupPage)
		{
			try
			{
				if (Endpoint.IsS3)
				{
					var s3 = S3Helper.GetS3ClientForEndpoint(Endpoint, S3Helper.LogOrSource.Source);
					LogEnabler.StopLoggingS3(s3, Endpoint);
				}
				else
				{
					var cf = setupPage.CFClient;
					if (Endpoint.IsStreaming)
					{
						LogEnabler.StopLoggingStreaming(cf, Endpoint);
					}
					else
					{
						LogEnabler.StopLoggingCloudfront(cf, Endpoint);
					}
				}
				Endpoint.IsLogging = false;
			}
			catch (Exception e)
			{
				var s3stat = new S3statHelper(AppState.UserName, AppState.Password);
				s3stat.NoteException(e, "StopLogging", true);

				ModernDialog.ShowMessage("Couldn't stop logging.", "Insufficient permission.  You can try again from your AWS Console.", MessageBoxButton.OK);

			}
		}

		private void btnStop_Click(object sender, RoutedEventArgs e)
		{
			var d = new ModernDialog
			{
				Title = "Confirm",
				Content = "This will stop S3stat reporting for this endpoint"
			};
			d.Buttons = new[] { d.OkButton, d.CancelButton };
			d.OkButton.Click += OkButton_Click;

			if (cbLogging.IsChecked.HasValue && cbLogging.IsChecked.Value)
			{
				d.Content += ", " + Environment.NewLine + "and will also stop logfiles being delivered for it";
			}
			d.Content += "." + Environment.NewLine + Environment.NewLine + "Continue?";

			d.ShowDialog();

		}

		void OkButton_Click(object sender, RoutedEventArgs e)
		{
			var setupPage = FindContainingPage();

			StopReporting();
			if (cbLogging.IsChecked.HasValue && cbLogging.IsChecked.Value)
			{
				StopLogging(setupPage);
			} 

			setupPage.UpdateEndpointEntry(Endpoint);
			setupPage.ShowPlaceholder();
		}
	}
}
