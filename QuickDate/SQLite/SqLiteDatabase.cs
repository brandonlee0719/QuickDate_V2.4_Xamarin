using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using QuickDate.Activities.Chat;
using QuickDate.Helpers.Model;
using QuickDate.Helpers.Utils;
using QuickDate.Library.OneSignalNotif;
using QuickDateClient;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Classes.Common;
using QuickDateClient.Classes.Global;
using SQLite;

namespace QuickDate.SQLite
{
    public class SqLiteDatabase  
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = Path.Combine(Folder, AppSettings.DatabaseName + "_.db");
        
        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            { 
                var connection = new SQLiteConnection(new SQLiteConnectionString(PathCombine, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true));
                return connection;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using var connection = OpenConnection();
                connection?.CreateTable<DataTables.LoginTb>();
                connection?.CreateTable<DataTables.SettingsTb>();
                connection?.CreateTable<DataTables.InfoUsersTb>();
                connection?.CreateTable<DataTables.GiftsTb>();
                connection?.CreateTable<DataTables.StickersTb>();
                connection?.CreateTable<DataTables.LastChatTb>();
                connection?.CreateTable<DataTables.MessageTb>();
                connection?.CreateTable<DataTables.FilterOptionsTb>(); 
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    CheckTablesStatus();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Delete table
        public void DropAll()
        {
            try
            {
                using var connection = OpenConnection();
                connection?.DropTable<DataTables.LoginTb>();
                connection?.DropTable<DataTables.SettingsTb>();
                connection?.DropTable<DataTables.InfoUsersTb>();
                connection?.DropTable<DataTables.GiftsTb>();
                connection?.DropTable<DataTables.StickersTb>();
                connection?.DropTable<DataTables.LastChatTb>();
                connection?.DropTable<DataTables.MessageTb>();
                connection?.DropTable<DataTables.FilterOptionsTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion Connection

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                connection.Insert(row);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                connection.Update(row);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                connection.Delete(row);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using var connection = OpenConnection();
                connection.InsertAll(row);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion General

        //Start SQL_Commander >>  Custom
        //*********************************************************

        #region Login

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    UserDetails.Username = dataUser.Username;
                    UserDetails.FullName = dataUser.Username;
                    UserDetails.Password = dataUser.Password;
                    UserDetails.AccessToken = dataUser.AccessToken;
                    UserDetails.UserId = Convert.ToInt32(dataUser.UserId);
                    UserDetails.Status = dataUser.Status;
                    UserDetails.Cookie = dataUser.Cookie;
                    UserDetails.Email = dataUser.Email;
                    UserDetails.DeviceId = dataUser.DeviceId;
                    AppSettings.Lang = dataUser.Lang;
                    Current.AccessToken = dataUser.AccessToken;

                    ListUtils.DataUserLoginList.Clear();
                    ListUtils.DataUserLoginList.Add(dataUser);

                    return dataUser;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login_Credentials();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }  
            }
        }

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using var connection = OpenConnection();
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.UserId = UserDetails.UserId.ToString();
                    dataUser.AccessToken = UserDetails.AccessToken;
                    dataUser.Cookie = UserDetails.Cookie;
                    dataUser.Username = UserDetails.Username;
                    dataUser.Password = UserDetails.Password;
                    dataUser.Status = UserDetails.Status;
                    dataUser.Lang = AppSettings.Lang;
                    dataUser.DeviceId = UserDetails.DeviceId;
                    dataUser.Email = UserDetails.Email;

