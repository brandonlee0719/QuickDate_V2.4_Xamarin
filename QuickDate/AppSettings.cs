//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PixelPhoto 15/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================
//For the accuracy of the icon and logo, please use this website " http://nsimage.brosteins.com " and add images according to size in folders " mipmap " 

using Android.Graphics;
using QuickDate.Helpers.Model;

namespace QuickDate
{
    internal static class AppSettings
    {
        /// <summary>
        /// Deep Links To App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">quickdatescript.com</string>
        /// </summary> 
        public static string TripleDesAppServiceProvider = "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";

        //Main Settings >>>>>
        //********************************************************* 
        public static string Version = "2.4";
        public static string ApplicationName = "QuickDate";
        public static string DatabaseName = "QuickDate"; 

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#a33596"; 
        public static Color TitleTextColor = Color.Black;
        public static Color TitleTextColorDark = Color.White;

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = true;
        public static string Lang = ""; //Default language ar

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "c6d8ecf6-e3b8-4c49-b208-07a23364a6ed"; 

        //********************************************************* 

        //Add Animation Image User
        //*********************************************************
        public static bool EnableAddAnimationImageUser = false;
         
        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml  
        //Google >> ../Properties/AndroidManifest.xml .. line 42
        //*********************************************************
        public static bool EnableSmartLockForPasswords = true;//#New

        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true; 
        public static bool ShowWoWonderLogin = true;  
        public static bool ShowSocialLoginAtRegisterScreen = true;
         
        public static string ClientId = "716215768781-1riglii0rihhc9gmp53qad69tt8o2e03.apps.googleusercontent.com";

        public static string AppNameWoWonder = "WoWonder";

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static ShowAds ShowAds = ShowAds.AllUsers;//#New

        public static bool ShowAdMobBanner = true;
        public static bool ShowAdMobInterstitial = true;
        public static bool ShowAdMobRewardVideo = true;
        public static bool ShowAdMobNative = true;
        public static bool ShowAdMobAppOpen = true;  
        public static bool ShowAdMobRewardedInterstitial = true; 

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/6657648824";
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/7559666953";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/2342769069";
        public static string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/7036343147";  
        public static string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/9662506481";  

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3;
        public static int ShowAdMobRewardedVideoCount = 3;
        public static int ShowAdMobAppOpenCount = 2;  
        public static int ShowAdMobRewardedInterstitialCount = 3;  

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowFbBannerAds = false; 
        public static bool ShowFbInterstitialAds = false; 
        public static bool ShowFbRewardVideoAds = false;  
        public static bool ShowFbNativeAds = false; 

        //YOUR_PLACEMENT_ID
        public static string AdsFbBannerKey = "250485588986218_554026418632132"; 
        public static string AdsFbInterstitialKey = "250485588986218_554026125298828";  
        public static string AdsFbRewardVideoKey = "250485588986218_554072818627492";  
        public static string AdsFbNativeKey = "250485588986218_554706301897477";

        //Colony Ads >> Please add the code ad in the Here 
        //*********************************************************  
        public static bool ShowColonyBannerAds = true; 
        public static bool ShowColonyInterstitialAds = true; 
        public static bool ShowColonyRewardAds = true; 

        public static string AdsColonyAppId = "app72922799d6714ded84"; 
        public static string AdsColonyBannerId = "vz294826d94e094cdf98"; 
        public static string AdsColonyInterstitialId = "vz3240d5ada18e4f78b3"; 
        public static string AdsColonyRewardedId = "vza09dafc6975146f3a7"; 

        //########################### 

        //Last_Messages Page >>
        ///********************************************************* 
        public static bool RunSoundControl = true;
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds
                  
        //Set Theme Tab
        //********************************************************* 
        public static bool SetTabDarkTheme = false; 

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = true;
        public static bool TurnSecurityProtocolType3072On = true;

