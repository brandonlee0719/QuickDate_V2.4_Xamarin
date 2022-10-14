using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Google.Android.Material.AppBar;
using QuickDate.Activities.Base;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Authorization;
using QuickDateClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.Premium
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PopularityActivity : BaseActivity
    {
        #region Variables Basic

       private CollapsingToolbarLayout CollapsingToolbar;
       private AppBarLayout AppBarLayout;
       private Toolbar ActionBarToolBar;
       private TextView TxtVisitsCost , TxtMatchesCost , TxtLikesCost;
       private Button BtnVisits, BtnMatches, BtnLikes;
        private HomeActivity GlobalContext;

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
                SetContentView(Resource.Layout.PopularityLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GetDataOption();

                GlobalContext = HomeActivity.GetInstance();
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

        #endregion
         
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                CollapsingToolbar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = GetText(Resource.String.Lbl_IncreasePopularity);

                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);
                 
                TxtVisitsCost = FindViewById<TextView>(Resource.Id.visitsCost);
                TxtMatchesCost = FindViewById<TextView>(Resource.Id.matchesCost);
                TxtLikesCost = FindViewById<TextView>(Resource.Id.likesCost);

                BtnVisits = FindViewById<Button>(Resource.Id.visitsButton);
                BtnMatches = FindViewById<Button>(Resource.Id.matchesButton);
                BtnLikes = FindViewById<Button>(Resource.Id.likesButton);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void InitToolbar()
        {
            try
            {
                ActionBarToolBar = (Toolbar)FindViewById(Resource.Id.maintoolbar);
                if (ActionBarToolBar != null)
                {
                    ActionBarToolBar.Title = GetText(Resource.String.Lbl_IncreasePopularity);
                    ActionBarToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(ActionBarToolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
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
                    BtnVisits.Click += BtnVisitsOnClick;
                    BtnMatches.Click += BtnMatchesOnClick;
                    BtnLikes.Click += BtnLikesOnClick;
                }
                else
                {
                    BtnVisits.Click -= BtnVisitsOnClick;
                    BtnMatches.Click -= BtnMatchesOnClick;
                    BtnLikes.Click -= BtnLikesOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Likes
        private async void BtnLikesOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!AppSettings.EnableAppFree)
                { 
                    var myBalance = ListUtils.MyUserInfo?.FirstOrDefault()?.Balance ?? "0.00";
                    if (!string.IsNullOrEmpty(myBalance) && myBalance != "0.00")
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                        //sent new api
                        var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("likes").ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            if (respond is AmountObject result)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        myBalance = result.CreditAmount.ToString();

                                        if (GlobalContext?.ProfileFragment.WalletNumber != null)
                                            GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                        BtnLikes.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);

                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        var window = new PopupController(this);
                        window.DisplayCreditWindow("credits");
                    }
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                    //sent new api
                    var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("likes").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is AmountObject result)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    //myBalance = result.CreditAmount.ToString();

                                    if (GlobalContext?.ProfileFragment.WalletNumber != null)
                                        GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                    BtnLikes.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    AndHUD.Shared.Dismiss(this);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Matches
        private async void BtnMatchesOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!AppSettings.EnableAppFree)
                {
                    var myBalance = ListUtils.MyUserInfo?.FirstOrDefault()?.Balance ?? "0.00";
                    if (!string.IsNullOrEmpty(myBalance) && myBalance != "0.00")
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //sent new api
                        var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("matches").ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            if (respond is AmountObject result)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        myBalance = result.CreditAmount.ToString();

                                        if (GlobalContext?.ProfileFragment.WalletNumber != null)
                                            GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                        BtnMatches.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);

                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        var window = new PopupController(this);
                        window.DisplayCreditWindow("credits");
                    }
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    //sent new api
                    var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("matches").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is AmountObject result)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    //myBalance = result.CreditAmount.ToString();

                                    if (GlobalContext?.ProfileFragment.WalletNumber != null)
                                        GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                    BtnMatches.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    AndHUD.Shared.Dismiss(this);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        //Visits
        private async void BtnVisitsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!AppSettings.EnableAppFree)
                {
                    var myBalance = ListUtils.MyUserInfo?.FirstOrDefault()?.Balance ?? "0.00";
                    if (!string.IsNullOrEmpty(myBalance) && myBalance != "0.00")
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //sent new api
                        var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("visits").ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            if (respond is AmountObject result)
                            {
                                RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        myBalance = result.CreditAmount.ToString();

                                        if (GlobalContext?.ProfileFragment != null)
                                            GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                        BtnVisits.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                    }
                                    catch (Exception exception)
                                    {
                                        Methods.DisplayReportResultTrack(exception);
                                    }
                                });
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);

                        AndHUD.Shared.Dismiss(this);
                    }
                    else
                    {
                        var window = new PopupController(this);
                        window.DisplayCreditWindow("credits");
                    }
                }
                else
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    //sent new api
                    var (apiStatus, respond) = await RequestsAsync.Users.ManagePopularityAsync("visits").ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is AmountObject result)
                        {
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    //myBalance = result.CreditAmount.ToString();

                                    if (GlobalContext?.ProfileFragment != null)
                                        GlobalContext.ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                                    BtnVisits.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    AndHUD.Shared.Dismiss(this);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
        
        #endregion

        private void GetDataOption()
        {
            try
            {
                var option = ListUtils.SettingsSiteList;
                if (option != null)
                {
                    TxtVisitsCost.Text = option.CostPerXvisits + " " + GetText(Resource.String.Lbl_Credits);
                    TxtMatchesCost.Text = option.CostPerXmatche + " " + GetText(Resource.String.Lbl_Credits);
                    TxtLikesCost.Text = option.CostPerXlike + " " + GetText(Resource.String.Lbl_Credits);
                }

                if (ListUtils.MyUserInfo.Count == 0)
                {
                    var sqlEntity = new SqLiteDatabase();
                    sqlEntity.GetDataMyInfo();
                    
                }

                var data = ListUtils.MyUserInfo?.FirstOrDefault();
                if (data != null)
                {
                    int xLikes = Convert.ToInt32(data.XlikesCreatedAt);
                    int xMatches = Convert.ToInt32(data.XmatchesCreatedAt);
                    int xVisits = Convert.ToInt32(data.XvisitsCreatedAt);
                    if (xLikes != 0)
                    {
                        BtnLikes.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                    }
                    else if (xMatches != 0)
                    {
                        BtnMatches.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                    }
                    else if (xVisits != 0)
                    {
                        BtnVisits.Text = GetText(Resource.String.Lbl_IncreaseStarted);
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

    }
}