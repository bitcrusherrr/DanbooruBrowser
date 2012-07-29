using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        }

        public List<BasePost> GetImages(String tags = null)
        {      
            _PostFetcherImnageList.Clear();

            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML || GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
                GetXMLImages(_PostFetcherImnageList, tags);
            else
                GetJSONImages(_PostFetcherImnageList, tags);

            return _PostFetcherImnageList;
        }

        #region XML Fetch routines
        private void GetXMLImages(List<BasePost> ImageList, string tags)
        {
            string finalURL = string.Empty;
            if (tags == null)
            {
                tags = string.Empty;
            }

            //NOTE: refactor
            //Manual exception for danbooru as it requires user for logging in
            if (GlobalSettings.Instance.CurrentBooru.URL.ToLowerInvariant().Contains(".donmai.us"))
            {
                finalURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.xml"; //+ tags from searchfield

                //danbooru HAS to be logged in to fetch shit which is pain in the ass
                finalURL = string.Format(finalURL + "?login=booruReader" + "&password_hash=70de755c930112801ef5e002aff10cfe4cafd76d");
                finalURL = string.Format(finalURL + "&page=" + GlobalSettings.Instance.CurrentPage + "&tags=" + FormTags(tags));
            }
            else
            {
                //Danbooru api based sites
                if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML)
                {
                    finalURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.xml"; //+ tags from searchfield
                    finalURL = string.Format(finalURL + "?page=" + GlobalSettings.Instance.CurrentPage + "&tags=" + FormTags(tags));
                }
                //Gelbooru api based sites
                else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
                {
                    finalURL = GlobalSettings.Instance.CurrentBooru.URL + "index.php?page=dapi&s=post&q=index";
                    finalURL = string.Format(finalURL + "&pid=" + GlobalSettings.Instance.CurrentPage + "&tags=" + FormTags(tags));
                }
            }

            GlobalSettings.Instance.CurrentPage++;

            try
            {
                XmlTextReader reader = new XmlTextReader(finalURL);
                
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
                                            GlobalSettings.Instance.TotalPosts = int.Parse(reader.Value);
                                        }

                                        if (reader.Name.ToLowerInvariant().Equals("offset")) // Posts Count
                                        {
                                            GlobalSettings.Instance.PostsOffset = int.Parse(reader.Value);
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
                                            case "file_url": post.FullPictureURL = NormaliseURL(reader.Value); break;
                                            case "preview_url": post.PreviewURL = NormaliseURL(reader.Value); break;
                                            case "md5": post.FileMD = reader.Value; break;
                                            case "tags": post.Tags = reader.Value; break;
                                            case "width": int.TryParse(reader.Value, out post._width); break;
                                            case "height": int.TryParse(reader.Value, out post._height); break;
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
                                        ImageList.Add(post);

                                }
                                break;
                    }
                }


            }
            catch (Exception exception)
            {
                exception.ToString();
                new MetroMessagebox("Error", exception.ToString()).ShowDialog();
            }

            if (GlobalSettings.Instance.PostsOffset > GlobalSettings.Instance.TotalPosts)
            {
                new MetroMessagebox("Fetch Error", "End of posts.").ShowDialog();
            }
        }

        #endregion

        #region JSON Fetch routines
        private void GetJSONImages(List<BasePost> _PostFetcherImnageList, string tags)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Helper Functions

        private string NormaliseURL(string url)
        {
            if (!url.Contains("http"))
            {
                url = GlobalSettings.Instance.CurrentBooru.URL + url;
            }

            return url;
        }

        private string FormTags(string tags)
        {
            string returnTags = tags;

            if(!string.IsNullOrEmpty(tags))
            {
                returnTags = tags.Replace(" ", "+");
            }

            return returnTags;
        }

        #endregion
    }
}
