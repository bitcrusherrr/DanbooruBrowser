using booruReader.Helpers;
using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net;
using System.IO; //Thanks stack overflow :D

namespace booruReader.Model.Site_Parsers
{
    /// <summary>
    /// Sankaku's APIs are either non-functional, unsupported, or require accounts.
    /// So the fallback is to parse HTML.
    /// 
    /// The following is an actual sample HTML tag for an image thumbnail, as of 2015/04/13: (massaged for readability)
    /// 
    /// <span class="thumb blacklisted" id=p4541203>
    ///    <a href="/post/show/4541203" onclick="return PostModeMenu.click(4541203);">
    ///      <img class=preview src="//c.sankakucomplex.com/data/preview/e8/96/e8969a0af93c4ce081f76dc2427c6618.jpg" 
    ///           title="dungeon_ni_deai_wo_motomeru_no_wa_machigatteiru_darou_ka? hestia_(danmachi) yoo_(tabi_no_shiori) 1girl ... white_dress Rating:Safe Score:5.0 Size:565x800 User:System" 
    ///           alt="" width=105 height=150>
    ///    </a>
    /// </span>
    /// 
    /// Note the thumbnail URL (preview src), Tags / Rating / etc in the title, and post id.
    /// 
    /// To get the full-size image URL it is necessary to follow the href link. In the example above, that
    /// would be "http://c.sankakucomplex.com/post/show/4541203". That page must then be parsed to get the
    /// actual full-size image URL.
    /// 
    /// TODO KBR 20150413 Sankaku pools can be accessed via https://chan.sankakucomplex.com/pool/index.xml
    /// 
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

            // KBR 20150413 This can happen if we get a bogus/error page.
            if (images == null)
                return;

            try
            {
                foreach (HtmlNode image in images)
                {
                    BasePost post = new BasePost();
                    if (image.GetAttributeValue("class", "").Contains("thumb"))
                    {
                        post.PostId = image.GetAttributeValue("id", "").Replace("p", "");

                        HtmlNode _image = image.ChildNodes[0].ChildNodes[0];

                        // KBR 20150413 I don't get this: Sankaku currently returns URLs of the form "//c.sankakucomplex.com/data/preview/2b/83/2b833e3c466315fe1360c56af409a02d.jpg" which
                        // I'm munging into "http://..." form.
                        post.PreviewURL = _image.GetAttributeValue("src", "").Replace("//c", "http://c");

                        post.FileMD = Path.GetFileNameWithoutExtension(post.PreviewURL);
                        string tagsAndRating = _image.GetAttributeValue("title", "");
                        if (!string.IsNullOrEmpty(tagsAndRating))
                        {
                            // crashes when string is "Rating" (note capital R)
                            post.Tags = tagsAndRating.Substring(0, tagsAndRating.IndexOf("rating",StringComparison.OrdinalIgnoreCase));

                            if (tagsAndRating.ToLower().Contains("explicit"))
                                post.ImageRating = PostRating.Explicit;
                            else if (tagsAndRating.ToLower().Contains("safe"))
                                post.ImageRating = PostRating.Safe;
                            else
                                post.ImageRating = PostRating.Questionable;
                        }

                        // TODO KBR 20150413 Maybe consider doing this async. Right now it slows down the UI a lot.
                        post.FullPictureURL = GetBigImageURL(post.PostId);
                        post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);

                        if (GlobalSettings.Instance.IsSafeMode && post.ImageRating != PostRating.Safe)
                        {
                            //Do nothing for now
                            //TODO: add more UI level filtering later!
                        }
                        else
                        {
                            // KBR 20150413 These statements break things completely. Sankaku has changed their URLs radically.
                            ////Check for V2 Danbooru, if it is we need to work out filepaths manually... Thanks for that...
                            //if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.DanbooruV2)
                            //{
                            //    string fullFilepath = "/data/" + post.FileMD + "." + post.FileExtension;
                            //    post.FullPictureURL = UtilityFunctions.NormaliseURL(fullFilepath);

                            //    string thumbFilepath = "/data/preview/" + post.FileMD + ".jpg";
                            //    post.PreviewURL = UtilityFunctions.NormaliseURL(thumbFilepath); ;
                            //}

                            // KBR 20150413 No longer necessary: don't munge the URLs.
                            ////Work out file extension at this point to save headaches later on
                            //if (string.IsNullOrEmpty(post.FileExtension))
                            //    post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);

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
            // TODO KBR 20150413 insure the server URL is slash terminated before trying to use it

            string returnpageString = string.Empty;
            // Open a connection
            try
            {
                System.Threading.Thread.Sleep(1000);
                HttpWebRequest WebRequestObject = (HttpWebRequest)WebRequest.Create(Url);

                // You can also specify additional header values like 
                // the user agent or the referer:
                // KBR 20150413 Sankaku is likely to reject unless we do this. 
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
