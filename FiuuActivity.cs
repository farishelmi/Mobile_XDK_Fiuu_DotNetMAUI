using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Interop;
using Newtonsoft.Json;

namespace FiuuXDKExample
{
    [Activity(
        Label = "FiuuXDKExample",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation
            | Android.Content.PM.ConfigChanges.ScreenSize
    )]
    public class FiuuActivity : Activity
    {
        public const int FiuuXDK = 9999;
        public const string FiuuPaymentDetails = "paymentDetails";
        public const string FiuuTransactionResult = "transactionResult";
        public const string mp_amount = "mp_amount";
        public const string mp_username = "mp_username";
        public const string mp_password = "mp_password";
        public const string mp_merchant_ID = "mp_merchant_ID";
        public const string mp_app_name = "mp_app_name";
        public const string mp_order_ID = "mp_order_ID";
        public const string mp_currency = "mp_currency";
        public const string mp_country = "mp_country";
        public const string mp_verification_key = "mp_verification_key";
        public const string mp_channel = "mp_channel";
        public const string mp_bill_description = "mp_bill_description";
        public const string mp_bill_name = "mp_bill_name";
        public const string mp_bill_email = "mp_bill_email";
        public const string mp_bill_mobile = "mp_bill_mobile";
        public const string mp_channel_editing = "mp_channel_editing";
        public const string mp_editing_enabled = "mp_editing_enabled";
        public const string mp_transaction_id = "mp_transaction_id";
        public const string mp_request_type = "mp_request_type";
        public const string mp_is_escrow = "mp_is_escrow";
        public const string mp_bin_lock = "mp_bin_lock";
        public const string mp_bin_lock_err_msg = "mp_bin_lock_err_msg";
        public const string mp_custom_css_url = "mp_custom_css_url";
        public const string mp_preferred_token = "mp_preferred_token";
        public const string mp_tcctype = "mp_tcctype";
        public const string mp_is_recurring = "mp_is_recurring";
        public const string mp_sandbox_mode = "mp_sandbox_mode";
        public const string mp_allowed_channels = "mp_allowed_channels";
        public const string mp_express_mode = "mp_express_mode";
        public const string mp_advanced_email_validation_enabled =
            "mp_advanced_email_validation_enabled";
        public const string mp_advanced_phone_validation_enabled =
            "mp_advanced_phone_validation_enabled";
        public const string mp_bill_name_edit_disabled = "mp_bill_name_edit_disabled";
        public const string mp_bill_email_edit_disabled = "mp_bill_email_edit_disabled";
        public const string mp_bill_mobile_edit_disabled = "mp_bill_mobile_edit_disabled";
        public const string mp_bill_description_edit_disabled = "mp_bill_description_edit_disabled";
        public const string mp_language = "mp_language";
        public const string mp_dev_mode = "mp_dev_mode";
        public const string mp_cash_waittime = "mp_cash_waittime";
        public const string mp_non_3DS = "mp_non_3DS";
        public const string mp_card_list_disabled = "mp_card_list_disabled";
        public const string mp_disabled_channels = "mp_disabled_channels";

        private const string mpopenmolpaywindow = "mpopenmolpaywindow://";
        private const string mpcloseallwindows = "mpcloseallwindows://";
        private const string mptransactionresults = "mptransactionresults://";
        private const string mprunscriptonpopup = "mprunscriptonpopup://";
        private const string mppinstructioncapture = "mppinstructioncapture://";
        private const string molpayresulturl = "MOLPay/result.php";
        private const string molpaynbepayurl = "MOLPay/nbepay.php";
        private const string module_id = "module_id";
        private const string wrapper_version = "wrapper_version";
        private static FiuuActivity fiuuActivity;
        private static WebView mpMainUI, fiuuUI, mpBankUI;
        private static TextView tw;
        private static Dictionary<string, object> paymentDetails;
        private static bool isMainUILoaded = false;
        private static bool isClosingReceipt = false;

        public class Image
        {
            public string filename { get; set; }
            public string base64ImageUrlData { get; set; }
        }

