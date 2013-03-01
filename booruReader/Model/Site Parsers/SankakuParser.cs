using booruReader.Helpers;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net;
using System.IO; //Thanks stack overflow :D

namespace booruReader.Model.Site_Parsers
{
    /// <summary>
    /// I dont even know. I guess ill parse html? 
    /// </summary>
    class SankakuParser
    {
        private bool _booruTestMode;

        internal SankakuParser(bool testMode = false)
        {
            _booruTestMode = testMode;
        }

        internal void GetImages(List<BasePost> _PostFetcherImageList, string tags, int page)
        {
            string finalURL = GetRequestURL(tags, page);

            GetImages(_PostFetcherImageList, finalURL);

            if (!_booruTestMode)
            {
                GlobalSettings.Instance.PostsOffset = page * 20;

                //A crappy workaround as json queries don't return total post count from the imageboard
                if (_PostFetcherImageList.Count > 0)
                {
                    GlobalSettings.Instance.TotalPosts = GlobalSettings.Instance.TotalPosts + GlobalSettings.Instance.PostsOffset + 1;
                }

                if (GlobalSettings.Instance.PostsOffset > GlobalSettings.Instance.TotalPosts)
                {
                    throw new Exception("End of posts.");
                }
            }
            // This one is going to be good... We gotta parse html for this.

            /*
             So theoreticly:
             nav past <div id="popular-preview">
              
             No go through each <span class="thumb blacklisted"
             src = link to thumb
             title = tags and will have Rating:whatever
             link obv tyo the main post
             * 
             go to href element for the post
             load that page and get the bug URL from there
             */
        }

        private void GetImages(List<BasePost> _PostFetcherImageList, string pageUrl)
        {
            //This is using the HtmlAgilityPack which basicly makes this similair to XMLDocument
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(GetPage(pageUrl));

            HtmlNode popular = page.DocumentNode.SelectSingleNode("//div[@id='popular-preview']");
            if (popular != null)
                popular.Remove();

            HtmlNodeCollection images = page.DocumentNode.SelectNodes("//span");

            try
            {
                foreach (HtmlNode image in images)
                {
                    BasePost post = new BasePost();
                    if (image.GetAttributeValue("class", "").Contains("thumb"))
                    {
                        post.PostId = image.GetAttributeValue("id", "").Replace("p", "");

                        HtmlNode _image = image.ChildNodes[0].ChildNodes[0];

                        post.PreviewURL = _image.GetAttributeValue("src", "");
                        post.FileMD = Path.GetFileNameWithoutExtension(post.PreviewURL);
                        string tagsAndRating = _image.GetAttributeValue("title", "");
                        if (!string.IsNullOrEmpty(tagsAndRating))
                        {
                            post.Tags = tagsAndRating.Substring(0, tagsAndRating.IndexOf("rating"));

                            if (tagsAndRating.ToLower().Contains("explicit"))
                                post.ImageRating = PostRating.Explicit;
                            else if (tagsAndRating.ToLower().Contains("safe"))
                                post.ImageRating = PostRating.Safe;
                            else
                                post.ImageRating = PostRating.Questionable;
                        }


                        post.FullPictureURL = GetBigImageURL(post.PostId);
                        //post.FullPictureURL = post.PreviewURL;
                        post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);


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
                            if (string.IsNullOrEmpty(post.FileExtension))
                                post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);

                            if (UtilityFunctions.GetUrlExtension(post.FullPictureURL) != null)
                                _PostFetcherImageList.Add(post);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.ToString());
            }
        }

        private string GetBigImageURL(string postID)
        {
            string imageURL = "/post/show/" + postID;
            string returnURL = string.Empty;
            HtmlDocument page = new HtmlDocument();
            page.LoadHtml(GetPage(UtilityFunctions.NormaliseURL(imageURL)));

            HtmlNode image = page.DocumentNode.SelectSingleNode("//a[@id='highres']");
            if (image != null)
                returnURL = image.GetAttributeValue("href", "");

            return returnURL;
        }

        private string GetPage(string Url)
        {
            string returnpageString = string.Empty;
            // Open a connection
            try
            {
                System.Threading.Thread.Sleep(1000);
                HttpWebRequest WebRequestObject = (HttpWebRequest)HttpWebRequest.Create(Url);

                // You can also specify additional header values like 
                // the user agent or the referer:
                WebRequestObject.UserAgent = "Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:22.0) Gecko/20130223 Firefox/22.0"; //This will do I guess...
                WebRequestObject.Referer = "";
                WebResponse Response = WebRequestObject.GetResponse();
                Stream WebStream = Response.GetResponseStream();
                StreamReader Reader = new StreamReader(WebStream);
                returnpageString = Reader.ReadToEnd();
            }
            catch { }

            return returnpageString;
        }

        private string GetRequestURL(string tags, int page)
        {
            string returnURL = string.Empty;

            //Danbooru api based sites
            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Sankaku || GlobalSettings.Instance.CurrentBooru.URL.ToLower().Contains("sankaku"))
            {
                returnURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index"; //+ tags from searchfield
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    returnURL = string.Format(returnURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
                else
                    returnURL = string.Format(returnURL + "?page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
            }

            return returnURL;
        }
    }
}
