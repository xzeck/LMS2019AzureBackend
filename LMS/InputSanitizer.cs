using System;
using System.Collections.Generic;
using System.Text;

namespace LMS
{
    class InputSanitizer
    {
        public HashSet<string> BlackListTags = new HashSet<string>()
        {
            { "html" },
            { "script" },
            { "iframe" },
            { "head" },
            { "form" },
            { "link" },
            { "meta" }
        };

        bool SanitizeHTML(string UserInput)
        {
            if (UserInput.StartsWith("<"))
                return false;

            return true;
             
        }
    }
}
