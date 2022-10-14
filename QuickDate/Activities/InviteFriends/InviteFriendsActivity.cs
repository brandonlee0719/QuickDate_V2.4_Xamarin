using System;
using System.Linq;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.OS;
using Android.Views;
using Android.Widget;
using QuickDate.Activities.Base;
using QuickDate.Library.Anjo.Share;
using QuickDate.Library.Anjo.Share.Abstractions;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.Activities.InviteFriends
{
    [Activity(Icon ="@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class InviteFriendsActivity : BaseActivity
    {
        #region Variables Basic

        private TextView TxtSubTitle; 
        private TextView IconCopy, IconTextInvite, IconSocialInvite;
        private TextView IconGo1, IconGo2, IconGo3;
        private RelativeLayout CopyLayouts,  TextLayouts, SocialLayouts;
        private PublisherAdView PublisherAdView;
         
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
                SetContentView(Resource.Layout.InviteFriendsLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_RewardedVideo(this);
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
                TxtSubTitle = FindViewById<TextView>(Resource.Id.subTitle); 
                IconCopy = FindViewById<TextView>(Resource.Id.IconCopy);
                IconTextInvite = FindViewById<TextView>(Resource.Id.IconTextInvite);
                IconSocialInvite = FindViewById<TextView>(Resource.Id.IconSocialInvite);
                IconGo1 = FindViewById<TextView>(Resource.Id.iconGo1);
                IconGo2 = FindViewById<TextView>(Resource.Id.iconGo2);
                IconGo3 = FindViewById<TextView>(Resource.Id.iconGo3);

                CopyLayouts = FindViewById<RelativeLayout>(Resource.Id.copyLayouts);
                TextLayouts = FindViewById<RelativeLayout>(Resource.Id.textLayouts);
                SocialLayouts = FindViewById<RelativeLayout>(Resource.Id.socialLayouts);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGo1, FontAwesomeIcon.AngleRight);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGo2, FontAwesomeIcon.AngleRight);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconGo3, FontAwesomeIcon.AngleRight);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconCopy, IonIconsFonts.Link);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconTextInvite, IonIconsFonts.Text);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconSocialInvite, IonIconsFonts.Share);
                 
                TxtSubTitle.Text = GetText(Resource.String.Lbl_ShareThe) + " " + AppSettings.ApplicationName + " " + GetText(Resource.String.Lbl_byInvitingFriends);

                if (!AppSettings.InvitationSystem)
                    TextLayouts.Visibility = ViewStates.Gone;

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolbar.Title = GetText(Resource.String.Lbl_InviteFriends);
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
                    CopyLayouts.Click += CopyLayoutsOnClick;
                    TextLayouts.Click += TextLayoutsOnClick;
                    SocialLayouts.Click += SocialLayoutsOnClick;
                }
                else
                {
                    CopyLayouts.Click -= CopyLayoutsOnClick;
                    TextLayouts.Click -= TextLayoutsOnClick;
                    SocialLayouts.Click -= SocialLayoutsOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        //Share
        private async void SocialLayoutsOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!CrossShare.IsSupported)
                    return;
                 
                var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = dataUser?.Username,
                    Text = "",
                    Url = UserDetails.Url
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Read Contacts Numbers
        private void TextLayoutsOnClick(object sender, EventArgs e)
        {
            try
            {
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    StartActivity(new Intent(this, typeof(InviteContactActivity)));
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.ReadContacts) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadPhoneNumbers) == Permission.Granted)
                        StartActivity(new Intent(this, typeof(InviteContactActivity)));
                    else
                      new PermissionsController(this).RequestPermission(101); 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        // Copy link
        private void CopyLayoutsOnClick(object sender, EventArgs e)
        {
            try
            { 
                Methods.CopyToClipboard(this, UserDetails.Url);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions   

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 101)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StartActivity(new Intent(this, typeof(InviteContactActivity)));
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
    }
}