using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;

using Android.Views;
using Android.Widget;
using AndroidHUD;
using QuickDate.Activities.Base;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace QuickDate.Activities.MyProfile
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ProfileInfoEditActivity : BaseActivity , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private RelativeLayout MainLayout;
        private TextView BackIcon, NameIcon, GenderIcon, BirthdayIcon, LocationIcon, LanguageIcon, RelationshipIcon, WorkStatusIcon, EducationIcon;
        private EditText EdtFirstName, EdtLastName, EdtGender, EdtBirthday, EdtLocation, EdtLanguage, EdtRelationship, EdtWorkStatus, EdtEducation; 
        private Button BtnSave;
        private string TypeDialog = "";
        private int IdGender, IdRelationShip, IdWorkStatus, IdEducation;
        private PublisherAdView PublisherAdView;
        private RewardedVideoAd RewardedVideoAd;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.ButtomSheetProfileInfoEdit);

                //Get Value And Set Toolbar
                InitComponent(); 

                GetMyInfoData();

                if (AppSettings.ShowFbRewardVideoAds)
                    RewardedVideoAd = AdsFacebook.InitRewardVideo(this);
                else
                    AdsColony.Ad_Rewarded(this);
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
                AddOrRemoveEvent(true);
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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

        protected override void OnDestroy()
        {
            try
            {
                PublisherAdView?.Destroy();
                RewardedVideoAd?.Destroy();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                MainLayout = FindViewById<RelativeLayout>(Resource.Id.mainLayout);
                MainLayout.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                BackIcon = FindViewById<TextView>(Resource.Id.IconBack);

                NameIcon = FindViewById<TextView>(Resource.Id.IconName);
                EdtFirstName = FindViewById<EditText>(Resource.Id.FirstNameEditText);
                EdtLastName = FindViewById<EditText>(Resource.Id.LastNameEditText);

                GenderIcon = FindViewById<TextView>(Resource.Id.IconGender);
                EdtGender = FindViewById<EditText>(Resource.Id.GenderEditText);
               
                BirthdayIcon = FindViewById<TextView>(Resource.Id.IconBirthday);
                EdtBirthday = FindViewById<EditText>(Resource.Id.BirthdayEditText);

                LocationIcon = FindViewById<TextView>(Resource.Id.IconLocation);
                EdtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);

                LanguageIcon = FindViewById<TextView>(Resource.Id.IconLanguage);
                EdtLanguage = FindViewById<EditText>(Resource.Id.LanguageEditText);

                RelationshipIcon = FindViewById<TextView>(Resource.Id.IconRelationship);
                EdtRelationship = FindViewById<EditText>(Resource.Id.RelationshipEditText);

                WorkStatusIcon = FindViewById<TextView>(Resource.Id.IconWorkStatus);
                EdtWorkStatus = FindViewById<EditText>(Resource.Id.WorkStatusEditText);

                EducationIcon = FindViewById<TextView>(Resource.Id.IconEducation);
                EdtEducation = FindViewById<EditText>(Resource.Id.EducationEditText);

                BtnSave = FindViewById<Button>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BackIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropright : IonIconsFonts.IosArrowDropleft); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, NameIcon, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, GenderIcon, FontAwesomeIcon.Transgender);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, BirthdayIcon, FontAwesomeIcon.BirthdayCake);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, LocationIcon, FontAwesomeIcon.MapMarkerAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, LanguageIcon, FontAwesomeIcon.Language);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, RelationshipIcon, FontAwesomeIcon.Heart);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, WorkStatusIcon, FontAwesomeIcon.Briefcase);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, EducationIcon, FontAwesomeIcon.GraduationCap);

                Methods.SetColorEditText(EdtFirstName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtLastName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtGender, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtBirthday, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtLanguage, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtRelationship, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtWorkStatus, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtEducation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
               
                Methods.SetFocusable(EdtGender);
                Methods.SetFocusable(EdtBirthday);
                Methods.SetFocusable(EdtLanguage);
                Methods.SetFocusable(EdtRelationship);
                Methods.SetFocusable(EdtWorkStatus);
                Methods.SetFocusable(EdtEducation);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    BackIcon.Click += BackIconOnClick;
                    BtnSave.Click += BtnSaveOnClick;
                    EdtGender.Touch += EdtGenderOnTouch;
                    EdtBirthday.Touch += EdtBirthdayOnClick;
                    EdtLocation.FocusChange += EdtLocationOnFocusChange;
                    EdtLanguage.Touch += EdtLanguageOnClick;
                    EdtRelationship.Touch += EdtRelationshipOnClick;
                    EdtWorkStatus.Touch += EdtWorkStatusOnClick;
                    EdtEducation.Touch += EdtEducationOnClick;
                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    BtnSave.Click -= BtnSaveOnClick;
                    EdtGender.Touch -= EdtGenderOnTouch; 
                    EdtBirthday.Touch -= EdtBirthdayOnClick;
                    EdtLocation.FocusChange -= EdtLocationOnFocusChange;
                    EdtLanguage.Touch -= EdtLanguageOnClick;
                    EdtRelationship.Touch -= EdtRelationshipOnClick;
                    EdtWorkStatus.Touch -= EdtWorkStatusOnClick;
                    EdtEducation.Touch -= EdtEducationOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region Events

        private void EdtGenderOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Gender";
                var genderArray = ListUtils.SettingsSiteList?.Gender;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (genderArray != null) arrayAdapter.AddRange(genderArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Gender)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Education
        private void EdtEducationOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Education";
                //string[] educationArray = Application.Context.Resources.GetStringArray(Resource.Array.EducationArray);
                var educationArray = ListUtils.SettingsSiteList?.Education;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (educationArray != null) arrayAdapter.AddRange(educationArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetString(Resource.String.Lbl_EducationLevel)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //WorkStatus
        private void EdtWorkStatusOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "WorkStatus";
                //string[] workStatusArray = Application.Context.Resources.GetStringArray(Resource.Array.WorkStatusArray);
                var workStatusArray = ListUtils.SettingsSiteList?.WorkStatus;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (workStatusArray != null) arrayAdapter.AddRange(workStatusArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_ChooseWorkStatus)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Open DatePicker And Get Short Date 
        private void EdtBirthdayOnClick(object sender, View.TouchEventArgs e)
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
                                EdtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Error_IsUserYearsOld), ToastLength.Short)?.Show();
                            }
                        }
                        else //All
                        {
                            EdtBirthday.Text = time.Date.ToString("dd-MM-yyyy");
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

        //RelationShip
        private void EdtRelationshipOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Relationship";
                //string[] relationshipArray = Application.Context.Resources.GetStringArray(Resource.Array.RelationShipArray);
                var relationshipArray = ListUtils.SettingsSiteList?.Relationship;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (relationshipArray != null) arrayAdapter.AddRange(relationshipArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_ChooseRelationshipStatus)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Language
        private void EdtLanguageOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Language";
                //string[] languageArray = Application.Context.Resources.GetStringArray(Resource.Array.LanguageArray); 
                var languageArray = ListUtils.SettingsSiteList?.Language;
                 
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (languageArray != null) arrayAdapter.AddRange(languageArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_ChooseLanguage)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Get Location
        private void EdtLocationOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int) Build.VERSION.SdkInt < 23)
                    {
                        // result Code 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                            new IntentController(this).OpenIntentLocation();
                        else
                            new PermissionsController(this).RequestPermission(105);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception); 
            }
        }

        //Click save data and sent api
        private async void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                { 
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {  
                        {"first_name", EdtFirstName.Text}, 
                        {"last_name", EdtLastName.Text},
                        {"gender", IdGender.ToString()},
                        {"birthday", EdtBirthday.Text},
                        {"location", EdtLocation.Text},
                        {"language", EdtLanguage.Text?.ToLower()},
                        {"relationship", IdRelationShip.ToString()},
                        {"work_status", IdWorkStatus.ToString()},
                        {"education", IdEducation.ToString()},
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
                                local.FirstName = EdtFirstName.Text;
                                local.LastName = EdtLastName.Text;
                                local.Gender = IdGender.ToString();
                                local.Birthday = EdtBirthday.Text;
                                local.Address = EdtLocation.Text;
                                local.Language = EdtLanguage.Text;
                                local.Relationship = IdRelationShip.ToString();
                                local.WorkStatus = IdWorkStatus.ToString();
                                local.Education = IdEducation.ToString();

                                SqLiteDatabase database = new SqLiteDatabase();
                                database.InsertOrUpdate_DataMyInfo(local);
                                 
                            }

                            Toast.MakeText(this, GetText(Resource.String.Lbl_SuccessfullyUpdated), ToastLength.Short)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            Intent resultIntent = new Intent();
                            SetResult(Result.Ok, resultIntent);
                            Finish();
                        } 
                    }
                    else  
                    {
                        //Methods.DisplayReportResult(this, respond);
                        if (respond is ErrorObject error)
                        {
                            AndHUD.Shared.ShowError(this, error.ErrorData.ErrorText, MaskType.Clear, TimeSpan.FromSeconds(2)); 
                        }
                    } 
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)?.Show();
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
        
        //Back
        private void BackIconOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent resultIntent = new Intent();
                SetResult(Result.Canceled, resultIntent);
                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void GetMyInfoData()
        {
            try
            {
                if (ListUtils.MyUserInfo.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                    
                }

                var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                if (dataUser != null)
                { 
                    EdtFirstName.Text = dataUser.FirstName;
                    EdtLastName.Text = dataUser.LastName;

                    EdtGender.Text = ListUtils.SettingsSiteList?.Gender?.FirstOrDefault(a => a.ContainsKey(dataUser.Gender))?.Values.FirstOrDefault() ?? dataUser.GenderTxt;
                    IdGender = Convert.ToInt32(dataUser.Gender);

                    EdtBirthday.Text = dataUser.Birthday;

                    if (Methods.FunString.StringNullRemover(dataUser.Location) != "-----")
                    {
                        EdtLocation.Text = dataUser.Location;
                    }

                    if (Methods.FunString.StringNullRemover(dataUser.Language) != "-----")
                    {
                        EdtLanguage.Text = dataUser.Language;
                    }

                    string relationship = QuickDateTools.GetRelationship(dataUser.Relationship);
                    if (Methods.FunString.StringNullRemover(relationship) != "-----")
                    {
                        EdtRelationship.Text = relationship;
                        IdRelationShip = Convert.ToInt32(dataUser.Relationship);
                    }

                    string work = QuickDateTools.GetWorkStatus(dataUser.WorkStatus);
                    if (Methods.FunString.StringNullRemover(work) != "-----")
                    {
                        EdtWorkStatus.Text = work;
                        IdWorkStatus = Convert.ToInt32(dataUser.WorkStatus);
                    }

                    string education = QuickDateTools.GetEducation(dataUser.Education);
                    if (Methods.FunString.StringNullRemover(education) != "-----")
                    {
                        EdtEducation.Text = education;
                        IdEducation = Convert.ToInt32(dataUser.Education);
                    } 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 502 && resultCode == Result.Ok) // Location
                {
                    var placeAddress = data.GetStringExtra("Address") ?? "";
                    var placeLatLng = data.GetStringExtra("latLng") ?? "";
                    if (!string.IsNullOrEmpty(placeAddress))
                        EdtLocation.Text = placeAddress; 
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
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Language":
                        EdtLanguage.Text = itemString;
                        break;
                    case "Relationship":
                    {
                        var relationshipArray = ListUtils.SettingsSiteList?.Relationship?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdRelationShip = int.Parse(relationshipArray ?? "1");
                        EdtRelationship.Text = itemString;
                        break;
                    }
                    case "WorkStatus":
                    {
                        var workStatusArray = ListUtils.SettingsSiteList?.WorkStatus?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdWorkStatus = int.Parse(workStatusArray ?? "1");
                        EdtWorkStatus.Text = itemString;
                        break;
                    }
                    case "Education":
                    {
                        var educationArray = ListUtils.SettingsSiteList?.Education?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdEducation = int.Parse(educationArray ?? "1");
                        EdtEducation.Text = itemString;
                        break;
                    }
                    case "Gender":
                    {
                        var genderArray = ListUtils.SettingsSiteList?.Gender?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdGender = int.Parse(genderArray ?? UserDetails.FilterOptionGenderMale);
                        EdtGender.Text = itemString;
                        break;
                    }
                }
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