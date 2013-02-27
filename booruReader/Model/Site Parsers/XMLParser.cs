using booruReader.Helpers;
using System;
using System.Collections.Generic;
using System.Xml;

namespace booruReader.Model.Site_Parsers
{
    internal class XMLParser
    {
        private bool _booruTestMode;

        internal XMLParser(bool testMode = false)
        {
            _booruTestMode = testMode;
        }

        internal void GetImages(List<BasePost> ImageList, string tags, int page)
        {
            string finalURL = GetRequestURL(tags, page); 

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
                                        case "file_url": post.FullPictureURL = UtilityFunctions.NormaliseURL(reader.Value); break;
                                        case "preview_url": post.PreviewURL = UtilityFunctions.NormaliseURL(reader.Value); break;
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
                                    //Work out file extension at this point to save headaches later on
                                    if (string.IsNullOrEmpty(post.FileExtension))
                                        post.FileExtension = UtilityFunctions.GetUrlExtension(post.FullPictureURL);

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

        private string GetRequestURL(string tags, int page)
        {
            string returnURL = string.Empty;

            //Danbooru api based sites
            if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.XML)
            {
                returnURL = GlobalSettings.Instance.CurrentBooru.URL + "post/index.xml"; //+ tags from searchfield
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    returnURL = string.Format(returnURL + "?login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
                else
                    returnURL = string.Format(returnURL + "?page=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
            }
            //Gelbooru api based sites
            else if (GlobalSettings.Instance.CurrentBooru.ProviderType == ProviderAccessType.Gelbooru)
            {
                returnURL = GlobalSettings.Instance.CurrentBooru.URL + "index.php?page=dapi&s=post&q=index";
                if (!string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.UserName) && !string.IsNullOrEmpty(GlobalSettings.Instance.CurrentBooru.Password))
                    returnURL = string.Format(returnURL + "&login=" + GlobalSettings.Instance.CurrentBooru.UserName + "&password_hash=" + GlobalSettings.Instance.CurrentBooru.Password + "&pid=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
                else
                    returnURL = string.Format(returnURL + "&pid=" + page + "&tags=" + UtilityFunctions.FormTags(tags));
            }

            return returnURL;
        }
    }
}
