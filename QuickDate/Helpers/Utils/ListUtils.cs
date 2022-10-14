using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using QuickDate.Helpers.Model;
using QuickDate.SQLite;
using QuickDateClient.Classes.Blogs;
using QuickDateClient.Classes.Chat;
using QuickDateClient.Classes.Common;
using QuickDateClient.Classes.Global;

namespace QuickDate.Helpers.Utils
{
    public static class ListUtils
    {
        //############# DON'T MODIFY HERE #############
        //List Items Declaration 
        //*********************************************************
        public static ObservableCollection<DataTables.LoginTb> DataUserLoginList = new ObservableCollection<DataTables.LoginTb>();
        public static GetOptionsObject.DataOptions SettingsSiteList;
        public static ObservableCollection<Classes.Languages> LanguagesSiteList = new ObservableCollection<Classes.Languages>();
        public static ObservableCollection<UserInfoObject> MyUserInfo = new ObservableCollection<UserInfoObject>();
        public static ObservableCollection<DataFile> GiftsList = new ObservableCollection<DataFile>();
        public static ObservableCollection<DataFile> StickersList = new ObservableCollection<DataFile>();
        public static ObservableCollection<UserInfoObject> AllMatchesList = new ObservableCollection<UserInfoObject>();
        public static readonly ObservableCollection<UserInfoObject> LikedList = new ObservableCollection<UserInfoObject>();
        public static ObservableCollection<UserInfoObject> DisLikedList = new ObservableCollection<UserInfoObject>();
        public static ObservableCollection<UserInfoObject> OldMatchesList = new ObservableCollection<UserInfoObject>();
        public static ObservableCollection<GetNotificationsObject.Datum> MatchList = new ObservableCollection<GetNotificationsObject.Datum>();
        public static ObservableCollection<GetNotificationsObject.Datum> VisitsList = new ObservableCollection<GetNotificationsObject.Datum>();
        public static ObservableCollection<GetNotificationsObject.Datum> LikesList = new ObservableCollection<GetNotificationsObject.Datum>();
        public static ObservableCollection<GetNotificationsObject.Datum> RequestsList = new ObservableCollection<GetNotificationsObject.Datum>();
        public static ObservableCollection<GetConversationListObject.DataConversation> ChatList = new ObservableCollection<GetConversationListObject.DataConversation>();
        public static ObservableCollection<ArticleDataObject> ListCachedDataArticle = new ObservableCollection<ArticleDataObject>();
        public static ObservableCollection<UserInfoObject> FriendRequestsList = new ObservableCollection<UserInfoObject>();

        public static void ClearAllList()
        {
            try
            {
                DataUserLoginList.Clear();
                SettingsSiteList = null!;
                LanguagesSiteList.Clear();
                MyUserInfo.Clear();
                GiftsList.Clear();
                StickersList.Clear();
                LikedList.Clear();
                DisLikedList.Clear();
                OldMatchesList.Clear();
                MatchList.Clear();
                VisitsList.Clear();
                LikesList.Clear();
                ChatList.Clear();
                AllMatchesList.Clear();
                FriendRequestsList.Clear();
            }
            catch (Exception e)
            {
               Methods.DisplayReportResultTrack(e);
            }
        }

        public static void AddRange<T>(ObservableCollection<T> collection, IEnumerable<T> items)
        {
            try
            {
                items.ToList().ForEach(collection.Add);
            }
            catch (Exception e)
            {
               Methods.DisplayReportResultTrack(e);
            }
        }

        public static List<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            var list = new List<List<T>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static IEnumerable<T> TakeLast<T>(IEnumerable<T> source, int n)
        {
            var enumerable = source as T[] ?? source.ToArray();

            return enumerable.Skip(Math.Max(0, enumerable.Count() - n));
        }

    }
}