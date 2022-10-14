using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Braintreepayments.Api.Dropin;
using Com.Google.Android.Play.Core.Install.Model;
using InAppBilling.Lib;
using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using Newtonsoft.Json;
using QuickDate.Activities.Chat;
using QuickDate.Activities.Chat.Service;
using QuickDate.Activities.Live.Utils;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes.Fragment;
using QuickDate.Activities.UserProfile;
using QuickDate.ButtomSheets;
using QuickDate.Helpers.CacheLoaders;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.Library.OneSignalNotif;
using QuickDate.PaymentGoogle;
using QuickDate.SQLite;
using QuickDateClient.Classes.Authorization;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Classes.Global; 
using QuickDateClient.Requests;
using Uri = Android.Net.Uri;
using Exception = System.Exception;
using Permission = Android.Content.PM.Permission;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
    public class HomeActivity : AppCompatActivity 
    {
        #region Variables Basic

        public CardMachFragment CardFragment;
        public TrendingFragment TrendingFragment;
        public NotificationsFragment NotificationsFragment;
        public ProfileFragment ProfileFragment;
        public LastChatFragment LastChatFragment;
        public LinearLayout NavigationTabBar;
        public CustomNavigationController FragmentBottomNavigator;

        private OpenGalleryDialogFor TypeAvatar;
        public static long CountNotificationsStatic, CountMessagesStatic;
        private static HomeActivity Instance;
        public TracksCounter TracksCounter;
        private PowerManager.WakeLock Wl; 
        private readonly Handler ExitHandler = new Handler(Looper.MainLooper);
        private bool RecentlyBackPressed;

        public InitPayPalPayment InitPayPalPayment;
        public InitInAppBillingPayment BillingPayment;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                AddFlagsWakeLock();

                // Create your application here
                SetContentView(Resource.Layout.TabbedMainLayout);

                Instance = this;

                TracksCounter = new TracksCounter(this);
                 
                if (AppSettings.ShowPaypal)
                    InitPayPalPayment = new InitPayPalPayment(this);

                if (AppSettings.ShowInAppBilling)
                    BillingPayment = new InitInAppBillingPayment(this);

                if (AppSettings.EnableAppFree)
                    AppSettings.PremiumSystemEnabled = false;

                //Get Value
                SetupBottomNavigationView();

                GetMyInfoData();

                string type = Intent?.GetStringExtra("TypeNotification") ?? "Don't have type";
                if (!string.IsNullOrEmpty(type) && type != "Don't have type")
                {
                    //var result = DbDatabase.Get_data_Login_Credentials();

                    var intent = new Intent(this, typeof(UserProfileActivity));

                    switch (type)
                    {
                        case "got_new_match":
                            intent.PutExtra("EventPage", "HideButton");
                            break;
                        case "like":
                            intent.PutExtra("EventPage", "likeAndClose");
                            break;
                        default:
                            intent.PutExtra("EventPage", "Close");
                            break;
                    }

                    intent.PutExtra("DataType", "OneSignal");
                    intent.PutExtra("ItemUser", JsonConvert.SerializeObject(OneSignalNotification.UserData));
                    StartActivity(intent);
                }

                SetService();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        MainSettings.ApplyTheme(MainSettings.LightMode);
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        MainSettings.ApplyTheme(MainSettings.DarkMode);
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        protected override void OnResume()
        {
            try
            {
                base.OnResume(); 
                SetWakeLock();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause(); 
                CardFragment?.SaveSwipeCount();
                OffWakeLock();
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

        public static HomeActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        protected override void OnDestroy()
        {
            try
            { 
                if (AppSettings.ShowInAppBilling)
                    BillingPayment?.DisconnectInAppBilling();

                ProfileFragment?.Time?.SetStopTimer();

                OffWakeLock();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        public void SetToolBar(Toolbar toolbar, string title, bool setBackground, bool showIconBack = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (setBackground)
                        toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetupBottomNavigationView()
        {
            try
            {
                NavigationTabBar = FindViewById<LinearLayout>(Resource.Id.ntb_horizontal);

                FragmentBottomNavigator = new CustomNavigationController(this);
                CardFragment = new CardMachFragment();

                if (AppSettings.ShowTrending)
                    TrendingFragment = new TrendingFragment();

                NotificationsFragment = new NotificationsFragment();
                LastChatFragment = new LastChatFragment();
                ProfileFragment = new ProfileFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(CardFragment);

                if (AppSettings.ShowTrending)
                    FragmentBottomNavigator.FragmentListTab1.Add(TrendingFragment);

                FragmentBottomNavigator.FragmentListTab2.Add(NotificationsFragment);
                FragmentBottomNavigator.FragmentListTab3.Add(LastChatFragment);
                FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);

                FragmentBottomNavigator.ShowFragment0(); 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
        
        public void ShowMessagesBox(UserInfoObject dataUser)
        {
            try
            {
                Intent intent = new Intent(this, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", dataUser.Id.ToString());
                intent.PutExtra("TypeChat", "LastChat");
                intent.PutExtra("UserItem", JsonConvert.SerializeObject(dataUser));

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartActivity(intent);
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        StartActivity(intent);

                    }
                    else
                        new PermissionsController(this).RequestPermission(100);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OpenDialogGallery(OpenGalleryDialogFor typeAvatar = OpenGalleryDialogFor.Other)
        {
            try
            {
                TypeAvatar = typeAvatar;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Activity()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Activity()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        //request Code 108
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OpenAddPhotoFragment()
        {
            try
            {
                var addPhoto = new AddPhotoBottomDialogFragment();
                addPhoto.Show(SupportFragmentManager, "addPhoto");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (AppSettings.ShowInAppBilling)
                    BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);

                switch (requestCode)
                {
                    case 108:
                    case CropImage.CropImageActivityRequestCode when resultCode == Result.Ok:
                    {
                        if (Methods.CheckConnectivity())
                        {
                            var result = CropImage.GetActivityResult(data);
                            if (result.IsSuccessful)
                            {
                                var resultPathImage = result.Uri.Path;
                                if (!string.IsNullOrEmpty(resultPathImage))
                                {
                                    if (TypeAvatar == OpenGalleryDialogFor.Avatar)
                                    {
                                        GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment?.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => ApiRequest.UpdateAvatarApi(this, resultPathImage)});
                                    }
                                    else
                                    {
                                        //sent api  
                                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => RequestsAsync.Users.UploadMediaFileUserAsync(resultPathImage)});
                                    }

                                    var reviewMediaFiles = ListUtils.SettingsSiteList?.ReviewMediaFiles;
                                    if (reviewMediaFiles == "1") //Uploaded successfully, file will be reviewed
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_UploadedSuccessfullyWithReviewed), ToastLength.Long)?.Show();
                                    else
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_UploadedSuccessfully), ToastLength.Long)?.Show();
                                }
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                        }

                        break;
                    }
                    case 1050:
                        //Get Location And Get Data Api
                        CardFragment?.CheckAndGetLocation();
                        break;
                    case InitPayPalPayment.PayPalDataRequestCode: 
                        switch (resultCode)
                        {
                            case Result.Ok:
                                {
                                    DropInResult dropInResult = (DropInResult)data.GetParcelableExtra(DropInResult.ExtraDropInResult);
                                    if (dropInResult != null)
                                        InitPayPalPayment.DisplayResult(dropInResult.PaymentMethodNonce, dropInResult.DeviceData);

                                    if (InitPayPalPayment?.PayType != "membership")
                                    {
                                        if (Methods.CheckConnectivity())
                                        {
                                            var (apiStatus, respond) = await RequestsAsync.Auth.SetCreditAsync(InitPayPalPayment?.Credits, InitPayPalPayment?.Price, "PayPal");
                                            if (apiStatus == 200)
                                            {
                                                if (respond is SetCreditObject result)
                                                {
                                                    var dataUser = ListUtils.MyUserInfo?.FirstOrDefault(a => a.Id == UserDetails.UserId);
                                                    if (dataUser != null)
                                                    {
                                                        dataUser.Balance = result.Balance;

                                                        var sqlEntity = new SqLiteDatabase();
                                                        sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                                    }

                                                    if (ProfileFragment.WalletNumber != null)
                                                        ProfileFragment.WalletNumber.Text = result.Balance;

                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                                                }
                                            }
                                            else Methods.DisplayReportResult(this, respond);
                                        }
                                        else
                                        {
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                        }
                                    }
                                    else
                                    {
                                        if (Methods.CheckConnectivity())
                                        {
                                            var (apiStatus, respond) = await RequestsAsync.Auth.SetProAsync(InitPayPalPayment?.Id, InitPayPalPayment?.Price, "PayPal");
                                            if (apiStatus == 200)
                                            {
                                                var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.VerifiedFinal = true;
                                                    dataUser.IsPro = "1";

                                                    var sqlEntity = new SqLiteDatabase();
                                                    sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                                }

                                                Toast.MakeText(this, GetText(Resource.String.Lbl_SuccessfullyUpgraded), ToastLength.Long)?.Show();
                                            }
                                            else Methods.DisplayReportResult(this, respond);
                                        }
                                        else
                                        {
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                        }
                                    } 
                                }
                                break;
                            case Result.Canceled:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long)?.Show();
                                break;
                            case Result.FirstUser:
                                var ss = (Exception)data.GetSerializableExtra(DropInActivity.ExtraError);
                                Toast.MakeText(this, ss?.Message, ToastLength.Long)?.Show();
                                break;
                        }
                        break;
                    case BillingProcessor.PurchaseFlowRequestCode when resultCode == Result.Ok && AppSettings.ShowInAppBilling:
                        {
                            if (BillingPayment?.PayType != "membership")
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    var (apiStatus, respond) = await RequestsAsync.Auth.SetCreditAsync(BillingPayment?.Credits, BillingPayment?.Price, "Google InApp");
                                    if (apiStatus == 200)
                                    {
                                        if (respond is SetCreditObject result)
                                        {
                                            var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                                            if (dataUser != null)
                                            {
                                                dataUser.Balance = result.Balance;

                                                var sqlEntity = new SqLiteDatabase();
                                                sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                            }

                                            if (ProfileFragment.WalletNumber != null)
                                                ProfileFragment.WalletNumber.Text = result.Balance.Replace(".00", "");

                                            Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                                        }
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }
                            else
                            {
                                if (Methods.CheckConnectivity())
                                {
                                    var (apiStatus, respond) = await RequestsAsync.Auth.SetProAsync(BillingPayment?.Id, BillingPayment?.Price, "Google InApp");
                                    if (apiStatus == 200)
                                    {
                                        var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                                        if (dataUser != null)
                                        {
                                            dataUser.VerifiedFinal = true;
                                            dataUser.IsPro = "1";

                                            var sqlEntity = new SqLiteDatabase();
                                            sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                                        }

                                        Toast.MakeText(this, GetText(Resource.String.Lbl_SuccessfullyUpgraded), ToastLength.Long)?.Show();
                                    }
                                    else Methods.DisplayReportResult(this, respond);
                                }
                                else
                                {
                                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                                }
                            }

                            break;
                        }
                    case 4711:
                        switch (resultCode) // The switch block will be triggered only with flexible update since it returns the install result codes
                        {
                            case Result.Ok:
                                // In app update success
                                if (UpdateManagerApp.AppUpdateTypeSupported == AppUpdateType.Immediate)
                                    Toast.MakeText(this, "App updated", ToastLength.Short)?.Show();
                                break;
                            case Result.Canceled:
                                Toast.MakeText(this, "In app update cancelled", ToastLength.Short)?.Show();
                                break;
                            case (Result)ActivityResult.ResultInAppUpdateFailed:
                                Toast.MakeText(this, "In app update failed", ToastLength.Short)?.Show();
                                break;
                        }

                        break;
                        //case PaymentActivity.ResultExtrasInvalid://wael
                        //    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long)?.Show();
                        //    break;  
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 105 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //Get Location And Get Data Api
                        CardFragment?.CheckAndGetLocation();
                        break;
                    case 105:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 108 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        OpenDialogGallery(TypeAvatar);
                        break;
                    case 108:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 110 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                        break;
                    case 110:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 100 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        ShowMessagesBox(DialogController.DataUser);
                        break;
                    case 100:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                    case 111 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        new LiveUtil(this).GoLiveOnClick();
                        break;
                    case 111:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Back Pressed 

        public override void OnBackPressed()
        {
            try
            {
                int count = LastChatFragment.MAdapter?.GetSelectedItemCount() ?? 0;
                if (count > 0)
                {
                    LastChatFragment.ToolBar.Visibility = ViewStates.Visible;
                    LastChatFragment.ActionMode.Finish();
                }
                else
                {
                    if (FragmentBottomNavigator.GetCountFragment() > 0)
                    {
                        FragmentBottomNavigator.OnBackStackClickFragment();
                    }
                    else
                    {
                        if (RecentlyBackPressed)
                        {
                            ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                            RecentlyBackPressed = false;
                            MoveTaskToBack(true);
                            Finish();
                        }
                        else
                        {
                            RecentlyBackPressed = true;
                            Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long)?.Show();
                            ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Timer

        public async void GetNotifications()
        {
            try
            {
                if (FragmentBottomNavigator != null)
                {
                    var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications(this).ConfigureAwait(false);

                    if (FragmentBottomNavigator.NotificationImage != null && countNotifications != 0 && countNotifications != CountNotificationsStatic)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                CountNotificationsStatic = countNotifications;
                                FragmentBottomNavigator.ShowNotificationBadge(true);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                CountNotificationsStatic = countNotifications;
                                FragmentBottomNavigator.ShowNotificationBadge(false);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                    return;
                }

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.GetSettings();
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                var dataUser = dbDatabase.GetDataMyInfo();
                if (dataUser != null)
                {
                    Glide.With(this).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();
                    GlideImageLoader.LoadImage(this, UserDetails.Avatar, FragmentBottomNavigator.ProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                }
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });

                var listStickers = dbDatabase.GetStickersList();
                if (ListUtils.StickersList.Count == 0 && listStickers?.Count > 0)
                    ListUtils.StickersList = listStickers;
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetStickers(this) });

                var listGifts = dbDatabase.GetGiftsList();
                if (ListUtils.GiftsList.Count == 0 && listGifts?.Count > 0)
                    ListUtils.GiftsList = listGifts;
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetGifts(this) });

                InAppUpdate();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void InAppUpdate()
        {
            try
            {
                if (AppSettings.ShowSettingsUpdateManagerApp)
                    UpdateManagerApp.CheckUpdateApp(this, 4711, Intent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private static int CountRateApp;
        public void InAppReview()
        {
            try
            {
                bool inAppReview = MainSettings.InAppReview.GetBoolean(MainSettings.PrefKeyInAppReview, false);
                if (!inAppReview && AppSettings.ShowSettingsRateApp)
                {
                    if (CountRateApp == AppSettings.ShowRateAppCount)
                    {
                        var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                        dialog.Title(GetText(Resource.String.Lbl_RateOurApp));
                        dialog.Content(GetText(Resource.String.Lbl_RateOurAppContent));
                        dialog.PositiveText(GetText(Resource.String.Lbl_Rate)).OnPositive((materialDialog, action) =>
                        {
                            try
                            {
                                StoreReviewApp store = new StoreReviewApp();
                                store.OpenStoreReviewPage(PackageName);
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        dialog.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new MyMaterialDialog());
                        dialog.AlwaysCallSingleChoiceCallback();
                        dialog.Build().Show();

                        MainSettings.InAppReview?.Edit()?.PutBoolean(MainSettings.PrefKeyInAppReview, true)?.Commit();
                    }
                    else
                    {
                        CountRateApp++;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
 
        #region WakeLock System

        private void AddFlagsWakeLock()
        {
            try
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.WakeLock) == Permission.Granted)
                    {
                        Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                    }
                    else
                    {
                        //request Code 110
                        new PermissionsController(this).RequestPermission(110);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetWakeLock()
        {
            try
            {
                if (Wl == null)
                {
                    PowerManager pm = (PowerManager)GetSystemService(PowerService);
                    Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                    Wl?.Acquire();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOnWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ScreenBright, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetOffWakeLock()
        {
            try
            {
                PowerManager pm = (PowerManager)GetSystemService(PowerService);
                Wl = pm.NewWakeLock(WakeLockFlags.ProximityScreenOff, "My Tag");
                Wl?.Acquire();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OffWakeLock()
        {
            try
            {
                // ..screen will stay on during this section..
                Wl?.Release();
                Wl = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    //var intent = new Intent(this, typeof(ChatApiService)); 
                    //StartService(intent);

                    // reschedule the job
                    ChatJobInfo.ScheduleJob(this); 
                }
                else
                {
                    //var intentService = new Intent(this, typeof(ChatApiService));
                    //StopService(intentService);

                    // Cancel all jobs
                    ChatJobInfo.StopJob(this);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void OnReceiveResult(string resultData) 
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetConversationListObject>(resultData);
                if (result != null)
                {
                    LastChatFragment?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    } 
}