        private static void CloseFiuu()
        {
            mpMainUI.LoadUrl("javascript:closemolpay()");
            if (isClosingReceipt)
            {
                isClosingReceipt = false;
                fiuuActivity.Finish();
            }
        }

        public override void OnBackPressed()
        {
            CloseFiuu();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_fiuu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.closeBtn:
                    CloseFiuu();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            isMainUILoaded = false;
            isClosingReceipt = false;
            fiuuActivity = this;

            Window.SetFlags(WindowManagerFlags.Secure, WindowManagerFlags.Secure);
            SetContentView(Resource.Layout.layout_fiuu);

            string json = Intent.GetStringExtra(FiuuPaymentDetails);
            paymentDetails = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            paymentDetails.Add(module_id, "fiuu-mobile-xdk-dotnet-maui");
            paymentDetails.Add(wrapper_version, "0");

            mpMainUI = FindViewById<WebView>(Resource.Id.MPMainUI);
            fiuuUI = FindViewById<WebView>(Resource.Id.FiuuUI);
            tw = FindViewById<TextView>(Resource.Id.resultTV);

            mpMainUI.Settings.JavaScriptEnabled = true;
            fiuuUI.Settings.JavaScriptEnabled = true;

            fiuuUI.Visibility = ViewStates.Gone;
            tw.Visibility = ViewStates.Gone;

            mpMainUI.Settings.AllowUniversalAccessFromFileURLs = true;
            mpMainUI.SetWebViewClient(new MPMainUIWebClient());
            mpMainUI.LoadUrl("https://pay.merchant.razer.com/RMS/API/xdk/");

            fiuuUI.Settings.AllowUniversalAccessFromFileURLs = true;
            fiuuUI.Settings.JavaScriptCanOpenWindowsAutomatically = true;
            fiuuUI.Settings.SetSupportMultipleWindows(true);
            fiuuUI.Settings.DomStorageEnabled = true;
            fiuuUI.SetWebViewClient(new FiuuUIWebClient());
            fiuuUI.SetWebChromeClient(new FiuuUIWebChromeClient());

            CookieManager.Instance.SetAcceptCookie(true);
        }

        public class MPMainUIWebClient : WebViewClient
        {
            [Obsolete]
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                Console.WriteLine("MPMainUIWebClient shouldOverrideUrlLoading url = " + url);

