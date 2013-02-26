using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal void GetImages(List<BasePost> _PostFetcherImnageList, string tags, int page)
        {
            throw new Exception("Sankaku support is not implemented yet.");
        }
    }
}
