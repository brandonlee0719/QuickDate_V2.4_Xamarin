using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Java.Lang;
using QuickDate.Activities.Default;
using QuickDate.Activities.Tabbes;
using QuickDate.Activities.UserProfile;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using Exception = System.Exception;

namespace QuickDate.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/SplashScreenTheme", NoHistory = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", AutoVerify = false)]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryBrowsable, Intent.CategoryDefault }, DataSchemes = new[] { "http", "https" }, DataHost = "@string/ApplicationUrlWeb", DataPathPrefixes = new[] { "", "/register/", "/@" }, AutoVerify = false)]
    public class SplashScreenActivity : AppCompatActivity
    { 
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                 
                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e); 
            }
        }
          
        private void FirstRunExcite()
        {
            try
            {
                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
                    #pragma warning disable 618
                    UserDetails.LangName = (int)Build.VERSION.SdkInt < 25 ? Resources?.Configuration?.Locale?.Language.ToLower() : Resources?.Configuration?.Locales.Get(0)?.Language.ToLower() ?? Resources?.Configuration?.Locale?.Language.ToLower();
                    #pragma warning restore 618
                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                if (!string.IsNullOrEmpty(UserDetails.AccessToken))
                {
                    switch (string.IsNullOrEmpty(UserDetails.AccessToken))
                    {
                        case false when Intent?.Data?.Path != null:
                            {
                                if (Intent.Data.Path.Contains("register") && UserDetails.Status != "Active" && UserDetails.Status != "Pending")
                                {
                                    StartActivity(new Intent(this, typeof(RegisterActivity)));
                                }
                                else if (Intent.Data.Path.Contains("@") && (UserDetails.Status == "Active" || UserDetails.Status == "Pending"))
                                {
                                    var username = Intent.Data.Path.Split("@").Last();

                                    var intent = new Intent(this, typeof(UserProfileActivity)); 
                                    intent.PutExtra("EventPage", "Close");  
                                    intent.PutExtra("DataType", "Search");
                                    //intent.PutExtra("ItemUser", JsonConvert.SerializeObject(OneSignalNotification.UserData));
                                    intent.PutExtra("Username", username.ToLower());
                                    StartActivity(intent);
                                }
                                else
                                {
                                    switch (UserDetails.Status)
                                    {
                                        case "Active":
                                        case "Pending":
                                            StartActivity(new Intent(this, typeof(HomeActivity)));
                                            break;
                                        default:
                                            StartActivity(new Intent(this, typeof(FirstActivity)));
                                            break;
                                    }
                                }

                                break;
                            }
                        case false:
                            switch (UserDetails.Status)
                            {
                                case "Active":
                                case "Pending":
                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    break;
                                default:
                                    StartActivity(new Intent(this, typeof(FirstActivity)));
                                    break;
                            }
                            break;
                        default:
                            StartActivity(new Intent(this, typeof(FirstActivity)));
                            break;
                    } 
                }
                else
                {
                    StartActivity(new Intent(this, typeof(FirstActivity)));
                }

                OverridePendingTransition(Resource.Animation.abc_fade_in, Resource.Animation.abc_fade_out);
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            { 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}