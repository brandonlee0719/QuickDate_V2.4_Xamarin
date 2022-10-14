using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS; 
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using QuickDate.Activities.Base;
using QuickDate.Activities.SearchFilter.Fragment;
using QuickDate.Activities.Tabbes;
using QuickDate.Adapters;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Requests; 
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.SearchFilter
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SearchFilterTabbedActivity : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private ViewPager2 ViewPager;
        private TabLayout TabLayout;

        private FilterBackgroundFragment BackgroundTab;
        private BasicFragment BasicTab;
        private LooksFragment LooksTab;
        private MoreFragment MoreTab;
        private LifestyleFragment LifestyleTab;
        private TextView ActionButton;
        public Button ResetFilterButton;
        

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
                SetContentView(Resource.Layout.SearchFilterTabbedLayout);

                LoadFilterOptionsData();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_RewardedInterstitial(this);
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
                

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach(); 

                TabLayout.SetTabTextColors(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor, Color.ParseColor(AppSettings.MainColor));

                ActionButton = FindViewById<TextView>(Resource.Id.toolbar_title);
                ActionButton.Text = GetText(Resource.String.Lbl_ApplyFilter);
                ActionButton.SetTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);

                ResetFilterButton = FindViewById<Button>(Resource.Id.ResetFilterButton);
                ResetFilterButton.Text = GetText(Resource.String.Lbl_ResetFilter);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_Filter);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);
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
                    ActionButton.Click += ActionButtonOnClick;
                    ResetFilterButton.Click += ResetFilterButtonClick;
                }
                else
                {
                    ActionButton.Click -= ActionButtonOnClick;
                    ResetFilterButton.Click -= ResetFilterButtonClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ResetFilterButtonClick(object sender, EventArgs e)
        {
            ResetAllFilters();
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                Adapter = new MainTabAdapter(this);

                if (AppSettings.ShowFilterBasic)
                {
                    BasicTab = new BasicFragment();
                    Adapter.AddFragment(BasicTab, GetText(Resource.String.Lbl_Basics));
                }

                if (AppSettings.ShowFilterLooks)
                {
                    LooksTab = new LooksFragment();
                    Adapter.AddFragment(LooksTab, GetText(Resource.String.Lbl_Looks));
                }

                if (AppSettings.ShowFilterBackground)
                {
                    BackgroundTab = new FilterBackgroundFragment();

                    Adapter.AddFragment(BackgroundTab, GetText(Resource.String.Lbl_Background));
                }
                if (AppSettings.ShowFilterLifestyle)
                {
                    LifestyleTab = new LifestyleFragment();
                    Adapter.AddFragment(LifestyleTab, GetText(Resource.String.Lbl_Lifestyle));
                }

                if (AppSettings.ShowFilterMore)
                {
                    MoreTab = new MoreFragment();
                    Adapter.AddFragment(MoreTab, GetText(Resource.String.Lbl_More));
                }

                ViewPager.OffscreenPageLimit = Adapter.ItemCount;
                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));
                viewPager.Adapter = Adapter;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly SearchFilterTabbedActivity Activity;

            public MyOnPageChangeCallback(SearchFilterTabbedActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
            {
                try
                {
                    base.OnPageScrolled(position, positionOffset, positionOffsetPixels);
                    Activity.ShowHideResetFilterOption(position);
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                } 
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    Activity.ShowHideResetFilterOption(position);
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }


        #endregion Set Tab

        #region Event

        private void ActionButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                // check current state of a Switch (true or false).
                if (BasicTab != null)
                {
                    UserDetails.AgeMin = BasicTab.AgeMin = (int)BasicTab.AgeSeekBar.GetSelectedMinValue();
                    UserDetails.AgeMax = BasicTab.AgeMax = (int)BasicTab.AgeSeekBar.GetSelectedMaxValue();
                    UserDetails.Gender = BasicTab.Gender;
                    UserDetails.Location = BasicTab.Location;
                    UserDetails.SwitchState = BasicTab.SwitchState;
                    UserDetails.Located = BasicTab.DistanceCount.ToString();
                }

                if (BackgroundTab != null)
                {
                    UserDetails.Language = BackgroundTab.Language;
                    UserDetails.Ethnicity = BackgroundTab.IdEthnicity.ToString();
                    UserDetails.Religion = BackgroundTab.IdReligion.ToString();
                }

                if (LifestyleTab != null)
                {
                    UserDetails.RelationShip = LifestyleTab.IdRelationShip.ToString();
                    UserDetails.Smoke = LifestyleTab.IdSmoke.ToString();
                    UserDetails.Drink = LifestyleTab.IdDrink.ToString();
                }

                if (LooksTab != null)
                {
                    UserDetails.Body = LooksTab.IdBody.ToString();
                    UserDetails.FromHeight = LooksTab.FromHeight;
                    UserDetails.ToHeight = LooksTab.ToHeight;
                }

                if (MoreTab != null)
                {
                    UserDetails.Interest = MoreTab.Interest;
                    UserDetails.Education = MoreTab.IdEducation.ToString();
                    UserDetails.Pets = MoreTab.IdPets.ToString();
                }
                 
                SaveFilterOptions();

                SetLocationUser();

                var mainContext = HomeActivity.GetInstance();
                if (mainContext.TrendingFragment?.MAdapter != null)
                {
                    var checkList = mainContext.TrendingFragment.MAdapter?.TrendingList?.Where(q => q.Type == ItemType.Users).ToList();
                    if (checkList?.Count > 0)
                    {
                        checkList.Clear();
                        mainContext.TrendingFragment.MAdapter.NotifyDataSetChanged();
                    }

                    var emptyStateChecker = mainContext.TrendingFragment.MAdapter?.TrendingList?.FirstOrDefault(a => a.Type == ItemType.EmptyPage);
                    if (emptyStateChecker != null)
                    {
                        mainContext.TrendingFragment.MAdapter.TrendingList.Remove(emptyStateChecker);
                        mainContext.TrendingFragment.MAdapter.NotifyDataSetChanged();
                    }

                    if (mainContext.TrendingFragment.MainScrollEvent != null)
                        mainContext.TrendingFragment.MainScrollEvent.IsLoading = false;

                    mainContext.TrendingFragment.SwipeRefreshLayout.Refreshing = true;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => mainContext.TrendingFragment.LoadUsersAsync() });
                }

                Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private void SetLocationUser()
        {
            try
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"show_me_to", UserDetails.Location},
                };
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Users.UpdateProfileAsync(dictionary) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LoadFilterOptionsData()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                dbDatabase.Get_data_Filter_Options();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SaveFilterOptions()
        {
            try
            {
                var filterOptions = new DataTables.FilterOptionsTb
                {
                    AgeMin = UserDetails.AgeMin,
                    AgeMax = UserDetails.AgeMax,
                    Gender = UserDetails.Gender,
                    Location = UserDetails.Location,
                    IsOnline = UserDetails.SwitchState,
                    Distance = UserDetails.Located,
                    Language = UserDetails.Language,
                    Ethnicity = UserDetails.Ethnicity,
                    Religion = UserDetails.Religion,
                    RelationShip = UserDetails.RelationShip,
                    Smoke = UserDetails.Smoke,
                    Drink = UserDetails.Drink,
                    Body = UserDetails.Body,
                    FromHeight = UserDetails.FromHeight,
                    ToHeight = UserDetails.ToHeight,
                    Interest = UserDetails.Interest,
                    Education = UserDetails.Education,
                    Pets = UserDetails.Pets
                };
                 
                var dbDatabase = new SqLiteDatabase();
                dbDatabase.InsertOrUpdateFilter_Options(filterOptions);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ResetAllFilters()
        {
            try
            { 
                UserDetails.AgeMin = UserDetails.FilterOptionAgeMin;
                UserDetails.AgeMax = UserDetails.FilterOptionAgeMax;
                UserDetails.Gender = UserDetails.FilterOptionGender;
                UserDetails.Location = "";
                UserDetails.SwitchState = UserDetails.FilterOptionIsOnline;
                UserDetails.Located = UserDetails.FilterOptionDistance;

                UserDetails.Language = UserDetails.FilterOptionLanguage;
                UserDetails.Ethnicity = "";
                UserDetails.Religion = "";

                UserDetails.RelationShip = "";
                UserDetails.Smoke = "";
                UserDetails.Drink = "";

                UserDetails.Body = "";
                UserDetails.FromHeight = UserDetails.FilterOptionFromHeight;
                UserDetails.ToHeight = UserDetails.FilterOptionToHeight;

                UserDetails.Interest = "";
                UserDetails.Education = "";
                UserDetails.Pets = "";

                SaveFilterOptions();
                LoadFilterOptionsData();

                BasicTab?.SetLocalData();
                BackgroundTab?.SetLocalData();
                LooksTab?.SetLocalData();
                MoreTab?.SetLocalData();
                LifestyleTab?.SetLocalData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
          
        private void ShowHideResetFilterOption(int position)
        {
            try
            {
                ResetFilterButton.Visibility = AppSettings.ShowResetFilterForAllPages || position == 0 ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}