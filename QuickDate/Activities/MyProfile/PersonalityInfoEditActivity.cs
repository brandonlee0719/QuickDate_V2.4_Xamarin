using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Adcolony.Sdk;
using QuickDate.Activities.Base;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Global;
using QuickDateClient.Classes.Users;
using QuickDateClient.Requests;
using Xamarin.Facebook.Ads;
using AdView = Xamarin.Facebook.Ads.AdView;
using Exception = System.Exception;

namespace QuickDate.Activities.MyProfile
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PersonalityInfoEditActivity : BaseActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private RelativeLayout MainLayout;
        private TextView BackIcon, CharacterIcon, ChildrenIcon, FriendsIcon, PetsIcon;
        private EditText EdtCharacter, EdtChildren, EdtFriends, EdtPets;
        private Button BtnSave;
        private string TypeDialog;
        private int IdCharacter, IdChildren, IdFriends, IdPets;
        private AdView BannerAd;
        private InterstitialAd InterstitialAd;

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
                SetContentView(Resource.Layout.ButtomSheetPersonalityInfoEdit);

                //Get Value And Set Toolbar
                InitComponent(); 

                GetMyInfoData();

                if (AppSettings.ShowFbInterstitialAds)
                    InterstitialAd = AdsFacebook.InitInterstitial(this);
                else
                    AdsColony.Ad_Interstitial(this);
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
                BannerAd?.Destroy();
                InterstitialAd?.Destroy();
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

                CharacterIcon = FindViewById<TextView>(Resource.Id.IconCharacter);
                EdtCharacter = FindViewById<EditText>(Resource.Id.CharacterEditText);

                ChildrenIcon = FindViewById<TextView>(Resource.Id.IconChildren);
                EdtChildren = FindViewById<EditText>(Resource.Id.ChildrenEditText);

                FriendsIcon = FindViewById<TextView>(Resource.Id.IconFriends);
                EdtFriends = FindViewById<EditText>(Resource.Id.FriendsEditText);

                PetsIcon = FindViewById<TextView>(Resource.Id.IconPets);
                EdtPets = FindViewById<EditText>(Resource.Id.PetsEditText);
                 
                BtnSave = FindViewById<Button>(Resource.Id.ApplyButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BackIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.IosArrowDropright : IonIconsFonts.IosArrowDropleft);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, CharacterIcon, FontAwesomeIcon.YinYang);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, ChildrenIcon, FontAwesomeIcon.Baby);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, FriendsIcon, FontAwesomeIcon.UserFriends);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, PetsIcon, FontAwesomeIcon.Cat);

                Methods.SetColorEditText(EdtCharacter, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtChildren, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtFriends, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtPets, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
               
                Methods.SetFocusable(EdtCharacter);
                Methods.SetFocusable(EdtChildren);
                Methods.SetFocusable(EdtFriends);
                Methods.SetFocusable(EdtPets);

                LinearLayout adContainer = FindViewById<LinearLayout>(Resource.Id.bannerContainer);
                if (AppSettings.ShowFbBannerAds)
                    BannerAd = AdsFacebook.InitAdView(this, adContainer, null);
                else
                    AdsColony.InitBannerAd(this, adContainer, AdColonyAdSize.Banner, null);
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
                    EdtCharacter.Touch += EdtCharacterOnClick;
                    EdtChildren.Touch += EdtChildrenOnClick;
                    EdtFriends.Touch += EdtFriendsOnClick;
                    EdtPets.Touch += EdtPetsOnClick;
                }
                else
                {
                    BackIcon.Click -= BackIconOnClick;
                    BtnSave.Click -= BtnSaveOnClick;
                    EdtCharacter.Touch -= EdtCharacterOnClick;
                    EdtChildren.Touch -= EdtChildrenOnClick;
                    EdtFriends.Touch -= EdtFriendsOnClick;
                    EdtPets.Touch -= EdtPetsOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Pets
        private void EdtPetsOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Pets";
                //string[] petsArray = Application.Context.Resources.GetStringArray(Resource.Array.PetsArray);
                var petsArray = ListUtils.SettingsSiteList?.Pets;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (petsArray != null) arrayAdapter.AddRange(petsArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Pets)).TitleColorRes(Resource.Color.primary);
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

        //Friends
        private void EdtFriendsOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Friends";
                //string[] friendsArray = Application.Context.Resources.GetStringArray(Resource.Array.FriendsArray);
                var friendsArray = ListUtils.SettingsSiteList?.Friends;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (friendsArray != null) arrayAdapter.AddRange(friendsArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Friends)).TitleColorRes(Resource.Color.primary);
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

        //Children
        private void EdtChildrenOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Children";
                //string[] childrenArray = Application.Context.Resources.GetStringArray(Resource.Array.ChildrenArray);
                var childrenArray = ListUtils.SettingsSiteList?.Children;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (childrenArray != null) arrayAdapter.AddRange(childrenArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Children)).TitleColorRes(Resource.Color.primary);
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

        //Character
        private void EdtCharacterOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Character";
                //string[] characterArray = Application.Context.Resources.GetStringArray(Resource.Array.CharacterArray);
                var characterArray = ListUtils.SettingsSiteList?.Character;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (characterArray != null) arrayAdapter.AddRange(characterArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Character)).TitleColorRes(Resource.Color.primary);
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
                        {"character", IdCharacter.ToString()},
                        {"children", IdChildren.ToString()},
                        {"friends", IdFriends.ToString()},
                        {"pets",IdPets.ToString()},
                    };

                    var (apiStatus, respond) = await RequestsAsync.Users.UpdateProfileAsync(dictionary);
                    if (apiStatus == 200)
                    {
                        if (respond is UpdateProfileObject result)
                        {
                            var local = ListUtils.MyUserInfo?.FirstOrDefault();
                            if (local != null)
                            {
                                local.Character = IdCharacter.ToString();
                                local.Children = IdChildren.ToString(); 
                                local.Friends = IdFriends.ToString();
                                local.Pets = IdPets.ToString();

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
                        if (respond is ErrorObject error)
                        {
                            //Methods.DisplayReportResult(this, respond);
                            var errorText = error.ErrorData.ErrorText; 
                            AndHUD.Shared.ShowError(this, errorText, MaskType.Clear, TimeSpan.FromSeconds(2));
 
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
                    string character = QuickDateTools.GetCharacter(dataUser.Character);
                    if (Methods.FunString.StringNullRemover(character) != "-----")
                    {
                        IdCharacter = Convert.ToInt32(dataUser.Character);
                        EdtCharacter.Text = character;
                    }

                    string children = QuickDateTools.GetChildren(dataUser.Children);
                    if (Methods.FunString.StringNullRemover(children) != "-----")
                    {
                        IdChildren = Convert.ToInt32(dataUser.Children);
                        EdtChildren.Text = children;
                    }

                    string friends = QuickDateTools.GetFriends(dataUser.Friends);
                    if (Methods.FunString.StringNullRemover(friends) != "-----")
                    {
                        IdFriends = Convert.ToInt32(dataUser.Friends);
                        EdtFriends.Text = friends;
                    }

                    string pets = QuickDateTools.GetPets(dataUser.Pets);
                    if (Methods.FunString.StringNullRemover(pets) != "-----")
                    {
                        IdPets = Convert.ToInt32(dataUser.Pets);
                        EdtPets.Text = pets;
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Character":
                    {
                        var characterArray = ListUtils.SettingsSiteList?.Character?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdCharacter = int.Parse(characterArray ?? "1");
                        EdtCharacter.Text = itemString;
                        break;
                    }
                    case "Children":
                    {
                        var childrenArray = ListUtils.SettingsSiteList?.Children?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdChildren = int.Parse(childrenArray ?? "1");
                        EdtChildren.Text = itemString;
                        break;
                    }
                    case "Friends":
                    {
                        var friendsArray = ListUtils.SettingsSiteList?.Friends?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdFriends = int.Parse(friendsArray ?? "1");
                        EdtFriends.Text = itemString;
                        break;
                    }
                    case "Pets":
                    {
                        var petsArray = ListUtils.SettingsSiteList?.Pets?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdPets = int.Parse(petsArray ?? "1");
                        EdtPets.Text = itemString;
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