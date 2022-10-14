using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Widget;
using InAppBilling.Lib;
using QuickDate.Helpers.Utils;  

namespace QuickDate.PaymentGoogle
{
    public class InitInAppBillingPayment : BillingProcessor.IBillingHandler
    {
        private readonly Activity ActivityContext;
        public string PayType, Price, Id, Credits;
        public BillingProcessor Handler;
        private List<SkuDetails> Products;
        private bool ReadyToPurchase;

        public InitInAppBillingPayment(Activity activity)
        {
            try
            {
                ActivityContext = activity;

                if (!BillingProcessor.IsIabServiceAvailable(activity , AppSettings.TripleDesAppServiceProvider))
                {
                    Console.WriteLine("In-app billing service is unavailable, please upgrade Android Market/Play to version >= 3.9.16");
                    return;
                }

                SetConnInAppBilling();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #region In-App Billing Google

        public void SetConnInAppBilling()
        {
            try
            {
                switch (Handler)
                {
                    case null:
                        Handler = new BillingProcessor(ActivityContext, InAppBillingGoogle.ProductId, AppSettings.TripleDesAppServiceProvider, this);
                        Handler.Initialize();
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        public void DisconnectInAppBilling()
        {
            try
            {
                Handler?.Release();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitInAppBilling(string price, string payType, string credits, string id)
        {
            Price = price; PayType = payType; Credits = credits; Id = id;

            if (Methods.CheckConnectivity())
            {
                if (!ReadyToPurchase || !Handler.IsInitialized())
                {
                    return;
                }

                try
                {
                    Products = Handler.GetPurchaseListingDetails(InAppBillingGoogle.ListProductSku);
                    switch (Products.Count)
                    {
                        case > 0:
                            { 
                                var bagOfCredits = Products.FirstOrDefault(a => a.ProductId == "bagofcredits");
                                var boxOfCredits = Products.FirstOrDefault(a => a.ProductId == "boxofcredits");
                                var chestOfCredits = Products.FirstOrDefault(a => a.ProductId == "chestofcredits");
                                var memberShipWeekly = Products.FirstOrDefault(a => a.ProductId == "membershipweekly");
                                var membershipMonthly = Products.FirstOrDefault(a => a.ProductId == "membershipmonthly");
                                var membershipYearly = Products.FirstOrDefault(a => a.ProductId == "membershipyearly");
                                var membershipLifeTime = Products.FirstOrDefault(a => a.ProductId == "membershiplifetime");

                                var option = ListUtils.SettingsSiteList;

                                switch (PayType)
                                {
                                    case "credits" when Credits == option?.BagOfCreditsAmount:
                                        Handler.Purchase(ActivityContext,bagOfCredits?.ProductId);
                                        break;
                                    case "credits" when Credits == option?.BoxOfCreditsAmount:
                                        Handler.Purchase(ActivityContext,boxOfCredits?.ProductId);
                                        break;
                                    case "credits" when Credits == option?.ChestOfCreditsAmount:
                                        Handler.Purchase(ActivityContext,chestOfCredits?.ProductId);
                                        break;
                                    //Weekly
                                    case "membership" when Id == "1":
                                        Handler.Purchase(ActivityContext,memberShipWeekly?.ProductId);
                                        break;
                                    //Monthly
                                    case "membership" when Id == "2":
                                        Handler.Purchase(ActivityContext,membershipMonthly?.ProductId);
                                        break;
                                    //Yearly
                                    case "membership" when Id == "3":
                                        Handler.Purchase(ActivityContext,membershipYearly?.ProductId);
                                        break;
                                    case "membership" when Id == "4":
                                        Handler.Purchase(ActivityContext,membershipLifeTime?.ProductId);
                                        break;
                                }
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    //Something else has gone wrong, log it
                    Methods.DisplayReportResultTrack(ex);
                } 
            }
            else
            {
                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long)?.Show();
            }
        }

        #endregion
         
        public void OnProductPurchased(string productId, TransactionDetails details)
        {
            Console.WriteLine("onProductPurchased: " + productId);
        }

        public void OnPurchaseHistoryRestored()
        {
            Console.WriteLine("onPurchaseHistoryRestored");

            foreach (var sku in Handler.ListOwnedProducts())
                Console.WriteLine("Owned Managed Product: " + sku);

            //foreach (var sku in Handler.ListOwnedSubscriptions())
            //    Console.WriteLine("Owned Subscription: " + sku); 
        }

        public void OnBillingError(int errorCode, Exception error)
        {
            Console.WriteLine("onBillingError: " + errorCode + " " + error.Message);
        }

        public void OnBillingInitialized()
        {
            Console.WriteLine("onBillingInitialized");
            ReadyToPurchase = true;
        }
    }
}