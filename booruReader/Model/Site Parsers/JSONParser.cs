using booruReader.Helpers;
using System;
using System.Collections.Generic;
using System.IO;

namespace booruReader.Model.Site_Parsers
{
    internal class JSONParser
    {
        private bool _booruTestMode;

        internal JSONParser(bool testMode = false)
        {
            _booruTestMode = testMode;
        }

        internal void GetImages(List<BasePost> ImageList, string tags, int page)
        {
            string finalURL = GetRequestURL(tags, page);

            try
            {
                StreamReader reader = new StreamReader(System.Net.WebRequest.Create(finalURL).GetResponse().GetResponseStream());

                String json = "";
                json = reader.ReadToEnd();
                reader.Close();

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
                                    post.FullPictureURL = UtilityFunctions.NormaliseURL(val[1].Replace("\"", ""));
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
                                    post.PreviewURL = UtilityFunctions.NormaliseURL(val[1].Replace("\"", ""));
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
                                post.FullPictureURL = UtilityFunctions.NormaliseURL(fullFilepath);

                                string thumbFilepath = "/data/preview/" + post.FileMD + ".jpg";
                                post.PreviewURL = UtilityFunctions.NormaliseURL(thumbFilepath); ;
                            }

                            //Work out file extension at this point to save headaches later on
                            if(string.IsNullOrEmpty(post.FileExtension))
                                post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);

                            if (UtilityFunctions.GetUrlExtension(post.FullPictureURL) != null)
                                ImageList.Add(post);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }

            if (!_booruTestMode)
            {
                GlobalSettings.Instance.PostsOffset = page * 20;

                //A crappy workaround as json queries don't return total post count from the imageboard
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

        private string GetRequestURL(string tags, int page)
        {
            string returnURL = string.Empty;

            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
            {
                returnURL = GlobalSettings.Instance.CurrentBooru.URL + "posts.json/"; //+ tags from searchfield
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    returnURL = string.Format(returnURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
                else
                    returnURL = string.Format(returnURL + "?page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
            }
            else
            {
                returnURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.json"; //+ tags from search field
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    returnURL = string.Format(returnURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
                else
                    returnURL = string.Format(returnURL + "?page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
            }

            return returnURL;
        }
    }
}
