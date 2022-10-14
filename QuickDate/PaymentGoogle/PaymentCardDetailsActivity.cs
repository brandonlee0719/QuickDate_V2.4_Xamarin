using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Stripe.Android;
using Com.Stripe.Android.Model;
using Com.Stripe.Android.View;
using QuickDate.Activities.Base;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.SQLite;
using QuickDateClient.Classes.Authorization;
using QuickDateClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace QuickDate.PaymentGoogle
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class PaymentCardDetailsActivity : BaseActivity, ITokenCallback
    {
        #region Variables Basic

        private TextView CardNumber, CardExpire, CardCvv, CardName;
        private EditText EtName;
        private Button BtnApply;
        private CardMultilineWidget MultilineWidget;

        private Stripe Stripe; 
        private string Price, PayType, Id, TokenId;
         
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
                SetContentView(Resource.Layout.PaymentCardDetailsLayout);

                Id = Intent?.GetStringExtra("Id") ?? "";
                Price = Intent?.GetStringExtra("Price") ?? "";
                PayType = Intent?.GetStringExtra("payType") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitWalletStripe();
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
                CardNumber = (TextView)FindViewById(Resource.Id.card_number);
                CardExpire = (TextView)FindViewById(Resource.Id.card_expire);
                CardCvv = (TextView)FindViewById(Resource.Id.card_cvv);
                CardName = (TextView)FindViewById(Resource.Id.card_name);

                MultilineWidget = (CardMultilineWidget)FindViewById(Resource.Id.card_multiline_widget);

                EtName = (EditText)FindViewById(Resource.Id.et_name);
                BtnApply = (Button)FindViewById(Resource.Id.ApplyButton);

                Methods.SetColorEditText(EtName, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    toolbar.Title = GetString(Resource.String.Lbl_CreditCard);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? AppSettings.TitleTextColorDark : AppSettings.TitleTextColor);
                    SetSupportActionBar(toolbar);
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
                    MultilineWidget.CvcComplete += MultilineWidgetOnCvcComplete;
                    EtName.TextChanged += EtNameOnTextChanged;
                    BtnApply.Click += BtnApplyOnClick;
                }
                else
                {
                    MultilineWidget.CvcComplete -= MultilineWidgetOnCvcComplete;
                    EtName.TextChanged -= EtNameOnTextChanged;
                    BtnApply.Click -= BtnApplyOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MultilineWidgetOnCvcComplete(object sender, EventArgs e)
        {
            try
            {
                if (MultilineWidget.Card != null && MultilineWidget.Card.ValidateCard() && MultilineWidget.ValidateAllFields())
                {
                    if (MultilineWidget.Card.Number.Trim().Length == 0)
                    {
                        CardNumber.Text = "**** **** **** ****";
                    }
                    else
                    {
                        string number = InsertPeriodically(MultilineWidget.Card.Number.Trim(), " ", 4);
                        CardNumber.Text = number;
                    }

                    if (MultilineWidget.Card.ExpMonth.ToString().Trim().Length == 0 && MultilineWidget.Card.ExpYear.ToString().Trim().Length == 0)
                    {
                        CardExpire.Text = "MM/YY";
                    }
                    else
                    {
                        CardExpire.Text = MultilineWidget.Card.ExpMonth + "/" + MultilineWidget.Card.ExpYear;
                    }

                    CardCvv.Text = MultilineWidget.Card.CVC.Trim().Length == 0 ? "***" : MultilineWidget.Card.CVC.Trim();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void EtNameOnTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                CardName.Text = e?.Text?.ToString().Trim().Length == 0 ? GetString(Resource.String.Lbl_YourName) : e?.Text?.ToString().Trim();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Stripe
        private void BtnApplyOnClick(object sender, EventArgs e)
        {
            try
            {
                //Show a progress
                if (MultilineWidget.Card.ValidateCard() && !string.IsNullOrEmpty(EtName.Text))
                {
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    Com.Stripe.Android.Model.Card card = MultilineWidget.Card;
                    Stripe.CreateToken(card, PaymentConfiguration.Instance.PublishableKey, this);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_PleaseVerifyDataCard), ToastLength.Long)?.Show();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string InsertPeriodically(string text, string insert, int period)
        {
            try
            {
                var parts = SplitInParts(text, period);
                string formatted = string.Join(insert, parts);
                return formatted;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return text;
            }
        }

        public static IEnumerable<string> SplitInParts(string s, int partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

        #endregion

        #region Stripe

        private void InitWalletStripe()
        {
            try
            {
                var stripePublishableKey = ListUtils.SettingsSiteList?.StripeId ?? "";
                if (!string.IsNullOrEmpty(stripePublishableKey))
                {
                    PaymentConfiguration.Init(stripePublishableKey);
                    Stripe = new Stripe(this, stripePublishableKey);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ErrorConnectionSystemStripe), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnError(Java.Lang.Exception error)
        {
            try
            {
                AndHUD.Shared.Dismiss(this);
                Toast.MakeText(this, error.Message, ToastLength.Long)?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void OnSuccess(Token token)
        {
            try
            {
                // Send token to your own web service
                //var stripeBankAccount = token.BankAccount;
                //var stripeCard = token.Card;
                //var stripeCreated = token.Created;
                TokenId = token.Id;
                //var stripeLiveMode = token.Livemode;
                //var stripeType = token.Type;
                //var stripeUsed = token.Used;

                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.Auth.PayStripeAsync(TokenId, PayType, "Pay the card " + Price, Price);
                    if (apiStatus == 200)
                    {
                        if (respond is AmountObject result)
                        {
                            var dataUser = ListUtils.MyUserInfo?.FirstOrDefault(a => a.Id == UserDetails.UserId);
                            if (dataUser != null)
                            {
                                if (PayType != "membership")
                                {
                                    dataUser.Balance = result.CreditAmount.ToString();
                                }
                                else
                                {
                                    dataUser.VerifiedFinal = true;
                                    dataUser.IsPro = "1";
                                }

                                var sqlEntity = new SqLiteDatabase();
                                sqlEntity.InsertOrUpdate_DataMyInfo(dataUser);
                            }

                            if (HomeActivity.GetInstance().ProfileFragment.WalletNumber != null)
                                HomeActivity.GetInstance().ProfileFragment.WalletNumber.Text = result.CreditAmount.ToString();

                            Toast.MakeText(this, GetText(Resource.String.Lbl_PaymentSuccessfully), ToastLength.Long)?.Show();
                            AndHUD.Shared.Dismiss(this);

                            Finish();
                        }
                    }
                    else Methods.DisplayAndHudErrorResult(this, respond);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion  
    }
}