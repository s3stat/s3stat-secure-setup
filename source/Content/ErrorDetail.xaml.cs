using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace S3stat.SecureSetup.Content
{
	/// <summary>
	/// Interaction logic for ErrorDetail.xaml
	/// </summary>
	public partial class ErrorDetail : UserControl
	{
		public string Message
		{
			set { bbFriendlyText.BBCode = value; }
		}

		public Exception Exception
		{
			set
			{
				if (value != null)
				{
					txtErrorMessage.Text = value.Message;
					txtErrorMessage.Visibility = Visibility.Visible;
				}
				else
				{
					txtErrorMessage.Visibility = Visibility.Collapsed;
				}
			}
		}

		public string ErrorCode
		{
			set
			{
				if (!String.IsNullOrWhiteSpace(value))
				{
					var hash = Regex.Replace(txtErrorMessage.Text, @"[^a-zA-z0-9]+", "-");
					bbHelpLink.BBCode = String.Format("[url=https://www.s3stat.com/secure-setup-errors/{0}#{1}]How to fix this[/url]", value, hash);
					bbHelpLink.Visibility = Visibility.Visible;
				}
				else
				{
					bbHelpLink.Visibility = Visibility.Collapsed;
				}
			}
		}

		public ErrorDetail()
		{
			InitializeComponent();
		}

		public static void ShowMessage(string message, string title, Exception e = null, string errorCode = null)
		{
			var win = new ModernDialog
			{
				Title = title,
				Content = new ErrorDetail
				{
					Message = message,
					Exception = e,
					ErrorCode = errorCode
				}
			};
			win.Show();


		}
	}
}