                    connection.Update(dataUser);
                }
                else
                {
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin_Credentials(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Filter Options 


        //Get data Filter Options
        public DataTables.FilterOptionsTb Get_data_Filter_Options()
        {
            try
            {
                using var connection = OpenConnection();
                var dataFilterOptions = connection.Table<DataTables.FilterOptionsTb>().FirstOrDefault();
                if (dataFilterOptions != null)
                {
                    UserDetails.AgeMin = dataFilterOptions.AgeMin;
                    UserDetails.AgeMax = dataFilterOptions.AgeMax;
                    UserDetails.Gender = dataFilterOptions.Gender;
                    UserDetails.Location = dataFilterOptions.Location;
                    UserDetails.SwitchState = dataFilterOptions.IsOnline;
                    UserDetails.Located = dataFilterOptions.Distance;
                    UserDetails.Language = dataFilterOptions.Language;
                    UserDetails.Ethnicity = dataFilterOptions.Ethnicity;
                    UserDetails.Religion = dataFilterOptions.Religion;
                    UserDetails.RelationShip = dataFilterOptions.RelationShip;
                    UserDetails.Smoke = dataFilterOptions.Smoke;
                    UserDetails.Drink = dataFilterOptions.Drink;
                    UserDetails.Body = dataFilterOptions.Body;
                    UserDetails.FromHeight = dataFilterOptions.FromHeight;
                    UserDetails.ToHeight = dataFilterOptions.ToHeight;
                    UserDetails.Interest = dataFilterOptions.Interest;
                    UserDetails.Education = dataFilterOptions.Education;
                    UserDetails.Pets = dataFilterOptions.Pets;

                    return dataFilterOptions;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Filter_Options();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        //Insert Or Update data filter options
        public void InsertOrUpdateFilter_Options(DataTables.FilterOptionsTb db)
        {
            try
            {
                using var connection = OpenConnection();
                var dataFilterOptions = connection.Table<DataTables.FilterOptionsTb>().FirstOrDefault();
                if (dataFilterOptions != null)
                {
                    dataFilterOptions.AgeMin = UserDetails.AgeMin;
                    dataFilterOptions.AgeMax = UserDetails.AgeMax;
                    dataFilterOptions.Gender = UserDetails.Gender;
                    dataFilterOptions.Location = UserDetails.Location;
                    dataFilterOptions.IsOnline = UserDetails.SwitchState;
                    dataFilterOptions.Distance = UserDetails.Located;
                    dataFilterOptions.Language = UserDetails.Language;
                    dataFilterOptions.Ethnicity = UserDetails.Ethnicity;
                    dataFilterOptions.Religion = UserDetails.Religion;
                    dataFilterOptions.RelationShip = UserDetails.RelationShip;
                    dataFilterOptions.Smoke = UserDetails.Smoke;
                    dataFilterOptions.Drink = UserDetails.Drink;
                    dataFilterOptions.Body = UserDetails.Body;
                    dataFilterOptions.FromHeight = UserDetails.FromHeight;
                    dataFilterOptions.ToHeight = UserDetails.ToHeight;
                    dataFilterOptions.Interest = UserDetails.Interest;
                    dataFilterOptions.Education = UserDetails.Education;
                    dataFilterOptions.Pets = UserDetails.Pets;

                    connection.Update(dataFilterOptions);
                }
                else
                {
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateFilter_Options(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
         
        #endregion

        #region Settings

        public void InsertOrUpdateSettings(GetOptionsObject.DataOptions settingsData)
        {
            try
            {
                using var connection = OpenConnection();
                if (settingsData != null)
                {
                    var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    if (select == null)
                    {
                        var db = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);

                        db.Height = JsonConvert.SerializeObject(settingsData.Height);
                        db.Notification = JsonConvert.SerializeObject(settingsData.Notification);
                        db.Gender = JsonConvert.SerializeObject(settingsData.Gender);
                        db.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                        db.Countries = JsonConvert.SerializeObject(settingsData.Countries);
                        db.HairColor = JsonConvert.SerializeObject(settingsData.HairColor);
                        db.Travel = JsonConvert.SerializeObject(settingsData.Travel);
                        db.Drink = JsonConvert.SerializeObject(settingsData.Drink);
                        db.Smoke = JsonConvert.SerializeObject(settingsData.Smoke);
                        db.Religion = JsonConvert.SerializeObject(settingsData.Religion);
                        db.Car = JsonConvert.SerializeObject(settingsData.Car);
                        db.LiveWith = JsonConvert.SerializeObject(settingsData.LiveWith);
                        db.Pets = JsonConvert.SerializeObject(settingsData.Pets);
                        db.Friends = JsonConvert.SerializeObject(settingsData.Friends);
                        db.Children = JsonConvert.SerializeObject(settingsData.Children);
                        db.Character = JsonConvert.SerializeObject(settingsData.Character);
                        db.Body = JsonConvert.SerializeObject(settingsData.Body);
                        db.Ethnicity = JsonConvert.SerializeObject(settingsData.Ethnicity);
                        db.Education = JsonConvert.SerializeObject(settingsData.Education);
                        db.WorkStatus = JsonConvert.SerializeObject(settingsData.WorkStatus);
                        db.Relationship = JsonConvert.SerializeObject(settingsData.Relationship);
                        db.Language = JsonConvert.SerializeObject(settingsData.Language);
                        db.CustomFields = JsonConvert.SerializeObject(settingsData.CustomFields);

                        connection.Insert(db);
                    }
                    else
                    {
                        select = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData); 
                        if (select != null)
                        {
                            select.Height = JsonConvert.SerializeObject(settingsData.Height);
                            select.Notification = JsonConvert.SerializeObject(settingsData.Notification);
                            select.Gender = JsonConvert.SerializeObject(settingsData.Gender);
                            select.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                            select.Countries = JsonConvert.SerializeObject(settingsData.Countries);
                            select.HairColor = JsonConvert.SerializeObject(settingsData.HairColor);
                            select.Travel = JsonConvert.SerializeObject(settingsData.Travel);
                            select.Drink = JsonConvert.SerializeObject(settingsData.Drink);
                            select.Smoke = JsonConvert.SerializeObject(settingsData.Smoke);
                            select.Religion = JsonConvert.SerializeObject(settingsData.Religion);
                            select.Car = JsonConvert.SerializeObject(settingsData.Car);
                            select.LiveWith = JsonConvert.SerializeObject(settingsData.LiveWith);
                            select.Pets = JsonConvert.SerializeObject(settingsData.Pets);
                            select.Friends = JsonConvert.SerializeObject(settingsData.Friends);
                            select.Children = JsonConvert.SerializeObject(settingsData.Children);
                            select.Character = JsonConvert.SerializeObject(settingsData.Character);
                            select.Body = JsonConvert.SerializeObject(settingsData.Body);
                            select.Ethnicity = JsonConvert.SerializeObject(settingsData.Ethnicity);
                            select.Education = JsonConvert.SerializeObject(settingsData.Education);
                            select.WorkStatus = JsonConvert.SerializeObject(settingsData.WorkStatus);
                            select.Relationship = JsonConvert.SerializeObject(settingsData.Relationship);
                            select.Language = JsonConvert.SerializeObject(settingsData.Language);
                            select.CustomFields = JsonConvert.SerializeObject(settingsData.CustomFields);

                            connection.Update(select);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateSettings(settingsData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Settings
        public GetOptionsObject.DataOptions GetSettings()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                if (select != null)
                {
                    var db = ClassMapper.Mapper?.Map<GetOptionsObject.DataOptions>(select);
                    if (db != null)
                    {
                        GetOptionsObject.DataOptions asd = db;
                        asd.Height = new List<Dictionary<string, string>>();
                        asd.Notification = new List<Dictionary<string, string>>();
                        asd.Gender = new List<Dictionary<string, string>>();
                        asd.BlogCategories = new List<Dictionary<string, string>>();
                        asd.Countries = new List<Dictionary<string, GetOptionsObject.Country>>();
                        asd.HairColor = new List<Dictionary<string, string>>();
                        asd.Travel = new List<Dictionary<string, string>>();
                        asd.Drink = new List<Dictionary<string, string>>();
                        asd.Smoke = new List<Dictionary<string, string>>();
                        asd.Religion = new List<Dictionary<string, string>>();
                        asd.Car = new List<Dictionary<string, string>>();
                        asd.LiveWith = new List<Dictionary<string, string>>();
                        asd.Pets = new List<Dictionary<string, string>>();
                        asd.Friends = new List<Dictionary<string, string>>();
                        asd.Children = new List<Dictionary<string, string>>();
                        asd.Character = new List<Dictionary<string, string>>();
                        asd.Body = new List<Dictionary<string, string>>();
                        asd.Ethnicity = new List<Dictionary<string, string>>();
                        asd.Education = new List<Dictionary<string, string>>();
                        asd.WorkStatus = new List<Dictionary<string, string>>();
                        asd.Relationship = new List<Dictionary<string, string>>();
                        asd.Language = new List<Dictionary<string, string>>();
                        asd.CustomFields = new List<GetOptionsObject.CustomField>();

                        if (!string.IsNullOrEmpty(select.Height))
                            asd.Height = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Height);

                        if (!string.IsNullOrEmpty(select.Notification))
                            asd.Notification = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Notification);

                        if (!string.IsNullOrEmpty(select.BlogCategories))
                            asd.BlogCategories = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.BlogCategories);

                        if (!string.IsNullOrEmpty(select.Gender))
                            asd.Gender = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Gender);

                        if (!string.IsNullOrEmpty(select.Countries))
                            asd.Countries = JsonConvert.DeserializeObject<List<Dictionary<string, GetOptionsObject.Country>>>(select.Countries);

                        if (!string.IsNullOrEmpty(select.HairColor))
                            asd.HairColor = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.HairColor);

                        if (!string.IsNullOrEmpty(select.Travel))
                            asd.Travel = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Travel);

                        if (!string.IsNullOrEmpty(select.Drink))
                            asd.Drink = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Drink);

                        if (!string.IsNullOrEmpty(select.Smoke))
                            asd.Smoke = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Smoke);

                        if (!string.IsNullOrEmpty(select.Religion))
                            asd.Religion = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Religion);

                        if (!string.IsNullOrEmpty(select.Car))
                            asd.Car = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Car);

                        if (!string.IsNullOrEmpty(select.LiveWith))
                            asd.LiveWith = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.LiveWith);

                        if (!string.IsNullOrEmpty(select.Pets))
                            asd.Pets = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Pets);

                        if (!string.IsNullOrEmpty(select.Friends))
                            asd.Friends = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Friends);

                        if (!string.IsNullOrEmpty(select.Children))
                            asd.Children = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Children);

                        if (!string.IsNullOrEmpty(select.Character))
                            asd.Character = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Character);

                        if (!string.IsNullOrEmpty(select.Body))
                            asd.Body = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Body);

                        if (!string.IsNullOrEmpty(select.Ethnicity))
                            asd.Ethnicity = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Ethnicity);

                        if (!string.IsNullOrEmpty(select.Education))
                            asd.Education = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Education);

                        if (!string.IsNullOrEmpty(select.WorkStatus))
                            asd.WorkStatus = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.WorkStatus);

                        if (!string.IsNullOrEmpty(select.Relationship))
                            asd.Relationship = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Relationship);

                        if (!string.IsNullOrEmpty(select.Language))
                            asd.Language = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(select.Language);

                        if (!string.IsNullOrEmpty(select.CustomFields))
                            asd.CustomFields = JsonConvert.DeserializeObject<List<GetOptionsObject.CustomField>>(select.CustomFields);

                        AppSettings.OneSignalAppId = asd.PushId;
                        OneSignalNotification.Instance.RegisterNotificationDevice();

                        ListUtils.SettingsSiteList = db;

                        return db;
                    }
                }
                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettings();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region My Info Data

        //Insert Or Update data MyInfo 
        public void InsertOrUpdate_DataMyInfo(UserInfoObject info)
        {
            try
            {
                using var connection = OpenConnection();
                var resultInfoTb = connection.Table<DataTables.InfoUsersTb>().FirstOrDefault();
                if (resultInfoTb != null)
                {
                    resultInfoTb = ClassMapper.Mapper?.Map<DataTables.InfoUsersTb>(info);
                    //resultInfoTb.Avater = JsonConvert.SerializeObject(info.Avater);
                    resultInfoTb.ProfileCompletionMissing = JsonConvert.SerializeObject(info.ProfileCompletionMissing);
                    resultInfoTb.Mediafiles = JsonConvert.SerializeObject(info.Mediafiles);
                    resultInfoTb.Likes = JsonConvert.SerializeObject(info.Likes);
                    resultInfoTb.Blocks = JsonConvert.SerializeObject(info.Blocks);
                    resultInfoTb.Payments = JsonConvert.SerializeObject(info.Payments);
                    resultInfoTb.Reports = JsonConvert.SerializeObject(info.Reports);
                    resultInfoTb.Visits = JsonConvert.SerializeObject(info.Visits);
                    resultInfoTb.AffPayments = JsonConvert.SerializeObject(info.AffPayments);
                    resultInfoTb.Referrals = JsonConvert.SerializeObject(info.Referrals);
                    connection.Update(resultInfoTb);
                }
                else
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.InfoUsersTb>(info);
                    //db.Avater = JsonConvert.SerializeObject(info.Avater);
                    db.ProfileCompletionMissing = JsonConvert.SerializeObject(info.ProfileCompletionMissing);
                    db.Mediafiles = JsonConvert.SerializeObject(info.Mediafiles);
                    db.Likes = JsonConvert.SerializeObject(info.Likes);
                    db.Blocks = JsonConvert.SerializeObject(info.Blocks);
                    db.Payments = JsonConvert.SerializeObject(info.Payments);
                    db.Reports = JsonConvert.SerializeObject(info.Reports);
                    db.Visits = JsonConvert.SerializeObject(info.Visits);
                    db.AffPayments = JsonConvert.SerializeObject(info.AffPayments);
                    db.Referrals = JsonConvert.SerializeObject(info.Referrals);
                    connection.Insert(db);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_DataMyInfo(info);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get Data My Info
        public UserInfoObject GetDataMyInfo()
        {
            try
            {
                using var connection = OpenConnection();
                DataTables.InfoUsersTb myInfo = connection.Table<DataTables.InfoUsersTb>().FirstOrDefault();
                if (myInfo != null)
                {
                    UserInfoObject infoObject = new UserInfoObject
                    {
                        Id = myInfo.Id,
                        Username = myInfo.Username,
                        Email = myInfo.Email,
                        FirstName = myInfo.FirstName,
                        LastName = myInfo.LastName,
                        //Avater = new AvaterUnion(),
                        Address = myInfo.Address,
                        Gender = myInfo.Gender,
                        Facebook = myInfo.Facebook,
                        Google = myInfo.Google,
                        Twitter = myInfo.Twitter,
                        Linkedin = myInfo.Linkedin,
                        Website = myInfo.Website,
                        Instagram = myInfo.Instagram,
                        WebDeviceId = myInfo.WebDeviceId,
                        Language = myInfo.Language,
                        Src = myInfo.Src,
                        IpAddress = myInfo.IpAddress,
                        Type = myInfo.Type,
                        PhoneNumber = myInfo.PhoneNumber,
                        Timezone = myInfo.Timezone,
                        Lat = myInfo.Lat,
                        Lng = myInfo.Lng,
                        About = myInfo.About,
                        Birthday = myInfo.Birthday,
                        Country = myInfo.Country,
                        Registered = myInfo.Registered,
                        Lastseen = myInfo.Lastseen,
                        Smscode = myInfo.Smscode,
                        ProTime = myInfo.ProTime,
                        LastLocationUpdate = myInfo.LastLocationUpdate,
                        Balance = myInfo.Balance,
                        Verified = myInfo.Verified,
                        Status = myInfo.Status,
                        Active = myInfo.Active,
                        Admin = myInfo.Admin,
                        StartUp = myInfo.StartUp,
                        IsPro = myInfo.IsPro,
                        ProType = myInfo.ProType,
                        SocialLogin = myInfo.SocialLogin,
                        CreatedAt = myInfo.CreatedAt,
                        UpdatedAt = myInfo.UpdatedAt,
                        DeletedAt = myInfo.DeletedAt,
                        MobileDeviceId = myInfo.MobileDeviceId,
                        WebToken = myInfo.WebToken,
                        MobileToken = myInfo.MobileToken,
                        Height = myInfo.Height,
                        HairColor = myInfo.HairColor,
                        WebTokenCreatedAt = myInfo.WebTokenCreatedAt,
                        MobileTokenCreatedAt = myInfo.MobileTokenCreatedAt,
                        MobileDevice = myInfo.MobileDevice,
                        Interest = myInfo.Interest,
                        Location = myInfo.Location,
                        Relationship = myInfo.Relationship,
                        WorkStatus = myInfo.WorkStatus,
                        Education = myInfo.Education,
                        Ethnicity = myInfo.Ethnicity,
                        Body = myInfo.Body,
                        Character = myInfo.Character,
                        Children = myInfo.Children,
                        Friends = myInfo.Friends,
                        Pets = myInfo.Pets,
                        LiveWith = myInfo.LiveWith,
                        Car = myInfo.Car,
                        Religion = myInfo.Religion,
                        Smoke = myInfo.Smoke,
                        Drink = myInfo.Drink,
                        Travel = myInfo.Travel,
                        Music = myInfo.Music,
                        Dish = myInfo.Dish,
                        Song = myInfo.Song,
                        Hobby = myInfo.Hobby,
                        City = myInfo.City,
                        Sport = myInfo.Sport,
                        Book = myInfo.Book,
                        Movie = myInfo.Movie,
                        Colour = myInfo.Colour,
                        Tv = myInfo.Tv,
                        PrivacyShowProfileOnGoogle = myInfo.PrivacyShowProfileOnGoogle,
                        PrivacyShowProfileRandomUsers = myInfo.PrivacyShowProfileRandomUsers,
                        PrivacyShowProfileMatchProfiles = myInfo.PrivacyShowProfileMatchProfiles,
                        EmailOnProfileView = myInfo.EmailOnProfileView,
                        EmailOnNewMessage = myInfo.EmailOnNewMessage,
                        EmailOnProfileLike = myInfo.EmailOnProfileLike,
                        EmailOnPurchaseNotifications = myInfo.EmailOnPurchaseNotifications,
                        EmailOnSpecialOffers = myInfo.EmailOnSpecialOffers,
                        EmailOnAnnouncements = myInfo.EmailOnAnnouncements,
                        PhoneVerified = myInfo.PhoneVerified,
                        Online = myInfo.Online,
                        IsBoosted = myInfo.IsBoosted,
                        BoostedTime = myInfo.BoostedTime,
                        IsBuyStickers = myInfo.IsBuyStickers,
                        UserBuyXvisits = myInfo.UserBuyXvisits,
                        XvisitsCreatedAt = myInfo.XvisitsCreatedAt,
                        UserBuyXmatches = myInfo.UserBuyXmatches,
                        XmatchesCreatedAt = myInfo.XmatchesCreatedAt,
                        UserBuyXlikes = myInfo.UserBuyXlikes,
                        XlikesCreatedAt = myInfo.XlikesCreatedAt,
                        VerifiedFinal = myInfo.VerifiedFinal,
                        FullName = myInfo.FullName,
                        Age = myInfo.Age,
                        LastseenTxt = myInfo.LastseenTxt,
                        LastseenDate = myInfo.LastseenDate,
                        IsOwner = myInfo.IsOwner,
                        IsLiked = myInfo.IsLiked,
                        IsBlocked = myInfo.IsBlocked,
                        ProfileCompletion = myInfo.ProfileCompletion,
                        CountryTxt = myInfo.CountryTxt,
                        GenderTxt = myInfo.GenderTxt,
                        LanguageTxt = myInfo.LanguageTxt,
                        EmailCode = myInfo.EmailCode,
                        HeightTxt = myInfo.HeightTxt,
                        HairColorTxt = myInfo.HairColorTxt,
                        RelationshipTxt = myInfo.RelationshipTxt,
                        WorkStatusTxt = myInfo.WorkStatusTxt,
                        EducationTxt = myInfo.EducationTxt,
                        EthnicityTxt = myInfo.EthnicityTxt,
                        BodyTxt = myInfo.BodyTxt,
                        CharacterTxt = myInfo.CharacterTxt,
                        ChildrenTxt = myInfo.ChildrenTxt,
                        FriendsTxt = myInfo.FriendsTxt,
                        PetsTxt = myInfo.PetsTxt,
                        LiveWithTxt = myInfo.LiveWithTxt,
                        CarTxt = myInfo.CarTxt,
                        ReligionTxt = myInfo.ReligionTxt,
                        SmokeTxt = myInfo.SmokeTxt,
                        DrinkTxt = myInfo.DrinkTxt,
                        TravelTxt = myInfo.TravelTxt,
                        ShowMeTo = myInfo.ShowMeTo,
                        EmailOnGetGift = myInfo.EmailOnGetGift,
                        EmailOnGotNewMatch = myInfo.EmailOnGotNewMatch,
                        EmailOnChatRequest = myInfo.EmailOnChatRequest,
                        LastEmailSent = myInfo.LastEmailSent,
                        ActivationRequestCount = myInfo.ActivationRequestCount,
                        AffBalance = myInfo.AffBalance,
                        ApprovedAt = myInfo.ApprovedAt,
                        ConfirmFollowers = myInfo.ConfirmFollowers,
                        FullPhoneNumber = myInfo.FullPhoneNumber,
                        HotCount = myInfo.HotCount,
                        IsFriend = myInfo.IsFriend,
                        IsFriendRequest = myInfo.IsFriendRequest,
                        LastActivationRequest = myInfo.LastActivationRequest,
                        NewEmail = myInfo.NewEmail,
                        NewPhone = myInfo.NewPhone,
                        Password = myInfo.Password,
                        PaypalEmail = myInfo.PaypalEmail,
                        Permission = myInfo.Permission,
                        Referrer = myInfo.Referrer,
                        Snapshot = myInfo.Snapshot,
                        SpamWarning = myInfo.SpamWarning,
                        TwoFactor = myInfo.TwoFactor,
                        TwoFactorEmailCode = myInfo.TwoFactorEmailCode,
                        TwoFactorVerified = myInfo.TwoFactorVerified,
                        LikesCount = myInfo.LikesCount,
                        VisitsCount = myInfo.VisitsCount,
                        ProfileCompletionMissing = new List<string>(),
                        Mediafiles = new List<MediaFile>(),
                        Likes = new List<Like>(),
                        Blocks = new List<Block>(),
                        Payments = new List<Payment>(),
                        Reports = new List<Report>(),
                        Visits = new List<Visit>(),
                        AffPayments = new List<AffPayment>(),
                        Referrals = new List<UserInfoObject>(),
                    };

                    infoObject.ProfileCompletionMissing = JsonConvert.DeserializeObject<List<string>>(myInfo.ProfileCompletionMissing);
                    infoObject.Mediafiles = JsonConvert.DeserializeObject<List<MediaFile>>(myInfo.Mediafiles);
                    infoObject.Likes = JsonConvert.DeserializeObject<List<Like>>(myInfo.Likes);
                    infoObject.Blocks = JsonConvert.DeserializeObject<List<Block>>(myInfo.Blocks);
                    infoObject.Payments = JsonConvert.DeserializeObject<List<Payment>>(myInfo.Payments);
                    infoObject.Reports = JsonConvert.DeserializeObject<List<Report>>(myInfo.Reports);
                    infoObject.Visits = JsonConvert.DeserializeObject<List<Visit>>(myInfo.Visits);
                    infoObject.AffPayments = JsonConvert.DeserializeObject<List<AffPayment>>(myInfo.AffPayments);
                    infoObject.Referrals = JsonConvert.DeserializeObject<List<UserInfoObject>>(myInfo.Referrals);
                    //infoObject.Avater = new AvaterUnion()
                    //{
                    //    PurpleUri = JsonConvert.DeserializeObject<string>(myInfo.Avater)
                    //};

                    ListUtils.MyUserInfo.Clear();
                    ListUtils.MyUserInfo.Add(infoObject);

                    return infoObject;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetDataMyInfo();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region Gifts

        //Insert data Gifts
        public void InsertAllGifts(ObservableCollection<DataFile> listData)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.GiftsTb>().ToList();
                List<DataTables.GiftsTb> list = new List<DataTables.GiftsTb>();
                foreach (var gift in listData)
                {
                    var item = new DataTables.GiftsTb
                    {
                        IdGifts = gift.Id,
                        File = gift.File,
                    };
                    list.Add(item);

                    var update = result.FirstOrDefault(a => a.IdGifts == gift.Id);
                    if (update != null)
                    {
                        update = item;
                        connection.Update(update);
                    }
                }

                if (list.Count > 0)
                {
                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.IdGifts).Contains(c.IdGifts)).ToList();
                    if (newItemList.Count > 0)
                    {
                        connection.InsertAll(newItemList);
                    }

                    result = connection.Table<DataTables.GiftsTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.IdGifts).Contains(c.IdGifts)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);

                    connection.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertAllGifts(listData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get List Gifts 
        public ObservableCollection<DataFile> GetGiftsList()
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.GiftsTb>().ToList();
                if (result?.Count > 0)
                {
                    List<DataFile> list = result.Select(gift => new DataFile
                    {
                        Id = gift.IdGifts,
                        File = gift.File,
                    }).ToList();

                    return new ObservableCollection<DataFile>(list);
                }
                else
                {
                    return new ObservableCollection<DataFile>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetGiftsList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<DataFile>();
                } 
            }
        }

        #endregion

        #region Stickers

        //Insert data Stickers
        public void InsertAllStickers(ObservableCollection<DataFile> listData)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.StickersTb>().ToList();
                List<DataTables.StickersTb> list = new List<DataTables.StickersTb>();
                foreach (var stickers in listData)
                {
                    var item = new DataTables.StickersTb
                    {
                        IdStickers = stickers.Id,
                        File = stickers.File,
                    };
                    list.Add(item);

                    var update = result.FirstOrDefault(a => a.IdStickers == stickers.Id);
                    if (update != null)
                    {
                        update = item;
                        connection.Update(update);
                    }
                }

                if (list.Count > 0)
                {
                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.IdStickers).Contains(c.IdStickers)).ToList();
                    if (newItemList.Count > 0)
                    {
                        connection.InsertAll(newItemList);
                    }

                    result = connection.Table<DataTables.StickersTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.IdStickers).Contains(c.IdStickers)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);

                    connection.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertAllStickers(listData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get List Stickers 
        public ObservableCollection<DataFile> GetStickersList()
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.StickersTb>().ToList();
                if (result?.Count > 0)
                {
                    List<DataFile> list = result.Select(stickers => new DataFile
                    {
                        Id = stickers.IdStickers,
                        File = stickers.File,
                    }).ToList();

                    return new ObservableCollection<DataFile>(list);
                }
                else
                {
                    return new ObservableCollection<DataFile>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetStickersList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<DataFile>();
                } 
            }
        }

        #endregion

        #region Last Chat

        //Insert data To Last Chat Table
        public void InsertOrReplaceLastChatTable(ObservableCollection<GetConversationListObject.DataConversation> usersContactList)
        {
            try
            {
                using var connection = OpenConnection();
                var result = connection.Table<DataTables.LastChatTb>().ToList();
                List<DataTables.LastChatTb> list = new List<DataTables.LastChatTb>();
                foreach (var user in usersContactList)
                {
                    var item = new DataTables.LastChatTb
                    {
                        Id = user.Id,
                        Owner = user.Owner,
                        Seen = user.Seen,
                        Text = user.Text,
                        Media = user.Media,
                        Sticker = user.Sticker,
                        Time = user.Time,
                        CreatedAt = user.CreatedAt,
                        UserId = user.User.Id.ToString(),
                        NewMessages = user.NewMessages,
                        MessageType = user.MessageType,
                        UserDataJson = JsonConvert.SerializeObject(user.User),
                    };

                    if (user.ToId != null) item.ToId = user.ToId.Value.ToString();
                    if (user.FromId != null) item.FromId = user.FromId.Value.ToString();

                    list.Add(item);

                    var update = result.FirstOrDefault(a => a.Id == user.Id);
                    if (update != null)
                    {
                        update = item;
                        if (user.User != null)
                            update.UserDataJson = JsonConvert.SerializeObject(user.User);

                        connection.Update(update);
                    }
                }

                if (list.Count > 0)
                {
                    connection.BeginTransaction();
                    //Bring new  
                    var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (newItemList.Count > 0)
                    {
                        connection.InsertAll(newItemList);
                    }

                    result = connection.Table<DataTables.LastChatTb>().ToList();
                    var deleteItemList = result.Where(c => !list.Select(fc => fc.Id).Contains(c.Id)).ToList();
                    if (deleteItemList.Count > 0)
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);

                    connection.Commit();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceLastChatTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To LastChat Table
        public ObservableCollection<GetConversationListObject.DataConversation> GetAllLastChat()
        {
            try
            {
                using var connection = OpenConnection();
                var select = connection.Table<DataTables.LastChatTb>().ToList();
                if (select.Count > 0)
                {
                    List<GetConversationListObject.DataConversation> list = new List<GetConversationListObject.DataConversation>();
                    foreach (var user in select)
                    {
                        var item = new GetConversationListObject.DataConversation
                        {
                            Id = user.Id,
                            Owner = user.Owner,
                            Seen = user.Seen,
                            Text = user.Text,
                            Media = user.Media,
                            Sticker = user.Sticker,
                            Time = user.Time,
                            CreatedAt = user.CreatedAt,
                            User = new UserInfoObject(),
                            NewMessages = user.NewMessages,
                            MessageType = user.MessageType
                        };

                        if (!string.IsNullOrEmpty(user.ToId)) item.ToId = Convert.ToInt32(user.ToId);
                        if (!string.IsNullOrEmpty(user.FromId)) item.FromId = Convert.ToInt32(user.FromId);

                        if (user.UserDataJson != null)
                            item.User = JsonConvert.DeserializeObject<UserInfoObject>(user.UserDataJson);

                        list.Add(item);
                    }
                    return new ObservableCollection<GetConversationListObject.DataConversation>(list);
                }
                else
                    return new ObservableCollection<GetConversationListObject.DataConversation>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetAllLastChat();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<GetConversationListObject.DataConversation>();
                } 
            }
        }

        // Get data To LastChat Table By Id >> Load More
        public ObservableCollection<GetConversationListObject.DataConversation> GetLastChatById(int id, int nSize)
        {
            try
            {
                using var connection = OpenConnection();
                var query = connection.Table<DataTables.LastChatTb>().Where(w => w.AutoIdLastChat >= id)
                    .OrderBy(q => q.AutoIdLastChat).Take(nSize).ToList();
                if (query.Count > 0)
                {
                    var list = query.Select(user => new GetConversationListObject.DataConversation
                    {
                        Id = user.Id,
                        Owner = user.Owner,
                        Seen = user.Seen,
                        Text = user.Text,
                        Media = user.Media,
                        Sticker = user.Sticker,
                        Time = user.Time,
                        CreatedAt = user.CreatedAt,
                        User = JsonConvert.DeserializeObject<UserInfoObject>(user.UserDataJson),
                        NewMessages = user.NewMessages,
                        MessageType = user.MessageType,
                    }).ToList();

                    if (list.Count > 0)
                        return new ObservableCollection<GetConversationListObject.DataConversation>(list);
                    else
                        return new ObservableCollection<GetConversationListObject.DataConversation>();
                }
                else
                    return new ObservableCollection<GetConversationListObject.DataConversation>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetLastChatById(id, nSize);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<GetConversationListObject.DataConversation>();
                } 
            }
        }

        //Remove data To LastChat Table
        public void DeleteUserLastChat(string userId)
        {
            try
            {
                using var connection = OpenConnection();
                var user = connection.Table<DataTables.LastChatTb>().FirstOrDefault(c => c.UserId.ToString() == userId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteUserLastChat(userId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Clear All data LastChat
        public void ClearLastChat()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DeleteAll<DataTables.LastChatTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearLastChat();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Message

        //Insert data To Message Table
        public void InsertOrReplaceMessages(ObservableCollection<GetChatConversationsObject.Messages> messageList)
        {
            try
            {
                using var connection = OpenConnection();
                List<DataTables.MessageTb> listOfDatabaseForInsert = new List<DataTables.MessageTb>();

                // get data from database
                var resultMessage = connection.Table<DataTables.MessageTb>().ToList();
                var listAllMessage = resultMessage.Select(messages => new GetChatConversationsObject.Messages
                {
                    Id = messages.Id,
                    FromName = messages.FromName,
                    FromAvater = messages.FromAvater,
                    ToName = messages.ToName,
                    ToAvater = messages.ToAvater,
                    From = messages.FromId,
                    To = messages.ToId,
                    Text = messages.Text,
                    Media = messages.Media,
                    FromDelete = messages.FromDelete,
                    ToDelete = messages.ToDelete,
                    Sticker = messages.Sticker,
                    CreatedAt = messages.CreatedAt,
                    Seen = messages.Seen,
                    Type = messages.Type,
                    MessageType = messages.MessageType,
                }).ToList();

                foreach (var messages in messageList)
                {
                    DataTables.MessageTb maTb = new DataTables.MessageTb
                    {
                        Id = messages.Id,
                        FromName = messages.FromName,
                        FromAvater = messages.FromAvater,
                        ToName = messages.ToName,
                        ToAvater = messages.ToAvater,
                        FromId = messages.From,
                        ToId = messages.To,
                        Text = messages.Text,
                        Media = messages.Media,
                        FromDelete = messages.FromDelete,
                        ToDelete = messages.ToDelete,
                        Sticker = messages.Sticker,
                        CreatedAt = messages.CreatedAt,
                        Seen = messages.Seen,
                        Type = messages.Type,
                        MessageType = messages.MessageType,
                    };

                    var dataCheck = listAllMessage.FirstOrDefault(a => a.Id == messages.Id);
                    if (dataCheck != null)
                    {
                        var checkForUpdate = resultMessage.FirstOrDefault(a => a.Id == dataCheck.Id);
                        if (checkForUpdate != null)
                        {
                            checkForUpdate.Id = messages.Id;
                            checkForUpdate.FromName = messages.FromName;
                            checkForUpdate.FromAvater = messages.FromAvater;
                            checkForUpdate.ToName = messages.ToName;
                            checkForUpdate.ToAvater = messages.ToAvater;
                            checkForUpdate.FromId = messages.From;
                            checkForUpdate.ToId = messages.To;
                            checkForUpdate.Text = messages.Text;
                            checkForUpdate.Media = messages.Media;
                            checkForUpdate.FromDelete = messages.FromDelete;
                            checkForUpdate.ToDelete = messages.ToDelete;
                            checkForUpdate.Sticker = messages.Sticker;
                            checkForUpdate.CreatedAt = messages.CreatedAt;
                            checkForUpdate.Seen = messages.Seen;
                            checkForUpdate.Type = messages.Type;
                            checkForUpdate.MessageType = messages.MessageType;

                            connection.Update(checkForUpdate);
                        }
                        else
                        {
                            listOfDatabaseForInsert.Add(maTb);
                        }
                    }
                    else
                    {
                        listOfDatabaseForInsert.Add(maTb);
                    }
                }

                connection.BeginTransaction();

                //Bring new  
                if (listOfDatabaseForInsert.Count > 0)
                {
                    connection.InsertAll(listOfDatabaseForInsert);
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrReplaceMessages(messageList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Update one Messages Table
        public void InsertOrUpdateToOneMessages(GetChatConversationsObject.Messages message)
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.MessageTb>().FirstOrDefault(a => a.Id == message.Id);
                if (data != null)
                {
                    data.Id = message.Id;
                    data.FromName = message.FromName;
                    data.FromAvater = message.FromAvater;
                    data.ToName = message.ToName;
                    data.ToAvater = message.ToAvater;
                    data.FromId = message.From;
                    data.ToId = message.To;
                    data.Text = message.Text;
                    data.Media = message.Media;
                    data.FromDelete = message.FromDelete;
                    data.ToDelete = message.ToDelete;
                    data.Sticker = message.Sticker;
                    data.CreatedAt = message.CreatedAt;
                    data.Seen = message.Seen;
                    data.Type = message.Type;
                    data.MessageType = message.MessageType;
                    connection.Update(data);
                }
                else
                {
                    DataTables.MessageTb mdb = new DataTables.MessageTb
                    {
                        Id = message.Id,
                        FromName = message.FromName,
                        FromAvater = message.FromAvater,
                        ToName = message.ToName,
                        ToAvater = message.ToAvater,
                        FromId = message.From,
                        ToId = message.To,
                        Text = message.Text,
                        Media = message.Media,
                        FromDelete = message.FromDelete,
                        ToDelete = message.ToDelete,
                        Sticker = message.Sticker,
                        CreatedAt = message.CreatedAt,
                        Seen = message.Seen,
                        Type = message.Type,
                        MessageType = message.MessageType,
                    };

                    //Insert  one Messages Table
                    connection.Insert(mdb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateToOneMessages(message);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data To Messages
        public string GetMessagesList(long fromId, long toId, long beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                var beforeQ = "";
                if (beforeMessageId != 0)
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query = connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);
                List<DataTables.MessageTb> queryList = query.Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId).OrderBy(q => q.CreatedAt).TakeLast(35).ToList();
                if (queryList.Count > 0)
                {
                    foreach (var m in queryList.Select(message => new GetChatConversationsObject.Messages
                    {
                        Id = message.Id,
                        FromName = message.FromName,
                        FromAvater = message.FromAvater,
                        ToName = message.ToName,
                        ToAvater = message.ToAvater,
                        From = message.FromId,
                        To = message.ToId,
                        Text = message.Text,
                        Media = message.Media,
                        FromDelete = message.FromDelete,
                        ToDelete = message.ToDelete,
                        Sticker = message.Sticker,
                        CreatedAt = message.CreatedAt,
                        Seen = message.Seen,
                        Type = message.Type,
                        MessageType = message.MessageType,
                    }))
                    {
                        if (beforeMessageId == 0)
                        {
                            if (MessagesBoxActivity.MAdapter != null)
                            {
                                MessagesBoxActivity.MAdapter.MessageList.Add(m);

                                int index = MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.Last());
                                if (index > -1)
                                {
                                    MessagesBoxActivity.MAdapter.NotifyItemInserted(index);

                                    //Scroll Down >> 
                                    MessagesBoxActivity.GetInstance()?.ChatBoxRecyclerView.ScrollToPosition(index);
                                }
                            }
                        }
                        else
                        {
                            MessagesBoxActivity.MAdapter?.MessageList.Insert(0, m);
                            MessagesBoxActivity.MAdapter?.NotifyItemInserted(MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.FirstOrDefault()));

                            var index = MessagesBoxActivity.MAdapter?.MessageList.FirstOrDefault(a => a.Id == beforeMessageId);
                            if (index != null)
                            {
                                MessagesBoxActivity.MAdapter?.NotifyItemChanged(MessagesBoxActivity.MAdapter.MessageList.IndexOf(index));
                                //Scroll Down >> 
                                MessagesBoxActivity.GetInstance()?.ChatBoxRecyclerView.ScrollToPosition(MessagesBoxActivity.MAdapter.MessageList.IndexOf(MessagesBoxActivity.MAdapter.MessageList.Last()));

                            }
                        }
                    }

                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessagesList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return "0";
                } 
            }
        }

        //Get data To where first Messages >> load more
        public List<DataTables.MessageTb> GetMessageList(long fromId, long toId, long beforeMessageId)
        {
            try
            {
                using var connection = OpenConnection();
                var beforeQ = "";
                if (beforeMessageId != 0)
                {
                    beforeQ = "AND Id < " + beforeMessageId + " AND Id <> " + beforeMessageId + " ";
                }

                var query = connection.Query<DataTables.MessageTb>("SELECT * FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + ")) " + beforeQ);
                List<DataTables.MessageTb> queryList = query
                    .Where(w => w.FromId == fromId && w.ToId == toId || w.ToId == fromId && w.FromId == toId)
                    .OrderBy(q => q.CreatedAt).TakeLast(35).ToList();
                return queryList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetMessageList(fromId, toId, beforeMessageId);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new List<DataTables.MessageTb>();
                } 
            }
        }

        //Remove data To Messages Table
        public void Delete_OneMessageUser(int messageId)
        {
            try
            {
                using var connection = OpenConnection();
                var user = connection.Table<DataTables.MessageTb>().FirstOrDefault(c => c.Id == messageId);
                if (user != null)
                {
                    connection.Delete(user);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Delete_OneMessageUser(messageId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteAllMessagesUser(string fromId, string toId)
        {
            try
            {
                using var connection = OpenConnection();
                var query = connection.Query<DataTables.MessageTb>("Delete FROM MessageTb WHERE ((FromId =" + fromId + " and ToId=" + toId + ") OR (FromId =" + toId + " and ToId=" + fromId + "))");
                Console.WriteLine(query);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteAllMessagesUser(fromId, toId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove All data To Messages Table
        public void ClearAll_Messages()
        {
            try
            {
                using var connection = OpenConnection();
                connection.DeleteAll<DataTables.MessageTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    ClearAll_Messages();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}