using System;
using AutoMapper;
using QuickDate.SQLite;
using QuickDateClient.Classes.Common;
using QuickDateClient.Classes.Global;

namespace QuickDate.Helpers.Utils
{
    public static class ClassMapper
    {
        public static IMapper Mapper;
        public static void SetMappers()
        {
            try
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    try
                    {
                        cfg.AllowNullCollections = true;
                         
                        cfg.CreateMap<GetOptionsObject.DataOptions, DataTables.SettingsTb>().ForMember(x => x.AutoId, opt => opt.Ignore());
                        cfg.CreateMap<UserInfoObject, DataTables.InfoUsersTb>().ForMember(x => x.AutoId, opt => opt.Ignore());
                        cfg.CreateMap<DataFile, DataTables.GiftsTb>().ForMember(x => x.AutoId, opt => opt.Ignore());
                        cfg.CreateMap<DataFile, DataTables.StickersTb>().ForMember(x => x.AutoIdStickers, opt => opt.Ignore());
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
                // only during development, validate your mappings; remove it before release
                //configuration.AssertConfigurationIsValid();

                Mapper = configuration.CreateMapper();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
} 