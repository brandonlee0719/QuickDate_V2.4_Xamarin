using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDialogsCore;
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
    public class LifestyleFragment : AndroidX.Fragment.App.Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private SearchFilterTabbedActivity GlobalContext;

        private TextView RelationshipIcon, SmokeIcon, DrinkIcon;
        private EditText EdtRelationship, EdtSmoke, EdtDrink;
        private string TypeDialog;
        public int IdRelationShip, IdSmoke, IdDrink;

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
                View view = inflater.Inflate(Resource.Layout.FilterLifestyleLayout, container, false); 
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
                AddOrRemoveEvent(true);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                RelationshipIcon = view.FindViewById<TextView>(Resource.Id.IconRelationship);
                EdtRelationship = view.FindViewById<EditText>(Resource.Id.RelationshipEditText);

                SmokeIcon = view.FindViewById<TextView>(Resource.Id.IconSmoke);
                EdtSmoke = view.FindViewById<EditText>(Resource.Id.SmokeEditText);

                DrinkIcon = view.FindViewById<TextView>(Resource.Id.IconDrink);
                EdtDrink = view.FindViewById<EditText>(Resource.Id.DrinkEditText);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, RelationshipIcon, FontAwesomeIcon.Heart);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, SmokeIcon, FontAwesomeIcon.Smoking);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, DrinkIcon, FontAwesomeIcon.Beer);

                Methods.SetColorEditText(EdtRelationship, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtSmoke, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(EdtDrink, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(EdtRelationship);
                Methods.SetFocusable(EdtSmoke);
                Methods.SetFocusable(EdtDrink);

                var template = view.FindViewById<TemplateView>(Resource.Id.my_template);
                AdsGoogle.Ad_AdMobNative(GlobalContext, template);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public void SetLocalData()
        {
            var relationship = ListUtils.SettingsSiteList?.Relationship?.FirstOrDefault(a => a.ContainsKey(UserDetails.RelationShip))?.Values.FirstOrDefault();
            IdRelationShip = string.IsNullOrWhiteSpace(UserDetails.RelationShip) ? 0 : int.Parse(UserDetails.RelationShip);
            EdtRelationship.Text = relationship;

            var smoke = ListUtils.SettingsSiteList?.Smoke?.FirstOrDefault(a => a.ContainsKey(UserDetails.Smoke))?.Values.FirstOrDefault();
            IdSmoke = string.IsNullOrWhiteSpace(UserDetails.Smoke) ? 0 : int.Parse(UserDetails.Smoke);
            EdtSmoke.Text = smoke;

            var drink = ListUtils.SettingsSiteList?.Drink?.FirstOrDefault(a => a.ContainsKey(UserDetails.Drink))?.Values.FirstOrDefault();
            IdDrink = string.IsNullOrWhiteSpace(UserDetails.Drink) ? 0 : int.Parse(UserDetails.Drink);
            EdtDrink.Text = drink;
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    EdtRelationship.Touch += EdtRelationshipOnClick;
                    EdtSmoke.Touch += EdtSmokeOnClick;
                    EdtDrink.Touch += EdtDrinkOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

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
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

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

        //Drink
        private void EdtDrinkOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Drink";
                //string[] drinkArray = Application.Context.Resources.GetStringArray(Resource.Array.DrinkArray);
                var drinkArray = ListUtils.SettingsSiteList?.Drink;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (drinkArray != null) arrayAdapter.AddRange(drinkArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Drink)).TitleColorRes(Resource.Color.primary);
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

        //Smoke
        private void EdtSmokeOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;
                TypeDialog = "Smoke";
                //string[] smokeArray = Application.Context.Resources.GetStringArray(Resource.Array.SmokeArray);
                var smokeArray = ListUtils.SettingsSiteList?.Smoke;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                if (smokeArray != null) arrayAdapter.AddRange(smokeArray.Select(item => Methods.FunString.DecodeString(item.Values.FirstOrDefault())));

                dialogList.Title(GetText(Resource.String.Lbl_Smoke)).TitleColorRes(Resource.Color.primary);
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
                    case "Relationship":
                    {
                        var relationshipArray = ListUtils.SettingsSiteList?.Relationship?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdRelationShip = int.Parse(relationshipArray ?? "1");
                        EdtRelationship.Text = itemString;
                        break;
                    }
                    case "Smoke":
                    {
                        var smokeArray = ListUtils.SettingsSiteList?.Smoke?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdSmoke = int.Parse(smokeArray ?? "1");
                        EdtSmoke.Text = itemString;
                        break;
                    }
                    case "Drink":
                    {
                        var drinkArray = ListUtils.SettingsSiteList?.Drink?.FirstOrDefault(a => a.ContainsValue(itemString))?.Keys.FirstOrDefault();
                        IdDrink = int.Parse(drinkArray ?? "1");
                        EdtDrink.Text = itemString;
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