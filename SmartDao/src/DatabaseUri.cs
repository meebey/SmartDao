/*
 * $Id$
 * $URL$
 * $Revision$
 * $Author$
 * $Date$
 */

using System;

namespace Meebey.SmartDao
{
    public class DatabaseUri : Uri
    {
        public string Username {
            get {
                if (!UserInfo.Contains(":")) {
                    return UserInfo;
                }
                return UserInfo.Substring(0, UserInfo.IndexOf(":"));
            }
        }
        
        public string Password {
            get {
                if (!UserInfo.Contains(":")) {
                    return null;
                }
                return UserInfo.Substring(UserInfo.IndexOf(":") + 1);
            }
        }
        
        public string DatabaseType {
            get {
                return Scheme;
            }
        }
        
        public string DatabaseName {
            get {
                // remove leading slash
                return AbsolutePath.Substring(1);
            }
        }
        
        public DatabaseUri(string uri) :
                      base(uri)
        {
        }
    }
}
