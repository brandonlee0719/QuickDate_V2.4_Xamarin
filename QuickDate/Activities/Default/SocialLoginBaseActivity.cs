using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api.Credentials;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Org.Json;
using QuickDate.Activities.Base;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.SocialLogins;
using QuickDate.Helpers.Utils;
using QuickDate.Library.OneSignalNotif;
using QuickDate.SQLite;
using QuickDateClient;
using QuickDateClient.Classes.Authorization;
using QuickDateClient.Classes.Global;
using QuickDateClient.Requests;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using Object = Java.Lang.Object;
using Task = System.Threading.Tasks.Task;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Default
{
    public abstract class SocialLoginBaseActivity : BaseActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback, IOnCompleteListener, IOnFailureListener
    {
        #region Variables Basic

        private Button WoWonderSignInButton;
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker MProfileTracker;
        private LoginButton FbLoginButton;
        private SignInButton GoogleSignInButton;
        public static GoogleSignInClient MGoogleSignInClient;
        public static SocialLoginBaseActivity Instance;
        private Toolbar Toolbar;

        #endregion

        #region General
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                InitializeQuickDate.Initialize(AppSettings.TripleDesAppServiceProvider, PackageName);

                //Set Full screen 
                Methods.App.FullScreenApp(this , true);

                Instance = this;

                if (string.IsNullOrEmpty(UserDetails.DeviceId))
                    OneSignalNotification.Instance.RegisterNotificationDevice();
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

         
        #endregion

        #region Events
        private void MProfileTrackerOnMOnProfileChanged(object sender, OnProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                {
                    //FbFirstName = e.MProfile.FirstName;
                    //FbLastName = e.MProfile.LastName;
                    //FbName = e.MProfile.Name;
                    //FbProfileId = e.MProfile.Id;

                    var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                    var parameters = new Bundle();
                    parameters.PutString("fields", "id,name,age_range,email");
                    request.Parameters = parameters;
                    request.ExecuteAsync();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }
        #endregion

        #region Functions
        public void InitToolbar()
        {
            try
            {
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    //Toolbar.Title = GetString(Resource.String.Lbl_Register);
                    Toolbar.Title = " ";
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    MProfileTracker = new FbMyProfileTracker();
                    MProfileTracker.StartTracking();
                    MProfileTracker.MOnProfileChanged += MProfileTrackerOnMOnProfileChanged;


                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    FbLoginButton.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hash = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hash);
                }
                else
                {
                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient = GoogleSignIn.GetClient(this, gso);

                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Click += GoogleSignInButtonOnClick;
                }
                else
                {
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
                }

                //#WoWonder 
                if (AppSettings.ShowWoWonderLogin)
                {
                    WoWonderSignInButton = FindViewById<Button>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Click += WoWonderSignInButtonOnClick;

                    WoWonderSignInButton.Text = GetString(Resource.String.Lbl_LoginWith) + " " + AppSettings.AppNameWoWonder;
                    WoWonderSignInButton.Visibility = ViewStates.Visible;
                }
                else
                {
                    WoWonderSignInButton = FindViewById<Button>(Resource.Id.WoWonderLogin_button);
                    WoWonderSignInButton.Visibility = ViewStates.Gone;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void SetDataLogin(LoginObject auth)
        {
            try
            {
                UserDetails.Username = auth.Data.UserInfo.Email;
                UserDetails.FullName = auth.Data.UserInfo.FullName;
                UserDetails.Password = auth.Data.UserInfo.Password;
                UserDetails.AccessToken = auth.Data.AccessToken;
                UserDetails.UserId = auth.Data.UserId;
                UserDetails.Status = "Pending";
                UserDetails.Cookie = auth.Data.AccessToken;
                UserDetails.Email = auth.Data.UserInfo.Email;

                Current.AccessToken = auth.Data.AccessToken;

                //Insert user data to database
                var user = new DataTables.LoginTb
                {
                    UserId = UserDetails.UserId.ToString(),
                    AccessToken = UserDetails.AccessToken,
                    Cookie = UserDetails.Cookie,
                    Username = auth.Data.UserInfo.Email,
                    Password = auth.Data.UserInfo.Password,
                    Status = "Pending",
                    Lang = "",
                    DeviceId = UserDetails.DeviceId,
                };
                ListUtils.DataUserLoginList.Add(user);

                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateLogin_Credentials(user);

                if (auth.Data.UserInfo != null)
                {
                    dbDatabase.InsertOrUpdate_DataMyInfo(auth.Data.UserInfo);

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetInfoData(this, UserDetails.UserId.ToString()) });
                }

                
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Abstract members
        public abstract void ToggleVisibility(bool isLoginProgress);
        #endregion

        #region Social Logins

        private string FbAccessToken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ToggleVisibility(false);

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ToggleVisibility(false);

                // Handle e
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                //var loginResult = result as LoginResult;
                //var id = AccessToken.CurrentAccessToken.UserId;

                ToggleVisibility(true);

                SetResult(Result.Ok);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                var data = json.ToString();
                var result = JsonConvert.DeserializeObject<FacebookResult>(data);

                ToggleVisibility(true);

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(FbAccessToken, "facebook", UserDetails.DeviceId);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            if (respond is LoginObject auth)
                            {
                                if (AppSettings.EnableSmartLockForPasswords)
                                {
                                    // Save Google Sign In to SmartLock
                                    Credential credential = new Credential.Builder(result.Email)
                                        .SetAccountType(IdentityProviders.Facebook)
                                        .SetName(result.Name)
                                        //.SetPassword(auth.AccessToken)
                                        .Build();
                                   
                                    SaveCredential(credential);
                                }
                                      
                                SetDataLogin(auth);

                                StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));
                                Finish();
                            }

                            break;
                        }
                        case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                string errorText = error.Message;
                                long errorId = error.Code;
                                switch (errorId)
                                {
                                    case 1:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_1), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 2:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 3:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 4:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_4), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 5:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_5), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ToggleVisibility(false);
                            break;
                        }
                        case 404:
                            ToggleVisibility(false);
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), e.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //======================================================

        #region Google

        //Event Click login using google
        private void GoogleSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (MGoogleSignInClient == null)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    MGoogleSignInClient ??= GoogleSignIn.GetClient(this, gso);
                }

                var signInIntent = MGoogleSignInClient.SignInIntent;
                StartActivityForResult(signInIntent, 0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
       
        private async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ToggleVisibility(true);

                    //var GAccountName = acct.Account.Name;
                    //var GAccountType = acct.Account.Type;
                    //var GDisplayName = acct.DisplayName;
                    //var GFirstName = acct.GivenName;
                    //var GLastName = acct.FamilyName;
                    //var GProfileId = acct.Id;
                    //var GEmail = acct.Email;
                    //var GImg = acct.PhotoUrl.Path;
                    GAccessToken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;
                    Console.WriteLine(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(GAccessToken, "google", UserDetails.DeviceId);
                        switch (apiStatus)
                        {
                            case 200:
                            {
                                if (respond is LoginObject auth)
                                {
                                    if (auth.Data != null)
                                    {
                                        if (AppSettings.EnableSmartLockForPasswords)
                                        { 
                                            // Save Google Sign In to SmartLock
                                            Credential credential = new Credential.Builder(acct.Email)
                                                .SetAccountType(IdentityProviders.Google)
                                                .SetName(acct.DisplayName)
                                                .SetProfilePictureUri(acct.PhotoUrl)
                                                .Build();

                                            SaveCredential(credential);
                                        }

                                        SetDataLogin(auth);

                                        StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));

                                        ToggleVisibility(false);
                                        Finish();
                                    }
                                }

                                break;
                            }
                            case 400:
                            {
                                if (respond is ErrorObject error)
                                {
                                    ToggleVisibility(false);
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.ErrorData.ErrorText, GetText(Resource.String.Lbl_Ok));
                                }

                                break;
                            }
                            default:
                                ToggleVisibility(false);
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                break;
                        }
                    }
                    else
                    {
                        ToggleVisibility(false);
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
            }
            catch (Exception exception)
            {
                ToggleVisibility(false);
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        #endregion

        #region WoWonder

        //Event Click login using WoWonder
        private void WoWonderSignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(WoWonderLoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public async void LoginWoWonder(string woWonderAccessToken)
        {
            try
            {
                ToggleVisibility(true);

                if (!string.IsNullOrEmpty(woWonderAccessToken))
                {
                    //Login Api 
                    var (apiStatus, respond) = await RequestsAsync.Auth.SocialLoginAsync(woWonderAccessToken, "wowonder", UserDetails.DeviceId);
                    switch (apiStatus)
                    {
                        case 200:
                        {
                            if (respond is LoginObject auth)
                            {
                                SetDataLogin(auth);

                                StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));
                                FinishAffinity();
                            }

                            break;
                        }
                        case 400:
                        {
                            if (respond is ErrorObject error)
                            {
                                string errorText = error.Message;
                                long errorId = error.Code;
                                switch (errorId)
                                {
                                    case 1:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_1), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 2:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_2), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 3:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_3), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 4:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_4), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    case 5:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_5), GetText(Resource.String.Lbl_Ok));
                                        break;
                                    default:
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }

                            ToggleVisibility(false);
                            break;
                        }
                        case 404:
                            ToggleVisibility(false);
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #endregion

        #region Permissions && Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 0)
                {
                    var task = await GoogleSignIn.GetSignedInAccountFromIntentAsync(data);
                    SetContentGoogle(task);
                }
                else if (requestCode == RcCredentialsRead)
                {
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else if (requestCode == RcCredentialsSave)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        //Saved
                    }
                    else
                    {
                        //Credential save failed
                    }
                }
                else if (requestCode == RcCredentialsHint)
                {
                    MIsResolving = false;
                    if (resultCode == Result.Ok)
                    {
                        var extra = data.GetParcelableExtra(Credential.ExtraKey);
                        if (extra != null && extra is Credential credential)
                        {
                            OnlyPasswords = true;
                            HandleCredential(credential, OnlyPasswords);
                        }
                    }
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Cross App Authentication

        private static readonly int RcCredentialsSave = 1;
        private static readonly int RcCredentialsRead = 2;
        private static readonly int RcCredentialsHint = 3;

        private bool OnlyPasswords;
        public bool MIsResolving;
        private Credential MCredential;
        private string CredentialType;

        private CredentialsClient MCredentialsClient;
        private GoogleSignInClient MSignInClient;

        public void BuildClients(string accountName)
        {
            try
            {
                var gsoBuilder = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                    .RequestIdToken(AppSettings.ClientId)
                    .RequestScopes(new Scope(Scopes.Profile))
                    .RequestScopes(new Scope(Scopes.PlusMe))
                    .RequestScopes(new Scope(Scopes.DriveAppfolder))
                    .RequestServerAuthCode(AppSettings.ClientId)
                    .RequestProfile().RequestEmail();

                if (accountName != null)
                    gsoBuilder.SetAccountName(accountName);

                CredentialsOptions options = new CredentialsOptions.Builder()
                    .ForceEnableSaveDialog()
                    .Build();

                MCredentialsClient = Credentials.GetClient(this, options);
                MSignInClient = GoogleSignIn.GetClient(this, gsoBuilder.Build());
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void LoadHintClicked()
        {
            try
            {
                HintRequest hintRequest = new HintRequest.Builder()
                    .SetHintPickerConfig(new CredentialPickerConfig.Builder()
                        .SetShowCancelButton(true)
                        .Build())
                    .SetIdTokenRequested(false)
                    .SetEmailAddressIdentifierSupported(true)
                    .SetAccountTypes(IdentityProviders.Google)
                    .Build();

                PendingIntent intent = MCredentialsClient.GetHintPickerIntent(hintRequest);
                StartIntentSenderForResult(intent.IntentSender, RcCredentialsHint, null, 0, 0, 0);
                MIsResolving = true;
            }
            catch (Exception e)
            {
                //Could not start hint picker Intent
                MIsResolving = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RequestCredentials(bool onlyPasswords)
        {
            try
            {
                OnlyPasswords = onlyPasswords;

                CredentialRequest.Builder crBuilder = new CredentialRequest.Builder()
                    .SetPasswordLoginSupported(true);

                if (!onlyPasswords)
                {
                    crBuilder.SetAccountTypes(IdentityProviders.Google);
                }

                CredentialType = "Request";

                MCredentialsClient.Request(crBuilder.Build()).AddOnCompleteListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void HandleCredential(Credential credential, bool onlyPasswords)
        {
            try
            {
                // See "Handle successful credential requests"  
                MCredential = credential;

                //Log.d(TAG, "handleCredential:" + credential.getAccountType() + ":" + credential.getId());
                if (IdentityProviders.Google.Equals(credential.AccountType))
                {
                    // Google account, rebuild GoogleApiClient to set account name and then try
                    BuildClients(credential.Id);
                    GoogleSilentSignIn();
                }
                else if (!string.IsNullOrEmpty(credential?.Id) && !string.IsNullOrEmpty(credential?.Password))
                {
                    // Email/password account
                    Console.WriteLine("Signed in as {0}", credential.Id);

                    //ContinueButton.Text = GetString(Resource.String.Lbl_ContinueAs) + " " + credential.Id;
                    //ContinueButton.Visibility = ViewStates.Visible;

                    if (onlyPasswords)
                    {
                        //send api auth  
                        ToggleVisibility(true);

                        await AuthApi(credential.Id, credential.Password);
                    }
                }
                else
                {
                    //ContinueButton.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private async Task AuthApi(string email, string password)
        {
            var (apiStatus, respond) = await RequestsAsync.Auth.LoginAsync(email, password, UserDetails.DeviceId);
            switch (apiStatus)
            {
                case 200 when respond is LoginObject auth:

                    // Save Google Sign In to SmartLock
                    Credential credential = new Credential.Builder(email)
                        .SetName(email)
                        .SetPassword(password)
                        .Build();
                     
                    SaveCredential(credential);
                     
                    SetDataLogin(auth);
                    StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));
                    Finish();
                    break;
                case 200:
                    {
                        if (respond is LoginTwoFactorObject auth2)
                        {
                            UserDetails.UserId = Convert.ToInt32(auth2.UserId);
                            Intent intent = new Intent(this, typeof(VerificationAccountActivity));
                            intent.PutExtra("Type", "TwoFactor");
                            StartActivity(intent);
                            Finish();
                        }

                        break;
                    }
                case 400:
                    {
                        if (respond is ErrorObject error)
                        {
                            string errorText = error.Message;
                            long errorId = error.Code;
                            switch (errorId)
                            {
                                case 1:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_1), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case 2:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_2), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case 3:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_3), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case 4:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_4), GetText(Resource.String.Lbl_Ok));
                                    break;
                                case 5:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_5), GetText(Resource.String.Lbl_Ok));
                                    break;
                                default:
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                    break;
                            }
                        }

                        ToggleVisibility(false);
                        break;
                    }
                case 404:
                    ToggleVisibility(false);
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                    break;
            }
        }
         
        private void ResolveResult(ResolvableApiException rae, int requestCode)
        {
            try
            {
                if (!MIsResolving)
                {
                    try
                    {
                        rae.StartResolutionForResult(this, requestCode);
                        MIsResolving = true;
                    }
                    catch (IntentSender.SendIntentException e)
                    {
                        MIsResolving = false;
                        //Failed to send Credentials intent
                        Methods.DisplayReportResultTrack(e);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SaveCredential(Credential credential)
        {
            try
            {
                if (credential == null)
                {
                    //Log.w(TAG, "Ignoring null credential.");
                    return;
                }

                CredentialType = "Save";
                MCredentialsClient.Save(credential).AddOnCompleteListener(this, this).AddOnFailureListener(this, this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void OnGoogleRevokeClicked()
        {
            if (MCredential != null)
            {
                await MCredentialsClient.DeleteAsync(MCredential);
            }
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            try
            {
                if (CredentialType == "Request")
                {
                    if (task.IsSuccessful && task.Result is CredentialRequestResponse credential)
                    {
                        // Auto sign-in success
                        HandleCredential(credential.Credential, OnlyPasswords);
                        return;
                    }
                }
                else if (CredentialType == "Save")
                {
                    if (task.IsSuccessful)
                    {
                        return;
                    }
                }

                var ee = task.Exception;
                if (ee is ResolvableApiException rae)
                {
                    // Getting credential needs to show some UI, start resolution 
                    if (CredentialType == "Request")
                        ResolveResult(rae, RcCredentialsRead);

                    else if (CredentialType == "Save")
                        ResolveResult(rae, RcCredentialsSave);
                }
                else
                {
                    Console.WriteLine("request: not handling exception {0}", ee);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnFailure(Java.Lang.Exception e)
        {

        }

        private async void GoogleSilentSignIn()
        {
            try
            {
                // Try silent sign-in with Google Sign In API
                GoogleSignInAccount silentSignIn = await MSignInClient.SilentSignInAsync();
                if (silentSignIn != null)
                {
                    SetContentGoogle(silentSignIn);
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