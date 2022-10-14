using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient;
using QuickDateClient.Classes.Authorization;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Exception = System.Exception;

namespace QuickDate.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RegisterActivity : SocialLoginBaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private EditText FirstNameEditText, LastNameEditText, EmailEditText, UsernameEditText, PasswordEditText, ConfirmPasswordEditText, GenderEditText, BirthdayEditText;
        private Button RegisterButton;
        private LinearLayout TermsLayout, SignLayout;
        private ProgressBar ProgressBar;
        private int IdGender = -1;
        private ViewGroup SocialLoginContainerLayout;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.RegisterLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                if (AppSettings.ShowSocialLoginAtRegisterScreen)
                {
                    SocialLoginContainerLayout.Visibility = ViewStates.Visible;
                    InitSocialLogins();
                } 
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void ToggleVisibility(bool isLoginProgress)
        {
            ProgressBar.Visibility = isLoginProgress ? ViewStates.Visible : ViewStates.Gone;
            RegisterButton.Visibility = isLoginProgress ? ViewStates.Gone : ViewStates.Visible;
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
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
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                FirstNameEditText = FindViewById<EditText>(Resource.Id.edt_firstname);
                LastNameEditText = FindViewById<EditText>(Resource.Id.edt_lastname);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                UsernameEditText = FindViewById<EditText>(Resource.Id.edt_username);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                ConfirmPasswordEditText = FindViewById<EditText>(Resource.Id.edt_Confirmpassword);
                GenderEditText = FindViewById<EditText>(Resource.Id.edt_gender);
                BirthdayEditText = FindViewById<EditText>(Resource.Id.edt_Birthday);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                RegisterButton = FindViewById<Button>(Resource.Id.SignInButton);
                TermsLayout = FindViewById<LinearLayout>(Resource.Id.termsLayout);
                SignLayout = FindViewById<LinearLayout>(Resource.Id.SignLayout);
                SocialLoginContainerLayout = FindViewById<LinearLayout>(Resource.Id.SocialLoginContainerLayout);

                Methods.SetFocusable(GenderEditText);
                Methods.SetFocusable(BirthdayEditText);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    RegisterButton.Click += RegisterButtonOnClick;
                    TermsLayout.Click += TermsLayoutOnClick;
                    SignLayout.Click += SignLayoutOnClick;
                    GenderEditText.Touch += GenderEditTextOnTouch;
                    BirthdayEditText.Touch += BirthdayEditTextOnTouch;
                }
                else
                {
                    RegisterButton.Click -= RegisterButtonOnClick;
                    TermsLayout.Click -= TermsLayoutOnClick;
                    SignLayout.Click -= SignLayoutOnClick;
                    GenderEditText.Touch -= GenderEditTextOnTouch;
                    BirthdayEditText.Touch -= BirthdayEditTextOnTouch;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Open Login Activity
        private void SignLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(LoginActivity)));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Terms of Service
        private void TermsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var url = InitializeQuickDate.WebsiteUrl + "/terms-of-use";
                new IntentController(this).OpenBrowserFromApp(url); 
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        //Register QuickDate
        private async void RegisterButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (IdGender == -1)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        RegisterButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                        return;
                    }

                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(UsernameEditText.Text.Replace(" ", "")) ||
                        !string.IsNullOrEmpty(FirstNameEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(LastNameEditText.Text.Replace(" ", "")) ||
                        !string.IsNullOrEmpty(PasswordEditText.Text) || !string.IsNullOrEmpty(ConfirmPasswordEditText.Text) || !string.IsNullOrEmpty(BirthdayEditText.Text))
                    {
                        var check = Methods.FunString.IsEmailValid(EmailEditText.Text);
                        if (!check)
                        {
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_VerificationFailed), GetText(Resource.String.Lbl_IsEmailValid), GetText(Resource.String.Lbl_Ok));
                        }
                        else
                        {
                            if (PasswordEditText.Text != ConfirmPasswordEditText.Text)
                            {
                                ProgressBar.Visibility = ViewStates.Gone;
                                RegisterButton.Visibility = ViewStates.Visible;
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Register_password), GetText(Resource.String.Lbl_Ok));
                            }
                            else
                            {
                                ProgressBar.Visibility = ViewStates.Visible;
                                RegisterButton.Visibility = ViewStates.Gone;

                                var (apiStatus, respond) = await RequestsAsync.Auth.RegisterAsync(FirstNameEditText.Text.Replace(" ", ""), LastNameEditText.Text.Replace(" ", ""), UsernameEditText.Text.Replace(" ", ""), EmailEditText.Text.Replace(" ", ""), PasswordEditText.Text, UserDetails.DeviceId);
                                switch (apiStatus)
                                {
                                    case 200:
                                    {
                                        if (respond is RegisterObject auth)
                                        {
                                            switch (auth.SuccessType)
                                            {
                                                case "registered":
                                                    SetDataRegister(auth);

                                                    StartActivity(AppSettings.ShowWalkTroutPage ? new Intent(this, typeof(BoardingActivity)) : new Intent(this, typeof(HomeActivity)));
                                                    FinishAffinity();
                                                    break;
                                                case "confirm_account":
                                                {
                                                    //Insert user data to database
                                                    var user = new DataTables.LoginTb
                                                    {
                                                        UserId = UserDetails.UserId.ToString(),
                                                        AccessToken = UserDetails.AccessToken,
                                                        Cookie = UserDetails.Cookie,
                                                        Username = EmailEditText.Text,
                                                        Password = PasswordEditText.Text,
                                                        Status = "Pending",
                                                        Lang = "",
                                                        DeviceId = UserDetails.DeviceId,
                                                    };
                                                    ListUtils.DataUserLoginList.Add(user);

                                                    var dbDatabase = new SqLiteDatabase();
                                                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                            

                                                    Intent intent = new Intent(this, typeof(VerificationAccountActivity));
                                                    intent.PutExtra("Email", EmailEditText.Text);
                                                    intent.PutExtra("Type", "acc");
                                                    StartActivity(intent);
                                                    Finish();
                                                    break;
                                                }
                                            }
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
                                                case 6:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_6), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 7:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_7), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 8:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_8), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 9:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_9), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 10:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_10), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 11:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_11), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 12:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_12), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 13:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_13), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 14:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_14), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 15:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_15), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 16:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_16), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 17:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_17), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                case 18:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetString(Resource.String.Lbl_Error_18), GetText(Resource.String.Lbl_Ok));
                                                    break;
                                                default:
                                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                                                    break;
                                            }
                                        }

                                        ProgressBar.Visibility = ViewStates.Gone;
                                        RegisterButton.Visibility = ViewStates.Visible;
                                        break;
                                    }
                                    case 404:
                                        ProgressBar.Visibility = ViewStates.Gone;
                                        RegisterButton.Visibility = ViewStates.Visible;
                                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), respond.ToString(), GetText(Resource.String.Lbl_Ok));
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        RegisterButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    RegisterButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                ProgressBar.Visibility = ViewStates.Gone;
                RegisterButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), ex.Message, GetText(Resource.String.Lbl_Ok));

            }
        }

        private async void SetDataRegister(RegisterObject auth)
        {
            try
            {
                if (auth.Data != null)
                {
                    UserDetails.Username = EmailEditText.Text;
                    UserDetails.FullName = EmailEditText.Text;
                    UserDetails.Password = PasswordEditText.Text;
                    UserDetails.AccessToken = auth.Data.AccessToken;
                    UserDetails.UserId = auth.Data.UserId;
                    UserDetails.Status = "Pending";
                    UserDetails.Cookie = auth.Data.AccessToken;
                    UserDetails.Email = EmailEditText.Text;

                    Current.AccessToken = auth.Data.AccessToken;

                    //Insert user data to database
                    var user = new DataTables.LoginTb
                    {
                        UserId = UserDetails.UserId.ToString(),
                        AccessToken = UserDetails.AccessToken,
                        Cookie = UserDetails.Cookie,
                        Username = EmailEditText.Text,
                        Password = PasswordEditText.Text,
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

                    if (IdGender != -1 && !string.IsNullOrEmpty(BirthdayEditText.Text))
                    {
                        if (Methods.CheckConnectivity())
                        {
                            var dictionary = new Dictionary<string, string>
                            {
                                {"gender", IdGender.ToString()},
                                {"birthday", BirthdayEditText.Text},
                            };

                            var (apiStatus, respond) = await RequestsAsync.Users.UpdateProfileAsync(dictionary);
                            if (apiStatus == 200)
                            {
                                if (respond is UpdateProfileObject result)
                                {
                                   Console.WriteLine(result.Message);
                                    var local = ListUtils.MyUserInfo?.FirstOrDefault();
                                    if (local != null)
                                    {
                                        local.Gender = IdGender.ToString();

                                        dbDatabase.InsertOrUpdate_DataMyInfo(local);
                                    }
                                }
                            }
                            else Methods.DisplayReportResult(this, respond);
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                        }
                    }

                    
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GenderEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var genderArray = ListUtils.SettingsSiteList?.Gender;

                if (genderArray?.Count > 0)
                {
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                    var arrayAdapter = genderArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_Gender)).TitleColorRes(Resource.Color.primary);
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "List Gender Not Found, Please check api option ");
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
        
        private void BirthdayEditTextOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                var frag = DatePickerFragment.NewInstance(delegate (DateTime time)
                {
                    try
                    {
                        if (AppSettings.IsUserYearsOld) // 18
                        {
                            if (!Methods.Time.HasAgeRequirement(time.Date)) // over 18 years
                            {
                                BirthdayEditText.Text = time.Date.ToString("dd-MM-yyyy");
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Error_IsUserYearsOld), ToastLength.Short)?.Show();  
                            }
                        }
                        else //All
                        {
                            BirthdayEditText.Text = time.Date.ToString("dd-MM-yyyy");
                        }
                    }
                    catch (Exception exception)
                    {
                        Methods.DisplayReportResultTrack(exception);
                    }
                });
                frag.Show(SupportFragmentManager, DatePickerFragment.Tag);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                var genderArray = ListUtils.SettingsSiteList?.Gender?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                IdGender = int.Parse(genderArray ?? UserDetails.FilterOptionGenderMale);
                GenderEditText.Text = itemString;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
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