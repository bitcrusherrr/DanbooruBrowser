using booruReader.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace booruReader.Model
{
    class PostsFetcher
    {
        //private string _currentBooruURL;
        private List<BasePost> _PostFetcherImnageList;

        //Temp hack to bypass some globals breaking
        bool _booruTestMode;

        public PostsFetcher(bool booruTestMode = false)
        {
            _PostFetcherImnageList = new List<BasePost>();
            _booruTestMode = booruTestMode;
        }

        public List<BasePost> GetImages(int page, String tags = null)
        {
            _PostFetcherImnageList.Clear();

            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML || GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
                GetXMLImages(_PostFetcherImnageList, tags, page);
            else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.JSON || GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
                GetJSONImages(_PostFetcherImnageList, tags, page);

            return _PostFetcherImnageList;
        }

        #region XML Fetch routines
        private void GetXMLImages(List<BasePost> ImageList, string tags, int page)
        {
            string finalURL = string.Empty;
            if (tags == null)
            {
                tags = string.Empty;
            }

            //Danbooru api based sites
            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML)
            {
                finalURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.xml"; //+ tags from searchfield
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    finalURL = string.Format(finalURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + FormTags(tags));
                else
                    finalURL = string.Format(finalURL + "?page=" + page + "&tags=" + FormTags(tags));
            }
            //Gelbooru api based sites
            else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
            {
                finalURL = GlobalSettings.Instance.CurrentBooru.URL + "index.php?page=dapi&s=post&q=index";
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    finalURL = string.Format(finalURL + "&login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&pid=" + page + "&tags=" + FormTags(tags));
                else
                        finalURL = string.Format(finalURL + "&pid=" + page + "&tags=" + FormTags(tags));
            }

            try
            {
                XmlTextReader reader = new XmlTextReader(finalURL);

                while (reader.Read())
                {

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            var nodeName = reader.Name.ToLowerInvariant();
                            if (nodeName.Equals("posts") && !_booruTestMode)
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
                                        //This is usually just a number, but it really makes no difference here
                                        case "id": post.PostId = reader.Value; break;
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
                                                else if (reader.Value.Contains("e"))
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
                                {
                                    if (UtilityFunctions.GetUrlExtension(post.FullPictureURL) != null)
                                        ImageList.Add(post);
                                }

                            }
                            break;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }

            if (!_booruTestMode && GlobalSettings.Instance.PostsOffset > GlobalSettings.Instance.TotalPosts)
            {
                throw new Exception("End of posts.");
            }
        }

        #endregion

        #region JSON Fetch routines
        private void GetJSONImages(List<BasePost> ImageList, string tags, int page)
        {
            const int Limit = 20;

            string finalURL = string.Empty;
            if (tags == null)
            {
                tags = string.Empty;
            }

            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
            {
                finalURL = GlobalSettings.Instance.CurrentBooru.URL + "posts.json/"; //+ tags from searchfield
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    finalURL = string.Format(finalURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + FormTags(tags));
                else
                    finalURL = string.Format(finalURL + "?page=" + page + "&tags=" + FormTags(tags));
            }
            else
            {
                finalURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.json"; //+ tags from search field
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    finalURL = string.Format(finalURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + FormTags(tags));
                else
                    finalURL = string.Format(finalURL + "?page=" + page + "&tags=" + FormTags(tags));
            }

            try
            {
                StreamReader reader = new StreamReader(System.Net.WebRequest.Create(finalURL).GetResponse().GetResponseStream());

                String json = "";
                json = reader.ReadToEnd();
                reader.Close();

                int actualCount = 0;

                if (json.Length > 4)
                {
                    json = json.Substring(2, json.Length - 4);
                    string[] splitter = { "},{" };
                    string[] split = json.Split(splitter, StringSplitOptions.None);

                    foreach (string str in split)
                    {
                        BasePost post = new BasePost();
                        string[] node = str.Split(',');
                        foreach (string str2 in node)
                        {
                            char[] splitter2 = { ':' };
                            string[] val = str2.Split(splitter2, 2);
                            switch (val[0].ToLowerInvariant())
                            {
                                //This is usually just a number, but it really makes no difference here
                                case "\"id\"":
                                    post.PostId = val[1].Replace("\"", "");
                                    break;
                                case "\"tags\"":
                                    post.Tags = val[1].Replace("\"", "");
                                    break;
                                case "\"file_url\"":
                                    post.FullPictureURL = NormaliseURL(val[1].Replace("\"", ""));
                                    break;
                                case "\"width\"":
                                    int.TryParse(val[1], out post._width);
                                    break;
                                case "\"height\"":
                                    int.TryParse(val[1], out post._height);
                                    break;
                                case "\"rating\"":
                                    if (val[1].Contains("s"))
                                        post.ImageRating = PostRating.Safe;
                                    else if (val[1].Contains("e"))
                                        post.ImageRating = PostRating.Explicit;
                                    else
                                        post.ImageRating = PostRating.Questionable;
                                    break;
                                case "\"md5\"":
                                    post.FileMD = val[1].Replace("\"", "");
                                    break;
                                case "\"preview_url\"":
                                    post.PreviewURL = NormaliseURL(val[1].Replace("\"", ""));
                                    break;
                                // This values have been added in danbooru v2 format so we can safely override some of the post values
                                case "\"tag_string\"": 
                                    post.Tags = val[1].Replace("\"", ""); 
                                    break;
                                case "\"file_ext\"": 
                                    post.FileExtension = val[1].Replace("\"", ""); 
                                    break;
                                case "\"image_width\"": 
                                    int.TryParse(val[1], out post._width); 
                                    break;
                                case "\"image_height\"": 
                                    int.TryParse(val[1], out post._height); 
                                    break;
                                default: break;
                            }
                        }

                        if (GlobalSettings.Instance.IsSafeMode && post.ImageRating != PostRating.Safe)
                        {
                            //Do nothing for now
                            //TODO: add more UI level filtering later!
                        }
                        else
                        {
                            //Check for V2 Danbooru, if it is we need to work out filepaths manually... Thanks for that...
                            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
                            {
                                string fullFilepath = "/data/" + post.FileMD + "." + post.FileExtension;
                                post.FullPictureURL = NormaliseURL(fullFilepath);

                                string thumbFilepath = "/data/preview/" + post.FileMD + ".jpg";
                                post.PreviewURL = NormaliseURL(thumbFilepath); ;
                            }

                            if (UtilityFunctions.GetUrlExtension(post.FullPictureURL) != null)
                                ImageList.Add(post);
                        }

                        actualCount++;
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }

            if (!_booruTestMode)
            {
                GlobalSettings.Instance.PostsOffset = page * Limit;

                //A shitty workaround as json queries don't return total post count from the imageboard
                if (ImageList.Count > 0)
                {
                    GlobalSettings.Instance.TotalPosts = GlobalSettings.Instance.TotalPosts + GlobalSettings.Instance.PostsOffset + 1;
                }

                if (GlobalSettings.Instance.PostsOffset > GlobalSettings.Instance.TotalPosts)
                {
                    throw new Exception("End of posts.");
                }
            }
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

            if (!string.IsNullOrEmpty(tags))
            {
                returnTags = tags.Replace(" ", "+");
            }

            return returnTags;
        }

        #endregion
    }
}
