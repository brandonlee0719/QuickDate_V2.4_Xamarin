using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Com.CodeMyBrainsOut.Onboarder;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Utils;

namespace QuickDate.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class BoardingActivity : AhoyOnboarderActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                var onBoarderCard1 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page1), GetString(Resource.String.Lbl_Description_page1), Resource.Drawable.Onboarding_icon1)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                var onBoarderCard2 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page2), GetString(Resource.String.Lbl_Description_page2), Resource.Drawable.Onboarding_icon2)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                var onBoarderCard3 = new AhoyOnboarderCard(GetString(Resource.String.Lbl_Title_page3), GetString(Resource.String.Lbl_Description_page3), Resource.Drawable.Onboarding_icon3)
                {
                    BackgroundColor = Resource.Color.accent_transparent
                };

                onBoarderCard1.TitleColor = Resource.Color.textIcon;
                onBoarderCard2.TitleColor = Resource.Color.textIcon;
                onBoarderCard3.TitleColor = Resource.Color.textIcon;

                onBoarderCard1.DescriptionColor = Resource.Color.textIcon;
                onBoarderCard2.DescriptionColor = Resource.Color.textIcon;
                onBoarderCard3.DescriptionColor = Resource.Color.textIcon;

                SetGradientBackground();

                SetFinishButtonTitle(GetString(Resource.String.Lbl_FinishButton_WalkTroutPage));

                var pages = new List<AhoyOnboarderCard>
                {
                    onBoarderCard1, onBoarderCard2, onBoarderCard3
                };

                SetOnboardPages(pages);

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetStickers(this), () => ApiRequest.GetGifts(this) });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnFinishButtonPressed()
        {
            try
            {
                SaveAppSettings();
                StartActivity(new Intent(this, typeof(HomeActivity)));
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SaveAppSettings()
        {
            AppSettings.ShowWalkTroutPage = false;
             
            MainSettings.StoreShowWalkThroughPageValue(AppSettings.ShowWalkTroutPage);
        }
    }
}