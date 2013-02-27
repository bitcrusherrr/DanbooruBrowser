using booruReader.Helpers;
using booruReader.Model.Site_Parsers;
using System;
using System.Collections.Generic;

namespace booruReader.Model
{
    class PostsFetcher
    {
        //private string _currentBooruURL;
        private List<BasePost> _PostFetcherImageList;

        private JSONParser _jsonParser;
        private XMLParser _xmlParser;
        private SankakuParser _sankakuParser;

        //Temp hack to bypass some globals breaking
        bool _booruTestMode;

        public PostsFetcher(bool booruTestMode = false)
        {
            _PostFetcherImageList = new List<BasePost>();
            _booruTestMode = booruTestMode;

            _jsonParser = new JSONParser(_booruTestMode);
            _xmlParser = new XMLParser(_booruTestMode);
            _sankakuParser = new SankakuParser(_booruTestMode);
        }

        public List<BasePost> GetImages(int page, String tags = null)
        {
            _PostFetcherImageList.Clear();

            //Explicitly check for sankaku
            if (GlobalSettings.Instance.CurrentBooru.URL.ToLower().Contains("sankakucomplex"))
                _sankakuParser.GetImages(_PostFetcherImageList, tags, page);
            else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML || GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
                _xmlParser.GetImages(_PostFetcherImageList, tags, page);
            else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.JSON || GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
                _jsonParser.GetImages(_PostFetcherImageList, tags, page);

            return _PostFetcherImageList;
        }
    }
}
