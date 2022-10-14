using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using Exception = System.Exception;

namespace QuickDate.Activities.SearchFilter.Fragment
{
    public class FilterBackgroundFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private SearchFilterTabbedActivity GlobalContext;

        private TextView LanguageIcon, EthnicityIcon, ReligionIcon;
        private EditText EdtLanguage, EdtEthnicity, EdtReligion;
        private PublisherAdView PublisherAdView;
        private string TypeDialog;
        public int IdEthnicity, IdReligion;
        public string Language;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (SearchFilterTabbedActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.FilterBackgroundLayout, container, false); 
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }
         
        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetLocalData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

            }
        }

        public override void OnResume()
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

        public override void OnPause()
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

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroy()
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                LanguageIcon = view.FindViewById<TextView>(Resource.Id.IconLanguage);
                EdtLanguage = view.FindViewById<EditText>(Resource.Id.LanguageEditText);

                EthnicityIcon = view.FindViewById<TextView>(Resource.Id.IconEthnicity);
                EdtEthnicity = view.FindViewById<EditText>(Resource.Id.EthnicityEditText);

                ReligionIcon = view.FindViewById<TextView>(Resource.Id.IconReligion);
                EdtReligion = view.FindViewById<EditText>(Resource.Id.ReligionEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, LanguageIcon, FontAwesomeIcon.Language);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, EthnicityIcon, FontAwesomeIcon.TheaterMasks);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, ReligionIcon, FontAwesomeIcon.Church);

                Methods.SetColorEditText(EdtLanguage, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtEthnicity, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtReligion, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(EdtLanguage);
                Methods.SetFocusable(EdtEthnicity);
                Methods.SetFocusable(EdtReligion);

                PublisherAdView = view.FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetLocalData()
        {
            Language = UserDetails.Language.ToLower();
            EdtLanguage.Text = Language;

            var ethnicity = ListUtils.SettingsSiteList?.Ethnicity?.FirstOrDefault(a => a.ContainsKey(UserDetails.Ethnicity))?.Values.FirstOrDefault();

            IdEthnicity = string.IsNullOrWhiteSpace(UserDetails.Ethnicity) ? 0 : int.Parse(UserDetails.Ethnicity);

            EdtEthnicity.Text = ethnicity;

            var religion = ListUtils.SettingsSiteList?.Religion?.FirstOrDefault(a => a.ContainsKey(UserDetails.Religion))?.Values.FirstOrDefault();

            IdReligion = string.IsNullOrWhiteSpace(UserDetails.Religion) ? 0 : int.Parse(UserDetails.Religion);

            EdtReligion.Text = religion;
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    EdtLanguage.Touch += EdtLanguageOnClick;
                    EdtEthnicity.Touch += EdtEthnicityOnClick;
                    EdtReligion.Touch += EdtReligionOnClick;
                }
                else
                {
                    EdtLanguage.Touch -= EdtLanguageOnClick;
                    EdtEthnicity.Touch -= EdtEthnicityOnClick;
                    EdtReligion.Touch -= EdtReligionOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

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
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

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

        //Ethnicity
        private void EdtEthnicityOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Ethnicity";
                //string[] ethnicityArray = Application.Context.Resources.GetStringArray(Resource.Array.EthnicityArray);
                var ethnicityArray = ListUtils.SettingsSiteList?.Ethnicity;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (ethnicityArray != null) arrayAdapter.AddRange(ethnicityArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_BodyType)).TitleColorRes(Resource.Color.primary);
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

        //Religion
        private void EdtReligionOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Religion";
                //string[] religionArray = Application.Context.Resources.GetStringArray(Resource.Array.ReligionArray);
                var religionArray = ListUtils.SettingsSiteList?.Religion;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (religionArray != null) arrayAdapter.AddRange(religionArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Religion)).TitleColorRes(Resource.Color.primary);
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

        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Language":
                        Language = itemString.ToLower();
                        EdtLanguage.Text = itemString;
                        break;
                    case "Ethnicity":
                    {
                        var ethnicityArray = ListUtils.SettingsSiteList?.Ethnicity?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdEthnicity = int.Parse(ethnicityArray ?? "1");
                        EdtEthnicity.Text = itemString;
                        break;
                    }
                    case "Religion":
                    {
                        var religionArray = ListUtils.SettingsSiteList?.Religion?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdReligion = int.Parse(religionArray ?? "1");
                        EdtReligion.Text = itemString;
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