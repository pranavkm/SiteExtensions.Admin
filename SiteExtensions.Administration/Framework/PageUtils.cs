// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace SiteExtensions.Administration
{
    public static class PageUtils
    {
        public static string GetFilterValue(HttpRequestBase request, string cookieName, string key)
        {
            var value = request.QueryString[key];
            if (String.IsNullOrEmpty(value))
            {
                var cookie = request.Cookies[cookieName];
                if (cookie != null)
                {
                    value = cookie[key];
                }
            }
            return value;
        }

        public static void PersistFilter(HttpResponseBase response, string cookieName, IDictionary<string, string> filterItems)
        {
            var cookie = response.Cookies[cookieName];
            if (cookie == null)
            {
                cookie = new HttpCookie(cookieName);
                response.Cookies.Add(cookie);
            }
            foreach (var item in filterItems)
            {
                cookie[item.Key] = item.Value;
            }
        }

        public static bool IsValidLicenseUrl(Uri licenseUri)
        {
            return Uri.UriSchemeHttp.Equals(licenseUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                   Uri.UriSchemeHttps.Equals(licenseUri.Scheme, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Constructs a query string from an IDictionary
        /// </summary>
        public static string BuildQueryString(IDictionary<string, string> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var param in parameters)
            {
                stringBuilder.Append(stringBuilder.Length == 0 ? '?' : '&')
                    .Append(HttpUtility.UrlEncode(param.Key))
                    .Append('=')
                    .Append(HttpUtility.UrlEncode(param.Value));
            }
            return stringBuilder.ToString();
        }
    }
}
