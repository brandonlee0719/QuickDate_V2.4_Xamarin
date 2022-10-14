using System.Collections.Generic;
using QuickDateClient.Classes.Global;

namespace QuickDate.Helpers.Model
{
    public class Classes
    {
        public class Languages
        {
            public string LanguagesId { get; set; }
            public string LanguagesName { get; set; }
        }

        public class TrendingClass
        {
            public long Id { get; set; }
            public ItemType Type { get; set; }
            public List<UserInfoObject> ProUserList { get; set; }
            public List<UserInfoObject> HotOrNotList { get; set; }
            public UserInfoObject UsersData { get; set; }
        } 
    }

    public enum ItemType
    {
        ProUser = 100, HotOrNot = 200, Users = 300, EmptyPage = 400 
    }

    public enum PaymentsSystem
    {
        All = 1, JustInAppBillingGoogle = 2
    }


    public enum OpenGalleryDialogFor
    {
        Other,
        Avatar
    }


    public enum ShowAds
    {
        AllUsers,
        UnProfessional,
    }

}