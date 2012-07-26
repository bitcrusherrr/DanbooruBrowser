using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using booruReader.Helpers;

namespace booruReader.Model
{
    class PostsFetcher
    {
        //private string _currentBooruURL;
        private List<BasePost> _PostFetcherImnageList;

        public PostsFetcher()
        {
            _PostFetcherImnageList = new List<BasePost>();
            //_currentBooruURL = GlobalSettings.Instance.CurrentBooruURL;
            //using (XmlTextReader reader = new XmlTextReader("http://danbooru.donmai.us/post/index.xml"))
            {
                //reader.Read();
            } 
        }

        public List<BasePost> GetImages(String tags = null)
        {
            string finalURL = GlobalSettings.Instance.CurrentBooru.URL; //+ tags from searchfield

            finalURL = string.Format(finalURL + "?page=" + GlobalSettings.Instance.CurrentPage);

            if (tags != null)
            {
                finalURL = string.Format(finalURL + "?page=" + GlobalSettings.Instance.CurrentPage + "&tags=" + tags);
            }
            
            GlobalSettings.Instance.CurrentPage++;

            XmlTextReader reader = new XmlTextReader(finalURL);
            
            try
            {                    
                _PostFetcherImnageList.Clear();
                while (reader.Read())
                {

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                                var nodeName = reader.Name.ToLowerInvariant();
                                if (nodeName.Equals("posts"))
                                {
                                    while (reader.MoveToNextAttribute())
                                    {
                                        if (reader.Name.ToLowerInvariant().Equals("count")) // Posts Count
                                        {
                                            //postCount = int.Parse(reader.Value);
                                            //RawData += "postCount:" + postCount;
                                        }
                                    }
                                }
                                else if (nodeName.Equals("post"))
                                {
                                    BasePost post = new BasePost();

                                    while (reader.MoveToNextAttribute())
                                    {
                                        switch (reader.Name.ToLowerInvariant())
                                        {
                                            case "file_url": post.FullPictureURL = reader.Value; break;
                                            case "preview_url": post.PreviewURL = reader.Value; break;
                                            case "rating":
                                                {
                                                    if (reader.Value.Contains("s"))
                                                        post.ImageRating = PostRating.Safe;
                                                    else if (reader.Value.Contains("q"))
                                                        post.ImageRating = PostRating.Explicit;
                                                    else
                                                        post.ImageRating = PostRating.Questionable;
                                                }
                                                break;
                                        }
                                    }

                                    if (GlobalSettings.Instance.IsSafeMode && post.ImageRating != PostRating.Safe)
                                    {
                                        //Do nothing for now
                                        //TODO: add more UI level filtering later!
                                    }
                                    else
                                        _PostFetcherImnageList.Add(post);

                                }
                                break;
                    }
                }


            }
            catch (Exception exception)
            {
                exception.ToString();
            }

            return _PostFetcherImnageList;
        }
    }
}