        //Show custom error reporting page
        public static bool RenderPriorityFastPostLoad = true;

        //Trending 
        //*********************************************************
        public static bool ShowTrending = true; 
         
        public static bool ShowFilterBasic = true;
        public static bool ShowFilterLooks = true;
        public static bool ShowFilterBackground = true;
        public static bool ShowFilterLifestyle = true;
        public static bool ShowFilterMore = true;
          
        //*********************************************************

        //Premium system
        public static bool PremiumSystemEnabled = true;

        //Phone Validation system
        public static bool ValidationEnabled = true;
         
        public static bool CompressImage = false;
        public static int AvatarSize = 60;  
        public static int ImageSize = 200;

        public static bool ShowTextWithSpace = true; 

        /// <summary>
        /// if notv Enable Friend System ..
        /// you should comment this lines https://prnt.sc/1d2n56g on file notifcation_bar_tabs.xml
        /// you can find this file from  Resources/xml/notifcation_bar_tabs.xml
        /// </summary>
        public static bool EnableFriendSystem = true; 
         
        //Error Report Mode
        //*********************************************************
        public static bool SetApisReportMode = false; 
         
        public static bool ShowWalkTroutPage = true;

        public static bool EnableAppFree = false;

        //Payment System (ShowPaymentCardPage >> Paypal & Stripe ) (ShowLocalBankPage >> Local Bank ) 
        //*********************************************************

        public static PaymentsSystem PaymentsSystem = PaymentsSystem.All;
         
        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc & https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static bool ShowPaypal = true;
        public static string MerchantAccountId = "test"; //#New

        public static string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4c******"; //#New
        public static string ProductionTokenizationKey = "production_t2wns2y2_dfy45******"; //#New

        public static bool ShowCreditCard = true;
        public static bool ShowBankTransfer = true;
         
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static bool ShowInAppBilling = false;  
        //*********************************************************

        //Settings Page >>  
        //********************************************************* 
        public static bool ShowSettingsAccount = true;  
        public static bool ShowSettingsSocialLinks = true; 
        public static bool ShowSettingsPassword = true; 
        public static bool ShowSettingsBlockedUsers = true; 
        public static bool ShowSettingsDeleteAccount = true; 
        public static bool ShowSettingsTwoFactor = true; 
        public static bool ShowSettingsManageSessions = true;  
        public static bool ShowSettingsWithdrawals = true;  
        public static bool ShowSettingsMyAffiliates = true;  
        public static bool ShowSettingsTransactions = true; //#New
         
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="android.permission.READ_CONTACTS" />
        /// <uses-permission android:name="android.permission.READ_PHONE_NUMBERS" />
        /// </summary>
        public static bool InvitationSystem = true;

        /// <summary>
        /// On main full filter view screen, reset filter option will available only on the first page by default
        /// If you want to show the reset filter option for all the pages then set "ShowResetFilterForAllPages" as true
        /// </summary>
        public static bool ShowResetFilterForAllPages = false;

        /// <summary>
        /// If want to have limit on messages then set this variable as 'true'
        /// If you set the limit on messages then non pro user will able to send only 5 messages
        /// </summary>
        public static bool ShouldHaveLimitOnMessages = true; 
        public static int MaxMessageLimitForNonProUser = 5;
        //********************************************************* 

        public static bool ShowSettingsRateApp = true; 
        public static int ShowRateAppCount = 5; 

        public static bool ShowSettingsUpdateManagerApp = false; 
         
        public static bool OpenVideoFromApp = true; 
        public static bool OpenImageFromApp = true;

        /// <summary>
        /// true => Only over 18 years old
        /// false => all 
        /// </summary>
        public static bool IsUserYearsOld = true;
         
        //********************************************************* 
        public static bool ShowLive = true; //#New
        public static string AppIdAgoraLive = "4529799cec30453aa41337d69f1d7e52"; //#New


        //*********************************************************
    }
} 