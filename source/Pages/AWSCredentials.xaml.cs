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
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using S3stat.SecureSetup.Content;
using S3stat.SecureSetup.Helpers;
using S3stat.SecureSetup.Helpers.LightObjects;
using FragmentNavigationEventArgs = FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs;
using NavigatingCancelEventArgs = FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs;
using NavigationEventArgs = FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs;

namespace S3stat.SecureSetup.Pages
{
	/// <summary>
	/// Interaction logic for AWSCredentials.xaml
	/// </summary>
	public partial class AWSCredentials : AppControl, IContent
	{
		public AWSCredentials()
		{
			InitializeComponent();
			Loaded += AWSCredentials_Loaded;
		}

		public void OnNavigatedTo(NavigationEventArgs e)
		{
			if (String.IsNullOrEmpty(AppState.UserName) || String.IsNullOrEmpty(AppState.Password))
			{
				NavigateToLogin();
				return;
			}

			txtAWSAccessKey.Text = AppState.AWSAccessKey;
			txtAWSSecretKey.Text = AppState.AWSSecretKey;
			cbRemember.IsChecked = AppState.RememberAWSCredentials;
		}

		void AWSCredentials_Loaded(object sender, RoutedEventArgs e)
		{
		}

		private void Go()
		{
			AmazonS3Client s3 = new AmazonS3Client(txtAWSAccessKey.Text, txtAWSSecretKey.Text, RegionEndpoint.USEast1);
			try
			{
				s3.ListBuckets();
				txtAWSAccessKey.Text = txtAWSAccessKey.Text.Trim();
				txtAWSSecretKey.Text = txtAWSSecretKey.Text.Trim();
				AppState.AWSAccessKey = txtAWSAccessKey.Text;
				AppState.AWSSecretKey = txtAWSSecretKey.Text;
				AppState.RememberAWSCredentials = cbRemember.IsChecked.GetValueOrDefault(true);
			}
			catch (Exception e)
			{
				AppState.NoteException(e, "VerifyAWSCredentials", false);
				ErrorDetail.ShowMessage("Couldn't connect to AWS with the supplied credentials.  The link below should help troubleshoot this."
					, "Bad Credentials", e, "VerifyAWSCredentials");
				return;
			}

			AppState.AWSAccountID = IAMHelper.GetAccountID();
			if (String.IsNullOrEmpty(AppState.AWSAccountID))
			{
				ErrorDetail.ShowMessage("Couldn't retrieve AWS AccountID using the supplied credentials", "Bad Credentials");
				return;
			}
			AppState.Save();

			var uri = new Uri("/Pages/Role.xaml", UriKind.Relative);
			var frame = NavigationHelper.FindFrame(null, this);
			frame.Source = uri;


		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Go();
		}

		private void txtAWSSecretKey_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Go();
			}

		}

		public void OnFragmentNavigation(FragmentNavigationEventArgs e)
		{
		}

		public void OnNavigatedFrom(NavigationEventArgs e)
		{
		}


		public void OnNavigatingFrom(NavigatingCancelEventArgs e)
		{
		}
	}
}
