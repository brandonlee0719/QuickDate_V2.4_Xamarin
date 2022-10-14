using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MaterialDialogsCore;
using Android.App;
using Android.Content;
using Android.Graphics;


using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager.Widget;
using Me.Relex.CircleIndicatorLib;
using QuickDate.Activities;
using QuickDate.Activities.Premium;
using QuickDate.Activities.Premium.Adapters;
using QuickDate.Activities.SettingsUser;
using QuickDate.Activities.Tabbes;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.PaymentGoogle;
using QuickDateClient;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace QuickDate.Helpers.Controller
{
    public class PopupController: Object, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity ActivityContext;
        private string CreditType;
        private CreditAdapter CreditAdapter;
        private Dialog PremiumWindow, DialogAddCredits, AddPhoneNumberWindow;
        private PremiumAdapter PremiumAdapter;
        private EditText TxtNumber1, TxtNumber2;
        private string FullNumber, DialogButtonType;
        private CreditsClass ItemCredits;
        private PremiumClass ItemPremium;

        public PopupController(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //============================ Phone Number ================================   

        #region Dialog >> Add Phone Number

        public void DisplayAddPhoneNumber()
        {
            try
            {
                var dataTwilio = ListUtils.SettingsSiteList;
                if (dataTwilio != null && string.IsNullOrEmpty(dataTwilio.SmsTwilioUsername) && string.IsNullOrEmpty(dataTwilio.SmsTwilioPassword) && string.IsNullOrEmpty(dataTwilio.SmsTPhoneNumber))
                    return;

                AddPhoneNumberWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                AddPhoneNumberWindow?.SetContentView(Resource.Layout.DialogAddPhoneNumber);

                TxtNumber1 = AddPhoneNumberWindow?.FindViewById<EditText>(Resource.Id.numberEdit1); //Gone
                TxtNumber2 = AddPhoneNumberWindow?.FindViewById<EditText>(Resource.Id.numberEdit2);

                Methods.SetColorEditText(TxtNumber1, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtNumber2, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                
                var dataUser = ListUtils.MyUserInfo?.FirstOrDefault();
                if (!string.IsNullOrEmpty(dataUser?.PhoneNumber))
                {
                    var correctly = Methods.FunString.IsPhoneNumber(dataUser.PhoneNumber);
                    if (correctly)
                    {
                        TxtNumber2.Text = dataUser.PhoneNumber/*.TrimStart(new[] { '0' , '+' })*/; 
                    }
                }

                FullNumber = TxtNumber2.Text/*.TrimStart(new[] { '0', '+' })*/;

                var btnAddPhoneNumber = AddPhoneNumberWindow?.FindViewById<Button>(Resource.Id.sentButton);
                var btnSkipAddPhoneNumber = AddPhoneNumberWindow?.FindViewById<TextView>(Resource.Id.skipbutton);

                btnAddPhoneNumber.Click += BtnAddPhoneNumberOnClick;
                btnSkipAddPhoneNumber.Click += BtnSkipAddPhoneNumberOnClick;
                 
                AddPhoneNumberWindow?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        private void BtnSkipAddPhoneNumberOnClick(object sender, EventArgs e)
        {
            try
            {
                AddPhoneNumberWindow?.Hide();
                AddPhoneNumberWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnAddPhoneNumberOnClick(object sender, EventArgs e)
        {
            try
            {
                FullNumber =  TxtNumber2.Text;

                if (Regex.IsMatch(FullNumber, "^\\+?(\\d[\\d-. ]+)?(\\([\\d-. ]+\\))?[\\d-. ]+\\d$") &&
                    FullNumber.Length >= 10)
                {
                    if (!string.IsNullOrEmpty(FullNumber))
                    {
                        Intent intent = new Intent(ActivityContext, typeof(VerificationCodeActivity));
                        intent.PutExtra("Number", FullNumber);
                        ActivityContext.StartActivityForResult(intent, 125);

                        AddPhoneNumberWindow?.Hide();
                        AddPhoneNumberWindow?.Dismiss();
                    }
                }
                else
                {
                    var dialog = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);
                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(FullNumber + " " + ActivityContext.GetText(Resource.String.Lbl_ISNotValidNumber));
                    dialog.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Ok)).OnNegative(new MyMaterialDialog());
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //============================ Upgrade ================================   

        #region Dialog >> Upgrade
         
        public void DisplayPremiumWindow()
        {
            if (!AppSettings.PremiumSystemEnabled)
                return;
            try
            {
                PremiumWindow = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                PremiumWindow?.SetContentView(Resource.Layout.UpgradePremiumLayout);

                var recyclerView = PremiumWindow?.FindViewById<RecyclerView>(Resource.Id.recyler);
                 
                PremiumAdapter = new PremiumAdapter(ActivityContext);
                recyclerView.SetLayoutManager(new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false));
                PremiumAdapter.ItemClick += PremiumAdapterOnItemClick;
                recyclerView.SetAdapter(PremiumAdapter);

                var btnSkipAddCredits = PremiumWindow?.FindViewById<Button>(Resource.Id.skippButton);
                btnSkipAddCredits.Click += BtnSkipAddCreditsOnClick;

                PremiumWindow?.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnSkipAddCreditsOnClick(object sender, EventArgs e)
        {
            try
            {
                PremiumWindow?.Hide();
                PremiumWindow?.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

      
        //Open walletFragment with Google 
        private void PremiumAdapterOnItemClick(object sender, PremiumAdapterClickEventArgs e)
        {
            try
            {
                int position = e.Position;
                if (position > -1)
                {
                    if (!AppSettings.ShowPaypal && !AppSettings.ShowCreditCard && !AppSettings.ShowBankTransfer && !AppSettings.ShowInAppBilling && !InitializeQuickDate.IsExtended)
                        return;
                     
                    PremiumClass item = PremiumAdapter.GetItem(position);
                    if (item != null)
                    {
                        ItemPremium = item;

                        switch (AppSettings.PaymentsSystem)
                        {
                            case PaymentsSystem.All:
                            {
                                DialogButtonType = "membership";

                                var arrayAdapter = new List<string>();
                                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                if (AppSettings.ShowInAppBilling && InitializeQuickDate.IsExtended)
                                    arrayAdapter.Add(ActivityContext.GetString(Resource.String.Btn_GooglePlay));

                                if (AppSettings.ShowPaypal)
                                    arrayAdapter.Add(ActivityContext.GetString(Resource.String.Btn_Paypal));

                                if (AppSettings.ShowCreditCard)
                                    arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CreditCard));

                                if (AppSettings.ShowBankTransfer)
                                    arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_BankTransfer));

                                dialogList.Items(arrayAdapter);
                                dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                                dialogList.AlwaysCallSingleChoiceCallback();
                                dialogList.ItemsCallback(this).Build().Show();
                                break;
                            }
                            case PaymentsSystem.JustInAppBillingGoogle when AppSettings.ShowInAppBilling && InitializeQuickDate.IsExtended:
                                HomeActivity.GetInstance()?.BillingPayment.SetConnInAppBilling();
                                HomeActivity.GetInstance()?.BillingPayment?.InitInAppBilling(ItemPremium.Price, "membership", ItemPremium.Type, ItemPremium.Id.ToString());
                                break;
                        }

                        PremiumWindow?.Hide();
                        PremiumWindow?.Dismiss(); 
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        //============================ Credit ================================   

        #region Dialog >> Credit

        public void DisplayCreditWindow(string type)
        {
            try
            {
                CreditType = type;
                DialogAddCredits = new Dialog(ActivityContext, AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                DialogAddCredits.SetContentView(Resource.Layout.DialogAddCredits);

                var recyclerView = DialogAddCredits.FindViewById<RecyclerView>(Resource.Id.recyler);

                var viewPagerView = DialogAddCredits.FindViewById<ViewPager>(Resource.Id.viewPager);
                var indicator = DialogAddCredits.FindViewById<CircleIndicator>(Resource.Id.indicator);

                var titleText = DialogAddCredits.FindViewById<TextView>(Resource.Id.mainTitelText);
                titleText.Text = ActivityContext.GetText(Resource.String.Lbl_Your) + " " + AppSettings.ApplicationName + " " + ActivityContext.GetText(Resource.String.Lbl_CreditsBalance);

                var mainText = DialogAddCredits.FindViewById<TextView>(Resource.Id.mainText);
                var data = ListUtils.MyUserInfo?.FirstOrDefault();
                mainText.Text = data?.Balance.Replace(".00", "") + " " + ActivityContext.GetText(Resource.String.Lbl_Credits);

                var btnSkip = DialogAddCredits.FindViewById<Button>(Resource.Id.skippButton);
                var btnTerms = DialogAddCredits.FindViewById<TextView>(Resource.Id.TermsText);

                var creditsClass = new List<CreditsFeaturesClass>
                {
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits1), ColorCircle = "#00bee7",ImageFromResource = Resource.Drawable.viewPager_rocket},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits2), ColorCircle = "#0456C4" ,ImageFromResource = Resource.Drawable.viewPager_msg},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits3), ColorCircle = "#ff7102" ,ImageFromResource = Resource.Drawable.viewPager_gift},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits4), ColorCircle = "#4caf50" ,ImageFromResource = Resource.Drawable.viewPager_target},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits5), ColorCircle = "#8c4fe6" ,ImageFromResource = Resource.Drawable.viewPager_crown},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits6), ColorCircle = "#22e271" ,ImageFromResource = Resource.Drawable.viewPager_sticker},
                    new CreditsFeaturesClass {Description = ActivityContext.GetText(Resource.String.Lbl_DescriptionCredits7), ColorCircle = "#f44336",ImageFromResource = Resource.Drawable.viewPager_heart}
                };

                var imageDescViewPager = new ImageDescViewPager(ActivityContext, creditsClass);
                viewPagerView.Adapter = imageDescViewPager;
                indicator.SetViewPager(viewPagerView);

                CreditAdapter = new CreditAdapter(ActivityContext);
                recyclerView.SetLayoutManager(new LinearLayoutManager(ActivityContext, LinearLayoutManager.Horizontal, false));
                CreditAdapter.OnItemClick += CreditAdapterOnItemClick;
                recyclerView.SetAdapter(CreditAdapter);

                btnSkip.Click += BtnSkipOnClick;
                btnTerms.Click += BtnTermsOnClick;
                DialogAddCredits.Show();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Open walletFragment with Google
        private void CreditAdapterOnItemClick(object sender, CreditAdapterViewHolderClickEventArgs e)
        {
            try
            {
                int position = e.Position;
                if (position > -1)
                {
                    if (!AppSettings.ShowPaypal && !AppSettings.ShowCreditCard && !AppSettings.ShowBankTransfer && !AppSettings.ShowInAppBilling && !InitializeQuickDate.IsExtended)
                        return;

                    CreditsClass item = CreditAdapter.GetItem(position);
                    if (item != null)
                    {
                        ItemCredits = item;
                        DialogButtonType = CreditType;

                        switch (AppSettings.PaymentsSystem)
                        {
                            case PaymentsSystem.All:
                                {
                                    var arrayAdapter = new List<string>();
                                    var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                                    if (AppSettings.ShowInAppBilling && InitializeQuickDate.IsExtended)
                                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Btn_GooglePlay));

                                    if (AppSettings.ShowPaypal)
                                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Btn_Paypal));

                                    if (AppSettings.ShowCreditCard)
                                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CreditCard));

                                    if (AppSettings.ShowBankTransfer)
                                        arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_BankTransfer));

                                    dialogList.Items(arrayAdapter);
                                    dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                                    dialogList.AlwaysCallSingleChoiceCallback();
                                    dialogList.ItemsCallback(this).Build().Show();
                                    break;
                                }
                            case PaymentsSystem.JustInAppBillingGoogle when AppSettings.ShowInAppBilling && InitializeQuickDate.IsExtended:
                                HomeActivity.GetInstance()?.BillingPayment.SetConnInAppBilling();
                                HomeActivity.GetInstance()?.BillingPayment?.InitInAppBilling(ItemCredits.Price, CreditType, ItemCredits.TotalCoins, "");
                                break;
                        }

                        DialogAddCredits.Hide();
                        DialogAddCredits.Dismiss();
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnTermsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(LocalWebViewActivity));
                intent.PutExtra("URL", InitializeQuickDate.WebsiteUrl + "/terms");
                intent.PutExtra("Type", ActivityContext.GetText(Resource.String.Lbl_TermsOfUse));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void BtnSkipOnClick(object sender, EventArgs e)
        {
            try
            {
                DialogAddCredits.Hide();
                DialogAddCredits.Dismiss();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        #endregion

        //////////////////////////////////////////////////////

        #region MaterialDialog

        public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                string text = itemString;
                if (text == ActivityContext. GetString(Resource.String.Btn_Paypal))
                {
                    if (DialogButtonType == "membership")
                    {
                        HomeActivity.GetInstance()?.InitPayPalPayment?.BtnPaypalOnClick(ItemPremium.Price, "membership", ItemPremium.Type, ItemPremium.Id.ToString());
                    }
                    else if (DialogButtonType == CreditType)
                    {
                        HomeActivity.GetInstance()?.InitPayPalPayment?.BtnPaypalOnClick(ItemCredits.Price, CreditType, ItemCredits.TotalCoins,"");
                    }
                } 
                else if (text == ActivityContext. GetString(Resource.String.Btn_GooglePlay))
                {
                    if (DialogButtonType == "membership")
                    {
                        HomeActivity.GetInstance()?.BillingPayment.SetConnInAppBilling();
                        HomeActivity.GetInstance()?.BillingPayment?.InitInAppBilling(ItemPremium.Price, "membership", ItemPremium.Type, ItemPremium.Id.ToString());
                    }
                    else if (DialogButtonType == CreditType)
                    {
                        HomeActivity.GetInstance()?.BillingPayment.SetConnInAppBilling();
                        HomeActivity.GetInstance()?.BillingPayment?.InitInAppBilling(ItemCredits.Price, CreditType, ItemCredits.TotalCoins, "");
                    }
                }
                else if (text == ActivityContext.GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == ActivityContext.GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
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

        private void OpenIntentBankTransfer()
        {
            try
            {
                if (DialogButtonType == "membership")
                {
                    Intent intent = new Intent(ActivityContext, typeof(PaymentLocalActivity));
                    intent.PutExtra("Id", ItemPremium.Id.ToString());
                    intent.PutExtra("credits", ItemPremium.Type);
                    intent.PutExtra("Price", ItemPremium.Price);
                    intent.PutExtra("payType", "membership");
                    ActivityContext.StartActivity(intent);
                }
                else if (DialogButtonType == CreditType)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PaymentLocalActivity));
                    intent.PutExtra("credits", ItemCredits.Description + " " + ItemCredits.TotalCoins);
                    intent.PutExtra("Price", ItemCredits.Price);
                    intent.PutExtra("payType", CreditType); // credits|membership
                    ActivityContext.StartActivity(intent);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OpenIntentCreditCard()
        {
            try
            {
                if (DialogButtonType == "membership")
                {
                    Intent intent = new Intent(ActivityContext, typeof(PaymentCardDetailsActivity));
                    intent.PutExtra("Id", ItemPremium.Id.ToString());
                    intent.PutExtra("credits", ItemPremium.Type);
                    intent.PutExtra("Price", ItemPremium.Price);
                    intent.PutExtra("payType", "membership");
                    ActivityContext.StartActivity(intent);
                }
                else if (DialogButtonType == CreditType)
                {
                    Intent intent = new Intent(ActivityContext, typeof(PaymentCardDetailsActivity));
                    intent.PutExtra("credits", ItemCredits.TotalCoins);
                    intent.PutExtra("Price", ItemCredits.Price);
                    intent.PutExtra("payType", CreditType);// credits|membership
                    ActivityContext.StartActivity(intent);
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