                if (url != null && url.StartsWith(mpopenmolpaywindow))
                {
                    string base64String = url.Replace(mpopenmolpaywindow, "");
                    Console.WriteLine(
                        "MPMainUIWebClient mpopenmolpaywindow base64String = " + base64String
                    );

                    string dataString = Base64Decode(base64String);
                    Console.WriteLine(
                        "MPMainUIWebClient mpopenmolpaywindow dataString = " + dataString
                    );

                    if (dataString.Length > 0)
                    {
                        Console.WriteLine("MPMainUIWebClient mpopenmolpaywindow success");
                        fiuuUI.LoadDataWithBaseURL("", dataString, "text/html", "UTF-8", "");
                        fiuuUI.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        Console.WriteLine("MPMainUIWebClient mpopenmolpaywindow empty dataString");
                    }
                }
                else if (url != null && url.StartsWith(mpcloseallwindows))
                {
                    if (mpBankUI != null)
                    {
                        mpBankUI.LoadUrl("about:blank");
                        mpBankUI.Visibility = ViewStates.Gone;
                        mpBankUI.ClearCache(true);
                        mpBankUI.ClearHistory();
                        mpBankUI.RemoveAllViews();
                        mpBankUI.Destroy();
                        mpBankUI = null;
                    }

                    if (fiuuUI != null)
                    {
                        fiuuUI.LoadUrl("about:blank");
                        fiuuUI.Visibility = ViewStates.Gone;
                        fiuuUI.ClearCache(true);
                        fiuuUI.ClearHistory();
                        fiuuUI.RemoveAllViews();
                        fiuuUI.Destroy();
                        fiuuUI = null;
                    }
                }
                else if (url != null && url.StartsWith(mptransactionresults))
                {
                    string base64String = url.Replace(mptransactionresults, "");
                    Console.WriteLine(
                        "MPMainUIWebClient mptransactionresults base64String = " + base64String
                    );

                    string dataString = Base64Decode(base64String);
                    Console.WriteLine(
                        "MPMainUIWebClient mptransactionresults dataString = " + dataString
                    );

                    fiuuActivity.PassTransactionResultBack(dataString);
                    try
                    {
                        Dictionary<string, object> jsonResult = JsonConvert.DeserializeObject<
                            Dictionary<string, object>
                        >(dataString);

                        Console.WriteLine(
                            "MPMainUIWebClient jsonResult = " + jsonResult.ToString()
                        );

                        Object requestType;
                        jsonResult.TryGetValue("mp_request_type", out requestType);
                        if (
                            !jsonResult.ContainsKey("mp_request_type")
                            || (string)requestType != "Receipt"
                            || jsonResult.ContainsKey("error_code")
                        )
                        {
                            fiuuActivity.Finish();
                        }
                        else
                        {
                            fiuuActivity.Window.ClearFlags(WindowManagerFlags.Secure);
                            isClosingReceipt = true;
                        }
                    }
                    catch (Exception)
                    {
                        fiuuActivity.Finish();
                    }
                }
                else if (url != null && url.StartsWith(mprunscriptonpopup))
                {
                    string base64String = url.Replace(mprunscriptonpopup, "");
                    Console.WriteLine(
                        "MPMainUIWebClient mprunscriptonpopup base64String = " + base64String
                    );

                    string jsString = Base64Decode(base64String);
                    Console.WriteLine(
                        "MPMainUIWebClient mprunscriptonpopup jsString = " + jsString
                    );

                    if (mpBankUI != null)
                    {
                        mpBankUI.LoadUrl("javascript:" + jsString);
                        Console.WriteLine("mpBankUI loadUrl = " + "javascript:" + jsString);
                    }
                }
                else if (url != null && url.StartsWith(mppinstructioncapture))
                {
                    string base64String = url.Replace(mppinstructioncapture, "");
                    Console.WriteLine(
                        "MPMainUIWebClient mppinstructioncapture base64String = " + base64String
                    );

                    string jsonString = Base64Decode(base64String);
                    Console.WriteLine(
                        "MPMainUIWebClient mppinstructioncapture jsonString = " + jsonString
                    );

                    var json = JsonConvert.DeserializeObject<Image>(jsonString);
                    byte[] imageAsBytes = Base64.Decode(
                        json.base64ImageUrlData,
                        Base64Flags.Default
                    );
                    Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeByteArray(
                        imageAsBytes,
                        0,
                        imageAsBytes.Length
                    );

                    var sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                    var filePath = System.IO.Path.Combine(sdCardPath.ToString(), json.filename);
                    var stream = new FileStream(filePath, FileMode.Create);

                    bool compress = bitmap.Compress(
                        Android.Graphics.Bitmap.CompressFormat.Png,
                        100,
                        stream
                    );
                    stream.Close();

                    MediaScannerConnection.ScanFile(
                        Application.Context,
                        new string[] { filePath },
                        null,
                        null
                    );

                    if (compress)
                    {
                        Toast.MakeText(fiuuActivity, "Image saved", ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(fiuuActivity, "Image not saved", ToastLength.Short).Show();
                    }
                }

                return true;
            }

            public override void OnPageFinished(WebView view, string url)
            {
                if (!isMainUILoaded && url != "about:blank")
                {
                    isMainUILoaded = true;
                    mpMainUI.LoadUrl(
                        "javascript:updateSdkData("
                            + JsonConvert.SerializeObject(paymentDetails)
                            + ")"
                    );
                }
            }
        }

