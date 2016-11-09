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
using S3stat.SecureSetup.Content;
using S3stat.SecureSetup.Helpers;

namespace SecureSetup.Pages
{
	/// <summary>
	/// Interaction logic for S3statLogin.xaml
	/// </summary>
	public partial class S3statLogin : UserControl
	{
		public S3statLogin()
		{
			InitializeComponent();
			Loaded += S3statLogin_Loaded;
		}

		void S3statLogin_Loaded(object sender, RoutedEventArgs e)
		{
			txtUserName.Text = AppState.UserName;
			txtPassword.Password = AppState.Password;
			cbRemember.IsChecked = AppState.RememberS3statLogin;
		}

		private void Go()
		{
			S3statHelper stat = new S3statHelper(txtUserName.Text, txtPassword.Password);
			if (stat.CheckUser())
			{
				AppState.UserName = txtUserName.Text;
				AppState.Password = txtPassword.Password;
				AppState.RememberS3statLogin = cbRemember.IsChecked.GetValueOrDefault(true);
				AppState.Save();

				var uri = new Uri("/Pages/AWSCredentials.xaml", UriKind.Relative);
				var frame = NavigationHelper.FindFrame(null, this);
				frame.Source = uri;
			}
			else
			{
				ErrorDetail.ShowMessage("Couldn't find an S3stat account with this username and password", "Login Failed");
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Go();
		}

		private void txtPassword_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				Go();
			}
		}
	}
}
