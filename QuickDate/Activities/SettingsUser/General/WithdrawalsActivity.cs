using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;


using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.RecyclerView.Widget;
using QuickDate.Activities.Base;
using QuickDate.Activities.SettingsUser.Adapters;
using QuickDate.Helpers.Ads;
using QuickDate.Helpers.Controller;
using QuickDate.Helpers.Fonts;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDateClient.Classes.Global;
using QuickDateClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
  
namespace QuickDate.Activities.SettingsUser.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class WithdrawalsActivity : BaseActivity
    {
        #region Variables Basic

        private TextView CountBalanceText, SendText, IconAmount, IconPayPalEmail;
        private EditText AmountEditText, PayPalEmailEditText;
        private double CountBalance;
        private LinearLayout PaymentHistoryLinear;
        private TextView IconPaymentHistory;
        private RecyclerView MRecycler;
        private PaymentHistoryAdapter MAdapter;

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
                SetContentView(Resource.Layout.WithdrawalsLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Get_Data_User();
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
                SendText = FindViewById<TextView>(Resource.Id.toolbar_title);
                SendText.SetTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);

                CountBalanceText = FindViewById<TextView>(Resource.Id.countBalanceText);

                IconAmount = FindViewById<TextView>(Resource.Id.IconAmount);
                AmountEditText = FindViewById<EditText>(Resource.Id.AmountEditText);

                IconPayPalEmail = FindViewById<TextView>(Resource.Id.IconPayPalEmail);
                PayPalEmailEditText = FindViewById<EditText>(Resource.Id.PayPalEmailEditText);

                PaymentHistoryLinear = (LinearLayout)FindViewById(Resource.Id.PaymentHistoryLinear);
                IconPaymentHistory = (TextView)FindViewById(Resource.Id.iconPaymentHistory);
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPaymentHistory, FontAwesomeIcon.ListUl);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAmount, FontAwesomeIcon.HandHoldingUsd);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconPayPalEmail, FontAwesomeIcon.Paypal);

                Methods.SetColorEditText(AmountEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(PayPalEmailEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                AdsGoogle.Ad_AdMobNative(this);
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
                Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Withdrawals);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                //Pro Recycler View 
                MAdapter = new PaymentHistoryAdapter(this)
                {
                    AffPaymentList = new ObservableCollection<AffPayment>()
                }; 
                MRecycler.SetLayoutManager(new LinearLayoutManager(this));
                MRecycler.SetItemViewCacheSize(20);
                MRecycler.HasFixedSize = true;
                MRecycler.NestedScrollingEnabled = false;
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter); 
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
                    SendText.Click += SendTextOnClick; 
                }
                else
                {
                    SendText.Click -= SendTextOnClick; 
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events
          
        private async void SendTextOnClick(object sender, EventArgs e)
        {
            try
            {
                if (CountBalance < Convert.ToDouble(AmountEditText.Text))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CantRequestWithdrawals), ToastLength.Long)?.Show();
                }
                else if (string.IsNullOrEmpty(PayPalEmailEditText.Text.Replace(" ", "")) || string.IsNullOrEmpty(AmountEditText.Text.Replace(" ", "")))
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Please_check_your_details), ToastLength.Long)?.Show();
                }
                else
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));
                        
                        var (apiStatus, respond) = await RequestsAsync.Users.RequestWithdrawAsync(AmountEditText.Text, PayPalEmailEditText.Text);
                        if (apiStatus == 200)
                        {
                            if (respond is InfoObject result)
                            {
                               Console.WriteLine(result.Message);
                                Toast.MakeText(this, GetText(Resource.String.Lbl_RequestSentWithdrawals), ToastLength.Long)?.Show();
                                AndHUD.Shared.Dismiss(this);
                            }
                        }
                        else
                        {
                            if (respond is ErrorObject errorMessage)
                            {
                                var errorText = errorMessage.ErrorData.ErrorText;
                                //Show a Error image with a message
                                AndHUD.Shared.ShowError(this, errorText, MaskType.Clear, TimeSpan.FromSeconds(2));
                            }

                            //Methods.DisplayReportResult(this, respond);
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                    }
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        private async void Get_Data_User()
        {
            try
            {
                if (ListUtils.MyUserInfo.Count == 0)
                    await ApiRequest.GetInfoData(this, UserDetails.UserId.ToString());

                var local = ListUtils.MyUserInfo?.FirstOrDefault();
                if (local != null)
                {
                    CountBalance = Convert.ToDouble(local.AffBalance);
                    CountBalanceText.Text = "$" + CountBalance.ToString(CultureInfo.InvariantCulture);

                    if (string.IsNullOrEmpty(local.PaypalEmail))
                        PayPalEmailEditText.Text = local.PaypalEmail;
                      
                    if (local.AffPayments.Count > 0)
                    {
                        MAdapter.AffPaymentList = new ObservableCollection<AffPayment>(local.AffPayments);

                        MAdapter.AffPaymentList.Insert(0, new AffPayment
                        {
                            Id = "000",
                            Amount = GetString(Resource.String.Lbl_Amount),
                            Time = GetString(Resource.String.Lbl_Requested),
                            Status = GetString(Resource.String.Lbl_Status)
                        });

                        MAdapter.NotifyDataSetChanged();
                    }
                    else
                    {
                        PaymentHistoryLinear.Visibility = ViewStates.Gone;
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}