        public class FiuuUIWebClient : WebViewClient
        {
            public override void OnPageStarted(
                WebView view,
                string url,
                Android.Graphics.Bitmap favicon
            )
            {
                Console.WriteLine("FiuuUIWebClient onPageStarted url = " + url);

                if (url != null)
                {
                    NativeWebRequestUrlUpdates(url);
                }
            }

            public override void OnPageFinished(WebView view, string url)
            {
                Console.WriteLine("FiuuUIWebClient onPageFinished url = " + url);

                if (
                    url.Contains("intermediate_appTNG-EWALLET.php")
                    || url.Contains("intermediate_app/processing.php")
                )
                {
                    Console.WriteLine("FiuuUIWebClient tngd found!");
                    JavascriptResult jr = new JavascriptResult();
                    view.EvaluateJavascript(
                        $"document.getElementById('systembrowserurl').innerHTML",
                        jr
                    );
                }
            }

            public class JavascriptResult : Java.Lang.Object, IValueCallback
            {
                public string Result;

                public void OnReceiveValue(Java.Lang.Object result)
                {
                    Result = ((Java.Lang.String)result).ToString();

                    byte[] data = Base64.Decode(Result, Base64Flags.Default);
                    String dataString = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);

                    Console.WriteLine("FiuuUIWebClient JavascriptResult = " + dataString);

                    var uri = Android.Net.Uri.Parse(dataString);
                    var intent = new Intent(Intent.ActionView, uri);
                    intent.AddFlags(ActivityFlags.NewTask);
                    Application.Context.StartActivity(intent);
                }
            }
        }

        public class FiuuUIWebChromeClient : WebChromeClient
        {
            public override bool OnCreateWindow(
                WebView view,
                bool dialog,
                bool userGesture,
                Message resultMsg
            )
            {
                Console.WriteLine(
                    "FiuuUIWebChromeClient onCreateWindow resultMsg = " + resultMsg
                );

                RelativeLayout container = (RelativeLayout)
                    fiuuActivity.FindViewById(Resource.Id.MPContainer);

                mpBankUI = new WebView(fiuuActivity);
                mpBankUI.Settings.JavaScriptEnabled = true;
                mpBankUI.Settings.AllowUniversalAccessFromFileURLs = true;
                mpBankUI.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                mpBankUI.Settings.SetSupportMultipleWindows(true);
                mpBankUI.SetWebViewClient(new MPBankUIWebClient());
                mpBankUI.SetWebChromeClient(new MPBankUIWebChromeClient());
                mpBankUI.LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent,
                    LinearLayout.LayoutParams.MatchParent
                );

                container.AddView(mpBankUI);
                WebView.WebViewTransport transport = (WebView.WebViewTransport)resultMsg.Obj;
                transport.WebView = mpBankUI;
                resultMsg.SendToTarget();
                return true;
            }
        }

        public class MPBankUIWebClient : WebViewClient
        {
            public override void OnPageStarted(
                WebView view,
                string url,
                Android.Graphics.Bitmap favicon
            )
            {
                Console.WriteLine("MPBankUIWebClient onPageStarted url = " + url);

                if (url != null)
                {
                    NativeWebRequestUrlUpdates(url);
                }
            }

            public override void OnPageFinished(WebView view, string url)
            {
                Console.WriteLine("MPBankUIWebClient onPageFinished url = " + url);
                NativeWebRequestUrlUpdates(url);
            }
        }

        public class MPBankUIWebChromeClient : WebChromeClient
        {
            public override void OnCloseWindow(WebView window)
            {
                CloseFiuu();
            }
        }

        private static void NativeWebRequestUrlUpdates(string url)
        {
            Console.WriteLine("nativeWebRequestUrlUpdates url = " + url);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("requestPath", url);
            mpMainUI.LoadUrl(
                "javascript:nativeWebRequestUrlUpdates(" + JsonConvert.SerializeObject(data) + ")"
            );
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private void PassTransactionResultBack(string dataString)
        {
            Intent result = new Intent();
            result.PutExtra(FiuuTransactionResult, dataString);
            SetResult(Result.Ok, result);
        }
    }
}
